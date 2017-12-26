
#include "Arduino.h"
#include "Adafruit_MCP23017.h"


#define LED_ON HIGH
#define LED_OFF LOW

class Led {
  public:
    int pin;
    byte state;

    Led(Adafruit_MCP23017* _mcp, int _pin);

    void setup();
    void on();
    void onFor(unsigned long duration);
    void off();
    void offFor(unsigned long duration);
    void startBlinking(unsigned long duration);
    void stopBlinking();
    void toggle();
    void update();
    

  private:
    Adafruit_MCP23017* mcp;
    byte nextState;
    unsigned long updateTime;
    unsigned long nextStateDelay;
    bool blink;

    void set(byte value);
};

