
#ifndef Button_h
#define Button_h

#include "Button.h"

Button::Button(Adafruit_MCP23017* _mcp, int _pin) {
  mcp = _mcp;
  pin = _pin;
}

void Button::setup() {
  mcp->pinMode(pin, INPUT);
  mcp->pullUp(pin, HIGH);
  state = mcp->digitalRead(pin);
  lastState = state;
  lastDebounceTime = 0;
}

byte Button::poll() {
  // read the state of the switch into a local variable:
  int reading = mcp->digitalRead(pin);

  // check to see if you just pressed the button
  // (i.e. the input went from LOW to HIGH), and you've waited long enough
  // since the last press to ignore any noise:

  // If the switch changed, due to noise or pressing:
  if (reading != lastState) {
    // reset the debouncing timer
    lastDebounceTime = millis();
  }

  // save the reading. Next time through the loop, it'll be the lastButtonState:
  lastState = reading;

  if ((millis() - lastDebounceTime) > DEBOUNCE_DELAY) {
    // whatever the reading is at, it's been there for longer than the debounce
    // delay, so take it as the actual current state:

    // if the button state has changed:
    if (reading != state) {
      state = reading;
      return state;
    }
  }

  return UNCHANGED;
}

#endif 
