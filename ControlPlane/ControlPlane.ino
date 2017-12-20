#include <Wire.h>
#include "Adafruit_MCP23017.h"
#include <Adafruit_GFX.h>
#include "Adafruit_LEDBackpack.h"

/*
    addr 0 = A2 low,  A1 low,  A0 low  000
    addr 1 = A2 low,  A1 low,  A0 high 001
    addr 2 = A2 low,  A1 high, A0 low  010
    addr 3 = A2 low,  A1 high, A0 high 011
    addr 4 = A2 high, A1 low,  A0 low  100
    addr 5 = A2 high, A1 low,  A0 high 101
    addr 6 = A2 high, A1 high, A0 low  110
    addr 7 = A2 high, A1 high, A0 high 111
 */

// Connect pin #12 of the expander to Analog 5 (i2c clock)
// Connect pin #13 of the expander to Analog 4 (i2c data)
// Connect pins #15, 16 and 17 of the expander to ground (address selection)
// Connect pin #9 of the expander to 5V (power)
// Connect pin #10 of the expander to ground (common ground)
// Connect pin #18 through a ~10kohm resistor to 5V (reset pin, active low)

// Output #0 is on pin 21 so connect an LED or whatever from that to ground

Adafruit_MCP23017 mcp0;
Adafruit_MCP23017 mcp1;
Adafruit_AlphaNum4 alpha4 = Adafruit_AlphaNum4();

#define LED_ON HIGH
#define LED_OFF LOW

struct Led {
  int pin;
  byte state;
  byte nextState;
  bool blink;
  unsigned long updateTime;
  unsigned long nextStateDelay;
};

Led LEDs_0[15] = {
  { 1,  LED_ON,  LED_ON,  false },
  { 2,  LED_OFF, LED_OFF, false },
  { 3,  LED_OFF, LED_OFF, false },
  { 4,  LED_OFF, LED_OFF, false },
  { 5,  LED_OFF, LED_OFF, false },
  { 6,  LED_OFF, LED_OFF, false },
  { 7,  LED_OFF, LED_OFF, false },
  { 8,  LED_OFF, LED_OFF, false },
  { 9,  LED_OFF, LED_OFF, false },
  { 10, LED_OFF, LED_OFF, false },
  { 11, LED_OFF, LED_OFF, false },
  { 12, LED_OFF, LED_OFF, false },
  { 13, LED_OFF, LED_OFF, false },
  { 14, LED_OFF, LED_OFF, false },
  { 15, LED_OFF, LED_OFF, false },
};
#define LEDs_0_Size sizeof(LEDs_0) / sizeof(Led)

Led LEDs_1[15] = {
  { 1,  LED_ON,  LED_ON,  false },
  { 2,  LED_OFF, LED_OFF, false },
  { 3,  LED_OFF, LED_OFF, false },
  { 4,  LED_OFF, LED_OFF, false },
  { 5,  LED_OFF, LED_OFF, false },
  { 6,  LED_OFF, LED_OFF, false },
  { 7,  LED_OFF, LED_OFF, false },
  { 8,  LED_OFF, LED_OFF, false },
  { 9,  LED_OFF, LED_OFF, false },
  { 10, LED_OFF, LED_OFF, false },
  { 11, LED_OFF, LED_OFF, false },
  { 12, LED_OFF, LED_OFF, false },
  { 13, LED_OFF, LED_OFF, false },
  { 14, LED_OFF, LED_OFF, false },
  { 15, LED_OFF, LED_OFF, false },
};
#define LEDs_1_Size sizeof(LEDs_1) / sizeof(Led)


void initLEDs(Led leds[], int count, Adafruit_MCP23017& _mcp) {
  unsigned long current = millis();
  
  for (int i = 0; i < count; i++) {
    Led led = leds[i];
    _mcp.pinMode(led.pin , OUTPUT);
    _mcp.digitalWrite(led.pin, led.state);
    led.updateTime = current;
  }
}

void ledOn(Led& led, Adafruit_MCP23017& _mcp) {
  ledSet(led, _mcp, LED_ON);
}

void ledsOn(Led leds[], int count, Adafruit_MCP23017& _mcp) {
  for (int i = 0; i < count; i++) {
    ledOn(leds[i], _mcp);
  }
}

void ledOnFor(Led& led, Adafruit_MCP23017& _mcp, unsigned long duration) {
  ledOn(led, _mcp);
  led.nextStateDelay = duration;
  led.nextState = LED_OFF;
}


void ledOff(Led& led, Adafruit_MCP23017& _mcp) {
  ledSet(led, _mcp, LED_OFF);
}

void ledsOff(Led leds[], int count, Adafruit_MCP23017& _mcp) {
  for (int i = 0; i < count; i++) {
    ledOff(leds[i], _mcp);
  }
}


