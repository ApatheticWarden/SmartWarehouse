#include <Drivers/MPU6050.h>

constexpr uint8_t REG_ACCEL_XOUT_H = 0x3B;
constexpr uint8_t REG_TEMP_OUT_H   = 0x41;
constexpr uint8_t REG_GYRO_XOUT_H  = 0x43;

constexpr float ACCEL_LSB_PER_G   = 16384.0f;  // ±2 g
constexpr float GYRO_LSB_PER_DPS  = 131.0f;    // ±250 °/с

//accel [g]   = raw / 16384.0      ( ±2 g)
//gyro  [°/с] = raw / 131.0        ( ±250 °/с)
//temp  [°C]  = raw / 340.0 + 36.53


MPU6050Config::MPU6050Config(i2c_port_num_t _port, gpio_num_t _sda, gpio_num_t _scl,uint8_t _address, uint32_t _freqHz)
: port(_port), sda(_sda), scl(_scl), address(_address), freqHz(_freqHz) {}

MPU6050::MPU6050(const MPU6050Config& config) : cfg(config) {}

MPU6050::~MPU6050() {
    if (devHandle) i2c_master_bus_rm_device(devHandle);
    if (busHandle) i2c_del_master_bus(busHandle);
}

esp_err_t MPU6050::CreateBus(){
    i2c_master_bus_config_t bus = {};
    bus.i2c_port = cfg.port;
    bus.sda_io_num = cfg.sda;
    bus.scl_io_num = cfg.scl;
    bus.clk_source = I2C_CLK_SRC_DEFAULT;
    bus.glitch_ignore_cnt = 7;
    
    return i2c_new_master_bus(&bus, &busHandle);
}

esp_err_t MPU6050::CreateDev(){
    i2c_device_config_t dev = {};
    dev.dev_addr_length = I2C_ADDR_BIT_LEN_7;
    dev.device_address = cfg.address;
    dev.scl_speed_hz = cfg.freqHz;
    return i2c_master_bus_add_device(busHandle, &dev, &devHandle); 
}

esp_err_t MPU6050::Init(){
    esp_err_t err = CreateBus();
    if(err != ESP_OK) return err;
    err = CreateDev();
    if(err != ESP_OK) return err;
    uint8_t wake[2] = { 0x6B, 0x00 };
    return i2c_master_transmit(devHandle, wake, sizeof(wake), 100);
}


// THIS IS FROM CLAUDE :P
static int16_t Join(uint8_t hi, uint8_t lo) {
    return (int16_t)((hi << 8) | lo);
}

esp_err_t MPU6050::ReadRegs(uint8_t reg, uint8_t* buf, size_t len) {
    if (!devHandle) return ESP_ERR_INVALID_STATE;
    return i2c_master_transmit_receive(devHandle, &reg, 1, buf, len, 100);
}

esp_err_t MPU6050::ReadVec3(uint8_t reg, float scale, Vec3& out) {
    uint8_t buf[6];
    esp_err_t err = ReadRegs(reg, buf, sizeof(buf));
    if (err != ESP_OK) return err;

    out.x = Join(buf[0], buf[1]) / scale;
    out.y = Join(buf[2], buf[3]) / scale;
    out.z = Join(buf[4], buf[5]) / scale;
    return ESP_OK;
}

esp_err_t MPU6050::GetAccel(Vec3& out) {
    return ReadVec3(REG_ACCEL_XOUT_H, ACCEL_LSB_PER_G, out);
}

esp_err_t MPU6050::GetGyro(Vec3& out) {
    return ReadVec3(REG_GYRO_XOUT_H, GYRO_LSB_PER_DPS, out);
}

esp_err_t MPU6050::GetTemp(float& out) {
    uint8_t buf[2];
    esp_err_t err = ReadRegs(REG_TEMP_OUT_H, buf, sizeof(buf));
    if (err != ESP_OK) return err;

    out = Join(buf[0], buf[1]) / 340.0f + 36.53f;
    return ESP_OK;
}