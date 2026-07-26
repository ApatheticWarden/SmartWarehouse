#include <Arduino.h>
#include <Utils/RobotUtils.h>
#include <Utils/Timer.h>

// constexpr - must be init at compile time
// const - in compile or runtime
constexpr uint8_t TRIGGER_PIN = 42;
constexpr uint8_t ECHO_PIN = 41;
constexpr uint8_t IN1 = 4;
constexpr uint8_t IN2 = 5;
constexpr uint8_t IN3 = 6;
constexpr uint8_t IN4 = 7;

Motor Left(IN1, IN2);
Motor Right(IN3, IN4);

UltraSonic US1(TRIGGER_PIN, ECHO_PIN);

Robot ROBOT(Left, Right, US1);

bool moving = false;
bool obstacle = false;

void setup() {
    Serial.begin(115200);
    ROBOT.Assemble();
}

unsigned long lastSonarTrigger = 0;

void loop() {
    unsigned long currentMillis = millis();

    if (currentMillis - lastSonarTrigger >= 60){
        ROBOT.US.SendTrigg();
        lastSonarTrigger = currentMillis;
    }

    long dist = ROBOT.CheckDistance(); // Get "prev." distance

    if( dist > 0 && dist < 30){
        Serial.print("OBSTACLE");
        ROBOT.FullStop();
    }else{
        ROBOT.MoveForward();
    }

}