#include <Arduino.h>
#include <Core/RobotUtils.h>
#include <Flow/Task.h>
#include <Comm/MQTTHandler.h>
#include <WiFi.h>
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

MQTTConfig MqttConfig(
    MQTT_BROCKER,
    MQTT_PORT,
    "DLU-01",
    "smartwh/robots/1/telemetry",
    "smartwh/robots/1/task/command"
);

MQTTHandler handler(MqttConfig);

bool moving = false;
bool obstacle = false;

// US Task
Task sensorTask([](){
        while (true) {
            ulTaskNotifyTake(pdTRUE, portMAX_DELAY); 
            // 1 arg is for "what to do when called while slept". Speaking widely - "Should i zero my counter or not"
            // 2 is how long you will wait
            US1.StartScan();
        }
    });

void setup() {
    Serial.begin(115200);

    // SET IN THE ENV VARIABLES!
    WiFi.begin(WIFI_SSID, WIFI_PASS);

    while (WiFi.status() != WL_CONNECTED) {
        delay(500);
    }
    
    Serial.println("WiFi connected");

    handler.Connect();

    handler.Post(
        "smartwh/robots/1/telemetry",
        "{\"robotId\":1,\"stance\":1,\"batteryLevel\":80}",
        1,
        0
    );

    sensorTask.Start("UlSo1", 2048, 1, 1);

    ROBOT.Assemble();
}

unsigned long lastSonarTrigger = 0;

void loop() {
    unsigned long currentMillis = millis();

    if (currentMillis - lastSonarTrigger >= 60){
        xTaskNotifyGive(sensorTask.GetHandle());
        lastSonarTrigger = currentMillis;
    }

    long dist = US1.GetDistance(); // Get "prev." distance

    if (dist <= 0) {
        // Serial.write("Sensor data read incorrectly");
    } else if (dist < 40) {
        ROBOT.FullStop();
    } else {
        ROBOT.MoveForward();
    }
}