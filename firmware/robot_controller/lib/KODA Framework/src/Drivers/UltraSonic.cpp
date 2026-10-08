#include <Drivers/UltraSonic.h>

constexpr float SOUND_SPEED_CM = 0.0343f;

UltraSonic::UltraSonic(uint8_t TR_PIN, uint8_t ECH_PIN){
    TRIGGER_PIN = TR_PIN;
    ECHO_PIN = ECH_PIN;
}

uint8_t UltraSonic::GetTriggerPin(){
    return TRIGGER_PIN;
}

uint8_t UltraSonic::GetEchoPin(){
    return ECHO_PIN;
}

void UltraSonic::Init(){
    pinMode(TRIGGER_PIN, OUTPUT);
    pinMode(ECHO_PIN, INPUT);
}


// refactor to non-blocking shit
// Try to use interrupts
void UltraSonic::SendTrigg(){
    digitalWrite(TRIGGER_PIN, LOW);
    delayMicroseconds(2);
    digitalWrite(TRIGGER_PIN, HIGH);
    delayMicroseconds(10);
    digitalWrite(TRIGGER_PIN, LOW);
}

long UltraSonic::GetDistance(){    
    return currentDistance.load(std::memory_order_relaxed);
}

long UltraSonic::StartScan(){
    SendTrigg();

    long duration = pulseIn(ECHO_PIN, HIGH, 30000UL);

    if (duration > 0) {
        currentDistance.store(duration / 58, std::memory_order_relaxed);
    } else {
        currentDistance.store(-1, std::memory_order_relaxed);
    }

    return GetDistance();
}