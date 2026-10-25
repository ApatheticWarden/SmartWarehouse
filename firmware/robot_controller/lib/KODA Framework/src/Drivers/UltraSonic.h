#ifndef ULTRASONIC_H
#define ULTRASONIC_H
#include <atomic>
#include "driver/gpio.h"
#include "esp_attr.h"

struct UltraSonicPolicy {
    gpio_pullup_t   echo_pull_up   = GPIO_PULLUP_DISABLE;
    gpio_pulldown_t echo_pull_down = GPIO_PULLDOWN_DISABLE;
};

class UltraSonic {
    gpio_num_t TRIGGER_PIN;
    gpio_num_t ECHO_PIN;
    
    std::atomic<int64_t>  echoStartTime{0};
    
    std::atomic<long> currentDistance{-1};

    // ISR
    static void IRAM_ATTR EchoISR(void* arg);
public:
    UltraSonic(gpio_num_t TRIG_PIN, gpio_num_t ECHO_PIN);

    gpio_num_t GetTriggerPin();
    gpio_num_t GetEchoPin();

    void Init(const UltraSonicPolicy &policy);
    void StartScan();
    long GetDistance();
};

#endif