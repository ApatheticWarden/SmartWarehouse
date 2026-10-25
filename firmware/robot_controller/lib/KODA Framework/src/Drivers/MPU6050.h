#ifndef MPU6050_H
#include <driver/i2c_master.h>
#define MPU6050_H

struct Vec3 { float x, y, z; };

struct MPU6050Config {
    i2c_port_num_t port;
    gpio_num_t sda;
    gpio_num_t scl;
    uint8_t address;
    uint32_t freqHz;

    MPU6050Config(i2c_port_num_t _port = I2C_NUM_0,
                  gpio_num_t _sda = GPIO_NUM_21,
                  gpio_num_t _scl = GPIO_NUM_47,
                  uint8_t _address = 0x68,
                  uint32_t _freqHz = 400000);
};

class MPU6050{
    MPU6050Config cfg;
    i2c_master_bus_handle_t busHandle = nullptr;
    i2c_master_dev_handle_t devHandle = nullptr;

    esp_err_t CreateBus();
    esp_err_t CreateDev();

    esp_err_t ReadRegs(uint8_t reg, uint8_t* buf, size_t len);
    esp_err_t ReadVec3(uint8_t reg, float scale, Vec3& out);
    public:
        explicit MPU6050(const MPU6050Config& config);
        MPU6050() = delete;
        MPU6050(const MPU6050&) = delete;
        MPU6050& operator=(const MPU6050&) = delete;
        ~MPU6050();

        esp_err_t GetAccel(Vec3& out);   // g
        esp_err_t GetGyro(Vec3& out);    // °/с
        esp_err_t GetTemp(float& out);   // °C

        esp_err_t Init();
};

#endif