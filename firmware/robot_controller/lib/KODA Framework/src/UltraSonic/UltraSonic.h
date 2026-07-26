#ifndef ULTRASONIC_H
#define ULTRASONIC_H

#include <Arduino.h>
#include <atomic>

class UltraSonic {
    uint8_t TRIGGER_PIN;
    uint8_t ECHO_PIN;
    
    volatile unsigned long echoStartTime = 0;
    
    std::atomic<long> currentDistance{0};

    // ISR
    static void IRAM_ATTR EchoISR(void* arg);

public:
    UltraSonic(uint8_t TRIG_PIN, uint8_t ECHO_PIN);

    uint8_t GetTriggerPin();
    uint8_t GetEchoPin();

    void Init();
    void SendTrigg();
    long GetDistance();
    long StartScan();
};

#endif