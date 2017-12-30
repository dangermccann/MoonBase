
#include "Arduino.h"
#include "Adafruit_MCP23017.h"

#define DEBOUNCE_DELAY 50
#define UNCHANGED 2

class Button {
  public:
    byte state;
    byte pin;
       
    Button(Adafruit_MCP23017* _mcp, byte _pin);
    void setup();
    byte poll();
    
    
  private:
    Adafruit_MCP23017* mcp;
    unsigned long lastDebounceTime;
    byte lastState;
    
    
    
};

