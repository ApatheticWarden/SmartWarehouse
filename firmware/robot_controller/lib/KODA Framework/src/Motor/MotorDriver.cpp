#include <Motor/MotorDriver.h>
#include <Arduino.h>

Motor::Motor(int first, int second){
    INF = first;
    INS = second;
}

uint8_t Motor::GetFirstPin() const{
    return INF;
}

uint8_t Motor::GetSecondPin() const{
    return INS;
}

void Motor::Init(){
    pinMode(INF, OUTPUT);
    pinMode(INS, OUTPUT);
}

void Motor::Move(){
    digitalWrite(INF, HIGH);
    digitalWrite(INS, LOW);
}

void Motor::Stop(){
    digitalWrite(INF, LOW);
    digitalWrite(INS, LOW);
}