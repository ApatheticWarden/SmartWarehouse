#include <Drivers/UltraSonic.h>
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include "esp_timer.h"
#include "esp_rom_sys.h"

constexpr float SOUND_SPEED_CM = 0.0343f;

// notice IRAM_ATTR. Places code into IRAM instead of flash
void IRAM_ATTR UltraSonic::EchoISR(void* arg){
    auto* self = static_cast<UltraSonic*>(arg);
    uint32_t now = (uint32_t)esp_timer_get_time();

    if (gpio_get_level(self->ECHO_PIN)) {
       self->echoStartTime.store(now, std::memory_order_relaxed);
    } else {
        uint32_t start = self->echoStartTime.load(std::memory_order_relaxed);
        uint32_t duration = now - start;
        if (start > 0 && duration > 0 && duration < 38000) {
            self->currentDistance.store(duration / 58, std::memory_order_relaxed);
        } else {
            self->currentDistance.store(-1, std::memory_order_relaxed);
        }
    }
}

UltraSonic::UltraSonic(gpio_num_t TR_PIN, gpio_num_t ECH_PIN){
    TRIGGER_PIN = TR_PIN;
    ECHO_PIN = ECH_PIN;
}

gpio_num_t UltraSonic::GetTriggerPin(){
    return TRIGGER_PIN;
}

gpio_num_t UltraSonic::GetEchoPin(){
    return ECHO_PIN;
}


// For future me:
// if you ever will forget howthishitworks - remember you set isr on pull_up of Echo pin.
// when current applied -> EchoISR called
void UltraSonic::Init(const UltraSonicPolicy &policy){
    gpio_reset_pin(TRIGGER_PIN);
    gpio_reset_pin(ECHO_PIN);

    gpio_config_t trig = {};
    trig.pin_bit_mask = 1ULL << TRIGGER_PIN;
    trig.mode = GPIO_MODE_OUTPUT;

    gpio_config_t echo = {};
    echo.pin_bit_mask = 1ULL << ECHO_PIN;
    echo.mode = GPIO_MODE_INPUT;
    echo.pull_up_en = policy.echo_pull_up;
    echo.pull_down_en = policy.echo_pull_down;
    echo.intr_type = GPIO_INTR_ANYEDGE;

    ESP_ERROR_CHECK(gpio_config(&trig));
    gpio_set_level(TRIGGER_PIN, 0); // savety shit
    ESP_ERROR_CHECK(gpio_config(&echo));

    // service assembly
    esp_err_t err = gpio_install_isr_service(0);
    if(err != ESP_OK && err != ESP_ERR_INVALID_STATE) ESP_ERROR_CHECK(err);

    // Remark: esp_err_t gpio_isr_handler_add(gpio_num_t gpio_num, gpio_isr_t isr_handler, void *args)
    ESP_ERROR_CHECK(gpio_isr_handler_add(ECHO_PIN, EchoISR, this));
}

void UltraSonic::StartScan() {
    gpio_set_level(TRIGGER_PIN, 0);
    esp_rom_delay_us(2);
    gpio_set_level(TRIGGER_PIN, 1);
    esp_rom_delay_us(10);   // mcs
    gpio_set_level(TRIGGER_PIN, 0);
}

long UltraSonic::GetDistance(){    
    return currentDistance.load(std::memory_order_relaxed);
}