#include <cstring>

#include "esp_log.h"
#include "esp_event.h"
#include "esp_netif.h"
#include "nvs_flash.h"
#include "esp_wifi.h"

#include "freertos/FreeRTOS.h"
#include "freertos/task.h"

#include <Net/WiFiHandler.h>
#include <Comm/MQTTHandler.h>
#include <Flow/Task.h>
#include <Core/RobotUtils.h>
#include <Drivers/MPU6050.h>

// US
constexpr gpio_num_t TRIGGER_PIN = GPIO_NUM_42;
constexpr gpio_num_t ECHO_PIN = GPIO_NUM_41;

// Motors
constexpr gpio_num_t IN1 = GPIO_NUM_4;
constexpr gpio_num_t IN2 = GPIO_NUM_5;
constexpr gpio_num_t IN3 = GPIO_NUM_6;
constexpr gpio_num_t IN4 = GPIO_NUM_7;

// MPU6050
constexpr i2c_port_num_t I2C_PORT = I2C_NUM_0;
constexpr gpio_num_t SDA = GPIO_NUM_21;
constexpr gpio_num_t SCL = GPIO_NUM_47;

const char* tel_topic = "smartwh/robots/1/telemetry";
const char* task_topic = "smartwh/robots/1/task/update"; 

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

constexpr long STOP_CM   = 20;
constexpr long RESUME_CM = 25;

// Entry-point
extern "C" void app_main(void) {
    vTaskDelay(pdMS_TO_TICKS(2000));

    Mainframe.Assemble();   // todo: return esp_err_t, so can be checked ESP_ERROR_CHECK(*);
                            // But for now -> may the machine-god bless you
    
    // Creating cfg
    MPU6050Config cfg(I2C_PORT, SDA, SCL);
    MPU6050 imu(cfg);

    if(imu.Init() != ESP_OK){
        ESP_LOGE(TAG, "INIT FAILED");
        return;
    }

    static WiFiHandler wifi(WiFiConfig(WIFI_SSID, WIFI_PASS));
    // Init sensortask
    sensorTask.Start("US-T");
    
    ESP_ERROR_CHECK(wifi.Connect());
    ESP_LOGI(TAG, "CONNECTED TO WIFI");
    
    static MQTTHandler mqtt(MQTTConfig(MQTT_BROCKER, MQTT_PORT, "1", tel_topic, task_topic));
    ESP_ERROR_CHECK(mqtt.Connect());
    ESP_LOGI(TAG, "INITIALISATION COMPLETE");

    while (1){
        // Vec3 a, g;
        // if (imu.GetAccel(a) == ESP_OK && imu.GetGyro(g) == ESP_OK) {
        //     ESP_LOGI("main", "a: %.2f %.2f %.2f | g: %.1f %.1f %.1f",
        //              a.x, a.y, a.z, g.x, g.y, g.z);
        // }

        long dist = US.GetDistance();

        if (dist < 0) {                      
            if (moving) { Mainframe.FullStop(); moving = false; }
            vTaskDelay(pdMS_TO_TICKS(100));
            continue;   // THIS SKIPS THE DELAY FOR MPU!
        }

        if (moving && dist > 0 && dist <= STOP_CM) {
            Mainframe.FullStop();
            moving = false;
        } else if (!moving && dist > RESUME_CM) {
            Mainframe.MoveForward();
            moving = true;
        }
        vTaskDelay(pdMS_TO_TICKS(100));
    }
}