void ledOffFor(Led& led, Adafruit_MCP23017& _mcp, unsigned long duration) {
  ledOff(led, _mcp);
  led.nextStateDelay = duration;
  led.nextState = LED_ON;
}

void ledSet(Led& led, Adafruit_MCP23017& _mcp, byte state) {
  led.state = state;
  led.nextState = state;
  led.updateTime = millis();
  led.nextStateDelay = 0;
  led.blink = false;
  _mcp.digitalWrite(led.pin, led.state);
}

void ledBlink(Led& led, Adafruit_MCP23017& _mcp, unsigned long duration) {
  ledOn(led, _mcp);
  led.nextState = LED_OFF;
  led.nextStateDelay = duration;
  led.blink = true;
}

void ledsBlink(Led leds[], int count, Adafruit_MCP23017& _mcp, unsigned long duration) {
  for (int i = 0; i < count; i++) {
    ledBlink(leds[i], _mcp, duration);
  }
}

void ledUpdate(Led& led, Adafruit_MCP23017& _mcp) {

  if(led.state != led.nextState) {
    unsigned long current = millis();
    if(current - led.updateTime > led.nextStateDelay) {
      if(led.blink) {
        byte next = led.state;
        led.state = led.nextState;
        led.nextState = next;
        led.updateTime = millis();
        _mcp.digitalWrite(led.pin, led.state);
      }
      else {
        ledSet(led, _mcp, led.nextState);
      }
    }
  }
}

void ledsUpdate(Led leds[], int count, Adafruit_MCP23017& _mcp) {
  for (int i = 0; i < count; i++) {
    ledUpdate(leds[i], _mcp);
  }
}


void alphaWriteString(char str[], Adafruit_AlphaNum4& _alpha) {
  _alpha.writeDigitAscii(0, str[0]);
  _alpha.writeDigitAscii(1, str[1]);
  _alpha.writeDigitAscii(2, str[2]);
  _alpha.writeDigitAscii(3, str[3]);
  _alpha.writeDisplay();
}

void alphaClear(Adafruit_AlphaNum4& _alpha) {
  alphaWriteString("    ", _alpha);
}


void systemDiagnostics() {
  alphaWriteString("TEST", alpha4);
  
  for(int i = 0; i < 3; i++) {
    ledsOn(LEDs_0, LEDs_0_Size, mcp0);
    ledsOn(LEDs_1, LEDs_1_Size, mcp1);
    delay(3000);
    
    ledsOff(LEDs_0, LEDs_0_Size, mcp0);
    ledsOff(LEDs_1, LEDs_1_Size, mcp1);
    delay(500);
  }

  alphaClear(alpha4);

  for (int i = 0; i < LEDs_0_Size; i++) {
    ledOn(LEDs_0[i], mcp0);
    delay(250);
  }
  ledsOff(LEDs_0, LEDs_0_Size, mcp0);

  for (int i = 0; i < LEDs_1_Size; i++) {
    ledOn(LEDs_1[i], mcp1);
    delay(250);
  }
  ledsOff(LEDs_1, LEDs_1_Size, mcp1);

  alphaWriteString("X   ", alpha4);
  delay(250);
  alphaWriteString(" X  ", alpha4);
  delay(250);
  alphaWriteString("  X ", alpha4);
  delay(250);
  alphaWriteString("   X", alpha4);
  delay(250);
  alphaClear(alpha4);
}


void setup() {  
  Serial.begin(9600);
  
  mcp0.begin(0);
  mcp0.pinMode(0, INPUT);
  mcp0.pullUp(0, HIGH);
  
  initLEDs(LEDs_0, LEDs_0_Size, mcp0);
  ledsOff(LEDs_0, LEDs_0_Size, mcp0);


  mcp1.begin(1);
  mcp1.pinMode(0, INPUT);
  mcp1.pullUp(0, HIGH);
  
  initLEDs(LEDs_1, LEDs_1_Size, mcp1);
  ledsOff(LEDs_1, LEDs_1_Size, mcp1);



  alpha4.begin(0x70);  // pass in the address
  alphaClear(alpha4);
}


// flip the pin #0 up and down

void loop() {

  ledsUpdate(LEDs_0, LEDs_0_Size, mcp0);
  ledsUpdate(LEDs_1, LEDs_1_Size, mcp1);

  if(mcp0.digitalRead(0) == LOW) {
    ledOnFor(LEDs_0[4], mcp0, 1500);
    ledOnFor(LEDs_0[3], mcp0, 700);
    //alphaWriteString("8888", alpha4);
    //ledsBlink(LEDs_0, LEDs_0_Size, mcp0, 1000);
    //ledsBlink(LEDs_1, LEDs_1_Size, mcp1, 1000);
  }

  if(mcp1.digitalRead(0) == LOW) {
    systemDiagnostics();
  }
  
}
