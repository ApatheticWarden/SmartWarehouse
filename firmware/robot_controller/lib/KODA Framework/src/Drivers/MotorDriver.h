#ifndef MOTORDRIVER_H
#define MOTORDRIVER_H
#include <cstdint>
#include "driver/gpio.h"

class Motor{
    gpio_num_t INF;
    gpio_num_t INS;
    public:
        // Getters        
        gpio_num_t GetFirstPin() const;
        gpio_num_t GetSecondPin() const;

        // Declare constructor
        Motor(gpio_num_t first, gpio_num_t second);

        // Declare funcs
        void Init();
        void Move();
        void Stop();
}; 

#endif

// https://docs.espressif.com/projects/arduino-esp32/en/latest/api/ledc.html