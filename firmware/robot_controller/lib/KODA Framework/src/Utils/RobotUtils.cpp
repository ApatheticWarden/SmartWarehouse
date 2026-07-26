#include <Utils/RobotUtils.h>

Robot::Robot(Motor& LMotor, Motor& RMotor, UltraSonic& US):
    LeftMotor(LMotor.GetFirstPin(), LMotor.GetSecondPin()),
    RightMotor(RMotor.GetFirstPin(), RMotor.GetSecondPin()),
    US(US.GetTriggerPin(), US.GetEchoPin())
    {}

void Robot::Assemble(){
    LeftMotor.Init();
    RightMotor.Init();
    US.Init();
}

void Robot::MoveForward(){
    LeftMotor.Move();
    RightMotor.Move();
}

void Robot::FullStop(){
    LeftMotor.Stop();
    RightMotor.Stop();
}

long Robot::CheckDistance(){
    return US.StartScan();
}

void Robot::TurnLeft(){
    LeftMotor.Stop();
    RightMotor.Move();
}

void Robot::TurnRight(){
    LeftMotor.Move();
    RightMotor.Stop();
}