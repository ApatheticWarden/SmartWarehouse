#ifndef MOTORDRIVER_H
#include <cstdint>
#define MOTORDRIVER_H

class Motor{
    uint8_t INF;
    uint8_t INS;
    public:
        // Getters        
        uint8_t GetFirstPin() const;
        uint8_t GetSecondPin() const;

        // Declare constructor
        Motor(int first, int second);

        // Declare funcs
        void Init();
        void Move();
        void Stop();
}; 

#endif

// https://docs.espressif.com/projects/arduino-esp32/en/latest/api/ledc.html