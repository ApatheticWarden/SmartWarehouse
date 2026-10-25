#include <cstring>

#include "esp_log.h"
#include "esp_event.h"
#include "esp_netif.h"
#include "nvs_flash.h"

#include "freertos/FreeRTOS.h"
#include "freertos/task.h"

#include <Flow/Task.h>
#include <Core/RobotUtils.h>

constexpr gpio_num_t TRIGGER_PIN = GPIO_NUM_42;
constexpr gpio_num_t ECHO_PIN = GPIO_NUM_41;
constexpr gpio_num_t IN1 = GPIO_NUM_4;
constexpr gpio_num_t IN2 = GPIO_NUM_5;
constexpr gpio_num_t IN3 = GPIO_NUM_6;
constexpr gpio_num_t IN4 = GPIO_NUM_7;

static const char* TAG = "main";

Motor MLeft(IN1, IN2);
Motor MRight(IN3, IN4);
UltraSonic US(TRIGGER_PIN, ECHO_PIN);

Task sensorTask([]{
    TickType_t last = xTaskGetTickCount();  // this thingy is for carrying cycle work time for each iter
    while (true) {
        US.StartScan();
        vTaskDelayUntil(&last, pdMS_TO_TICKS(60));  // every 60 ms (6 ticks)
    }
});

Robot Mainframe(MLeft,MRight,US);

// КОСТЫЛИ :D
bool obst = false;
bool moving = false;
long lastSonarTime = 0;
// 

// Entry-point
extern "C" void app_main(void) {
    vTaskDelay(pdMS_TO_TICKS(2000));

    Mainframe.Assemble();   // todo: return esp_err_t, so can be checked ESP_ERROR_CHECK(*);
                            // But for now -> may the machine-god bless you

    // Init sensortask
    sensorTask.Start("US-T");
    
    while (1){
        vTaskDelay(pdMS_TO_TICKS(20));
        long dist = US.GetDistance();   // todo: make an getter, or change da arch. Bc this is shitty to use this type of call

        // shitty, but for first test enough
        if(obst) continue;
        if(dist <= -1){
            Mainframe.FullStop();   // todo: decide if -1 is a err
        }else if (dist <= 20){ // in cm
            // stop da roomba
            obst = true;
            Mainframe.FullStop();
            moving = false;
        }else if (dist > 20 && !moving) {
            Mainframe.MoveForward();
            moving = true;
        }
    }
}