#include <Wire.h>
#include "Adafruit_MCP23017.h"
#include <Adafruit_GFX.h>
#include "Adafruit_LEDBackpack.h"
#include "Button.h"
#include "Led.h"

/*
 *  -- MCP23017 Address Selection --
 *  addr 0 = A2 low,  A1 low,  A0 low  000
 *  addr 1 = A2 low,  A1 low,  A0 high 001
 *  addr 2 = A2 low,  A1 high, A0 low  010
 *  addr 3 = A2 low,  A1 high, A0 high 011
 *  addr 4 = A2 high, A1 low,  A0 low  100
 *  addr 5 = A2 high, A1 low,  A0 high 101
 *  addr 6 = A2 high, A1 high, A0 low  110
 *  addr 7 = A2 high, A1 high, A0 high 111
 */

/*
 *  -- MCP23017 Pin Assignment --
 *  Connect pin #12 of the expander to Analog 5 (i2c clock)
 *  Connect pin #13 of the expander to Analog 4 (i2c data)
 *  Connect pins #15, 16 and 17 of the expander to ground or 5V (address selection)
 *  Connect pin #9 of the expander to 5V (power)
 *  Connect pin #10 of the expander to ground (common ground)
 *  Connect pin #18 through a ~10kohm resistor to 5V (reset pin, active low)
 */

/*
 * -- ATmega328P Pin Assignment -- 
 * Connect Vin and UBS to 5V
 * Connect both GND pins to Ground
 * A4 and A5 are I2C bus
 */

Adafruit_MCP23017 mcp0;
Adafruit_MCP23017 mcp1;
Adafruit_AlphaNum4 alpha4 = Adafruit_AlphaNum4();



#define LEDs_0_Size 14
Led LEDs_0[LEDs_0_Size] = {
  Led(&mcp0, 1),
  Led(&mcp0, 2),
  Led(&mcp0, 3),
  Led(&mcp0, 4),
  Led(&mcp0, 5),
  Led(&mcp0, 6),
  Led(&mcp0, 7),
  Led(&mcp0, 8),
  Led(&mcp0, 9),
  Led(&mcp0, 10),
  Led(&mcp0, 11),
  Led(&mcp0, 12),
  Led(&mcp0, 13),
  Led(&mcp0, 14),
};


#define LEDs_1_Size 15
Led LEDs_1[LEDs_1_Size] = {
  Led(&mcp1, 1),
  Led(&mcp1, 2),
  Led(&mcp1, 3),
  Led(&mcp1, 4),
  Led(&mcp1, 5),
  Led(&mcp1, 6),
  Led(&mcp1, 7),
  Led(&mcp1, 8),
  Led(&mcp1, 9),
  Led(&mcp1, 10),
  Led(&mcp1, 11),
  Led(&mcp1, 12),
  Led(&mcp1, 13),
  Led(&mcp1, 14),
  Led(&mcp1, 15),
};

Button b0 = Button(&mcp0, 15);
Button b1 = Button(&mcp1, 0);


void setupLEDs(Led leds[], int count) {
  for (int i = 0; i < count; i++) {
    Led led = leds[i];
    led.setup();
  }
}

void ledsOn(Led leds[], int count) {
  for (int i = 0; i < count; i++) {
    leds[i].on();
  }
}

void ledsOff(Led leds[], int count) {
  for (int i = 0; i < count; i++) {
    leds[i].off();
  }
}

void ledsBlink(Led leds[], int count, Adafruit_MCP23017& _mcp, unsigned long duration) {
  for (int i = 0; i < count; i++) {
    leds[i].startBlinking(duration);
  }
}

void ledsUpdate(Led leds[], int count, Adafruit_MCP23017& _mcp) {
  for (int i = 0; i < count; i++) {
    leds[i].update();
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
    ledsOn(LEDs_0, LEDs_0_Size);
    ledsOn(LEDs_1, LEDs_1_Size);
    delay(3000);
    
    ledsOff(LEDs_0, LEDs_0_Size);
    ledsOff(LEDs_1, LEDs_1_Size);
    delay(500);
  }

  alphaClear(alpha4);

  for (int i = 0; i < LEDs_0_Size; i++) {
    LEDs_0[i].on();
    delay(250);
  }
  ledsOff(LEDs_0, LEDs_0_Size);

  for (int i = 0; i < LEDs_1_Size; i++) {
    LEDs_1[i].on();
    delay(250);
  }
  ledsOff(LEDs_1, LEDs_1_Size);

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
  setupLEDs(LEDs_0, LEDs_0_Size);
  ledsOff(LEDs_0, LEDs_0_Size);


  mcp1.begin(1);
  setupLEDs(LEDs_1, LEDs_1_Size);
  ledsOff(LEDs_1, LEDs_1_Size);



  alpha4.begin(0x70);  // pass in the address
  alphaClear(alpha4);

  //systemDiagnostics();  

  b0.setup();
  b1.setup();
}

void loop() {  

  ledsUpdate(LEDs_0, LEDs_0_Size, mcp0);
  ledsUpdate(LEDs_1, LEDs_1_Size, mcp1);

  byte b1val = b0.poll();
  if(b1val == LOW) {
    LEDs_0[1].on();
    LEDs_0[2].startBlinking(250);
  }
  else if(b1val == HIGH) {
    LEDs_0[1].off();
    LEDs_0[2].stopBlinking();
  }

  if(b1.poll() != UNCHANGED) {
    LEDs_1[6].toggle();
  }

  // Read joystic values - returns value between 0 and 1023
  // analogRead(A0); 
  //char buff[4];
  //itoa(analogRead(A0), buff, 10);
  //alphaWriteString(buff, alpha4);

}
