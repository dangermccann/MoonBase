
#ifndef Led_h
#define Led_h

#include "Led.h"

Led::Led(Adafruit_MCP23017* _mcp, byte _pin) {
  mcp = _mcp;
  pin = _pin;
  state = LED_OFF;
}

void Led::setup() {
  if(mcp != NULL) {
    mcp->pinMode(pin , OUTPUT);
    mcp->digitalWrite(pin, state);
  }
  else {
    pinMode(pin , OUTPUT);
    digitalWrite(pin, state);
  }
  updateTime = millis();
}
void Led::on() { 
  set(LED_ON);
}

void Led::onFor(unsigned long duration) {
  on();
  nextStateDelay = duration;
  nextState = LED_OFF;
}


void Led::off() {
  set(LED_OFF);  
}

void Led::offFor(unsigned long duration) {
  off();
  nextStateDelay = duration;
  nextState = LED_ON;  
}

void Led::startBlinking(unsigned long duration) {
  on();
  nextState = LED_OFF;
  nextStateDelay = duration;
  blink = true;
}

void Led::stopBlinking() {
  off();
}

void Led::toggle() {
  if(state == LED_ON)
    off();
  else
    on();
}

void Led::update() {
  if(state != nextState) {
    unsigned long current =  millis();
    if(current - updateTime > nextStateDelay) {
      if(blink) {
        byte next = state;
        state = nextState;
        nextState = next;
        updateTime = millis();

        if(mcp != NULL)
          mcp->digitalWrite(pin, state);
        else
          digitalWrite(pin, state);
      }
      else {
        set(nextState);
      }
    }
  }
}

void Led::set(byte value) {
  state = value;
  nextState = state;
  updateTime = millis();
  nextStateDelay = 0;
  blink = false;

  if(mcp != NULL)
    mcp->digitalWrite(pin, state);
  else
    digitalWrite(pin, state);
}

#endif
