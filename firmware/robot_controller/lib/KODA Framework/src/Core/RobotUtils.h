#ifndef ROBOTUTILS_H
#include <Drivers/MotorDriver.h>
#include <Drivers/UltraSonic.h>
#include <Flow/Timer.h>
#define ROBOTUTILS_H

class Robot{
    Motor LeftMotor;
    Motor RightMotor;
    UltraSonic US;
    public:
        Robot(Motor& L, Motor& R, UltraSonic& US);

        void Assemble();

        long CheckDistance();
        void MoveForward();
        // void Turn(float deg); 
        // I want to do truning by degrees, but we need encoder (we dont have any)
        void TurnLeft();
        void TurnRight();
        void FullStop();
};

#endif