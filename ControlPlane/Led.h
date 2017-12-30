
#include "Arduino.h"
#include "Adafruit_MCP23017.h"


#define LED_ON HIGH
#define LED_OFF LOW

class Led {
  public:
    byte pin;
    byte state;

    Led(Adafruit_MCP23017* _mcp, byte _pin);

    void setup();
    void on();
    void onFor(unsigned long duration);
    void off();
    void offFor(unsigned long duration);
    void startBlinking(unsigned long duration);
    void stopBlinking();
    void toggle();
    void set(byte value);
    void update();
        

  private:
    Adafruit_MCP23017* mcp;
    byte nextState;
    unsigned long updateTime;
    unsigned long nextStateDelay;
    bool blink;
};

