#include <Core/RobotUtils.h>

Robot::Robot(Motor& LMotor, Motor& RMotor, UltraSonic& us):
    LeftMotor(LMotor),
    RightMotor(RMotor),
    US(us)
    {}

void Robot::Assemble(){
    LeftMotor.Init();
    RightMotor.Init();

    UltraSonicPolicy policy = {};

    US.Init(policy);
}

void Robot::MoveForward(){
    LeftMotor.Move();
    RightMotor.Move();
}

void Robot::FullStop(){
    LeftMotor.Stop();
    RightMotor.Stop();
}

long Robot::CheckDistance() {
    US.StartScan();
    return US.GetDistance();
}

void Robot::TurnLeft(){
    LeftMotor.Stop();
    RightMotor.Move();
}

void Robot::TurnRight(){
    LeftMotor.Move();
    RightMotor.Stop();
}