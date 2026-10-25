#include <Drivers/MotorDriver.h>

Motor::Motor(gpio_num_t first, gpio_num_t second){
    INF = first;
    INS = second;
}

gpio_num_t Motor::GetFirstPin() const{
    return INF;
}

gpio_num_t Motor::GetSecondPin() const{
    return INS;
}

void Motor::Init(){
    gpio_reset_pin(INF);
    gpio_reset_pin(INS);

    // dont need cfg as we got in UltraSonic. completely usless in this usecase 
    gpio_config_t cfg = {};
    cfg.pin_bit_mask = 1ULL << INF | 1ULL << INS;
    cfg.mode = GPIO_MODE_OUTPUT;

    ESP_ERROR_CHECK(gpio_config(&cfg));
    // remember the safety
    gpio_set_level(INF, 0);
    gpio_set_level(INS, 0);
}

void Motor::Move(){
    gpio_set_level(INF, 1);
    gpio_set_level(INS, 0);
}

void Motor::Stop(){
    gpio_set_level(INF, 0);
    gpio_set_level(INS, 0);
}