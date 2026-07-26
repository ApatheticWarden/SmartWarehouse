#include <UltraSonic/UltraSonic.h>

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

    attachInterruptArg(digitalPinToInterrupt(ECHO_PIN), EchoISR, this, CHANGE);
}

void IRAM_ATTR UltraSonic::EchoISR(void* arg) {
    // Restore pointer to our instance
    UltraSonic* instance = static_cast<UltraSonic*>(arg);
    unsigned long now = micros();

    if (digitalRead(instance->ECHO_PIN) == HIGH) {
        // Catch echo start
        instance->echoStartTime = now;
    } else {
        // Catch echo end
        unsigned long duration = now - instance->echoStartTime;
        
        // convert to centimeters and store
        instance->currentDistance.store(duration / 58, std::memory_order_relaxed);
    }
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
  
    return GetDistance();
}