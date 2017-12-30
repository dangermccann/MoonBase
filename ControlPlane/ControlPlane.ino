#include "ControlPlane.h"
#include <Wire.h>
#include "Adafruit_MCP23017.h"
#include <Adafruit_GFX.h>
#include "Adafruit_LEDBackpack.h"
#include "Button.h"
#include "Led.h"

/*
    -- MCP23017 Address Selection --
    addr 0 = A2 low,  A1 low,  A0 low  000
    addr 1 = A2 low,  A1 low,  A0 high 001
    addr 2 = A2 low,  A1 high, A0 low  010
    addr 3 = A2 low,  A1 high, A0 high 011
    addr 4 = A2 high, A1 low,  A0 low  100
    addr 5 = A2 high, A1 low,  A0 high 101
    addr 6 = A2 high, A1 high, A0 low  110
    addr 7 = A2 high, A1 high, A0 high 111
*/

/*
    -- MCP23017 Pin Assignment --
    Connect pin #12 of the expander to Analog 5 (i2c clock)
    Connect pin #13 of the expander to Analog 4 (i2c data)
    Connect pins #15, 16 and 17 of the expander to ground or 5V (address selection)
    Connect pin #9 of the expander to 5V (power)
    Connect pin #10 of the expander to ground (common ground)
    Connect pin #18 through a ~10kohm resistor to 5V (reset pin, active low)
*/

/*
   -- ATmega328P Pin Assignment --
   Connect Vin and UBS to 5V
   Connect both GND pins to Ground
   A4 and A5 are I2C bus
*/

#define ANALOG_SEND_FREQUENCY 50

unsigned long lastAnalogSend = 0;

Adafruit_MCP23017 mcp0;
Adafruit_MCP23017 mcp1;


#define LEDs_Size 28
Led* LEDs[LEDs_Size] = {
  // MCP 0
  new Led(&mcp0, 0),
  new Led(&mcp0, 1),
  new Led(&mcp0, 2),
  new Led(&mcp0, 3),
  new Led(&mcp0, 4),
  new Led(&mcp0, 5),
  new Led(&mcp0, 6),
  new Led(&mcp0, 7),
  new Led(&mcp0, 8),
  new Led(&mcp0, 9),
  new Led(&mcp0, 10),
  new Led(&mcp0, 11),
  new Led(&mcp0, 12),
  new Led(&mcp0, 13),

  // MCP 1
  new Led(&mcp1, 2),
  new Led(&mcp1, 3),
  new Led(&mcp1, 4),
  new Led(&mcp1, 5),
  new Led(&mcp1, 6),
  new Led(&mcp1, 7),
  new Led(&mcp1, 8),
  new Led(&mcp1, 9),
  new Led(&mcp1, 10),
  new Led(&mcp1, 11),
  new Led(&mcp1, 12),
  new Led(&mcp1, 13),
  new Led(&mcp1, 14),
  new Led(&mcp1, 15),
};

#define Buttons_Size 4
Button* Buttons[Buttons_Size] = {
  new Button(&mcp0, 14),
  new Button(&mcp0, 15),
  new Button(&mcp1, 0),
  new Button(&mcp1, 1),
};

#define Alphas_Size 1
Adafruit_AlphaNum4 Alphas[Alphas_Size] = {
  Adafruit_AlphaNum4(),
};


void setup() {
  mcp0.begin(0);
  mcp1.begin(1);

  setupLEDs(LEDs, LEDs_Size);



  Alphas[0].begin(0x70);  // pass in the address
  //alphaClear(Alphas[0]);
  alphaWriteString("INIT", Alphas[0]);
  ledsOn(LEDs, LEDs_Size);

  delay(1000);

  ledsOff(LEDs, LEDs_Size);


  for (int i = 0; i < Buttons_Size; i++) {
    Button* b = Buttons[i];
    b->setup();
  }

  Serial.begin(115200);

  alphaWriteString("  GO", Alphas[0]);

  LEDs[4]->onFor(1000);
}

void loop() {
  ledsUpdate(LEDs, LEDs_Size);
  processIncomingCommands();
  processButtons();
  processAnalog();
}


void setupLEDs(Led* leds[], int count) {
  for (int i = 0; i < count; i++) {
    Led* led = leds[i];
    led->setup();
  }
}

void ledsOn(Led* leds[], int count) {
  for (int i = 0; i < count; i++) {
    leds[i]->on();
  }
}

void ledsOff(Led* leds[], int count) {
  for (int i = 0; i < count; i++) {
    leds[i]->off();
  }
}

void ledsUpdate(Led* leds[], int count) {
  for (int i = 0; i < count; i++) {
    leds[i]->update();
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



void processIncomingCommands() {
  if (Serial.available() == 0)
    return;

  // wait for start of command, as signed by magic number
  int start = Serial.peek();
  while (start != COMMAND_START_1) {
    Serial.read();
    start = Serial.peek();
    if (start == -1)
      return;
  }

  if (Serial.available() < COMMAND_LENGTH)
    return;

  byte commandBuffer[COMMAND_LENGTH];
  size_t bytesRead = Serial.readBytes(commandBuffer, COMMAND_LENGTH);

  if (bytesRead != COMMAND_LENGTH)
    return;

  // validate magic number
  if (commandBuffer[0] != COMMAND_START_1)
    return;

  if (commandBuffer[1] != COMMAND_START_2)
    return;


  byte instruction = commandBuffer[COMMAND_OFFSET_INSTRUCTION];
  byte index = commandBuffer[COMMAND_OFFSET_PARAMETER];
  switch (instruction) {
    case COMMAND_LED_OFF:
      LEDs[index]->off();
      break;

    case COMMAND_LED_ON:
      LEDs[index]->on();
      break;

    case COMMAND_LED_OFF_FOR:
      LEDs[index]->offFor(bytesToUInt(commandBuffer[COMMAND_OFFSET_PARAMETER], commandBuffer[COMMAND_OFFSET_PARAMETER + 1]));
      break;

    case COMMAND_LED_ON_FOR:
      LEDs[index]->onFor(bytesToUInt(commandBuffer[COMMAND_OFFSET_PARAMETER], commandBuffer[COMMAND_OFFSET_PARAMETER + 1]));
      break;

    case COMMAND_LED_BLINK:
      LEDs[index]->startBlinking(bytesToUInt(commandBuffer[COMMAND_OFFSET_PARAMETER], commandBuffer[COMMAND_OFFSET_PARAMETER + 1]));
      break;

    case COMMAND_SET_ALPHA:
      alphaWriteString(commandBuffer + COMMAND_OFFSET_PARAMETER + 1, Alphas[index]);
      break;

  }

}

unsigned int bytesToUInt(byte b1, byte b2) {
  unsigned int i = (b1 << 8) | b2;
  return i;
}



int processButtons() {
  int sent = 0;

  for (byte i = 0; i < Buttons_Size; i++) {
    Button* b = Buttons[i];
    byte bVal = b->poll();

    if (bVal != UNCHANGED) {
      sendInputChanged(i, bVal);
      if(bVal == HIGH)
        LEDs[7]->onFor(200);
      else 
        LEDs[8]->onFor(200);

        sent++;
    }
  }

  if(sent > 0)
    Serial.flush();

  return sent;
}

void processAnalog() {
  unsigned long current = millis();
  if(current - lastAnalogSend < ANALOG_SEND_FREQUENCY) {
    return;
  }

  lastAnalogSend = current;
  sendAnalogValue(JOYSTICK_X, analogRead(A1));
  sendAnalogValue(JOYSTICK_Y, analogRead(A0));
  Serial.flush();
}

void sendInputChanged(byte index, byte value) {
  byte eventBuffer[EVENT_LENGTH];
  initEvent(eventBuffer);

  eventBuffer[EVENT_OFFSET_INSTRUCTION] = EVENT_INPUT_CHANGED;
  eventBuffer[EVENT_OFFSET_PARAMETER] = index;
  eventBuffer[EVENT_OFFSET_PARAMETER + 1] = value;

  Serial.write(eventBuffer, EVENT_LENGTH);
}

void sendAnalogValue(byte index, unsigned int value) {
  byte eventBuffer[EVENT_LENGTH];
  initEvent(eventBuffer);

  eventBuffer[EVENT_OFFSET_INSTRUCTION] = EVENT_ANALOG_VALUE;
  eventBuffer[EVENT_OFFSET_PARAMETER] = index;
  eventBuffer[EVENT_OFFSET_PARAMETER + 1] = (value >> 8) & 0xFF;
  eventBuffer[EVENT_OFFSET_PARAMETER + 2] = value & 0xFF;

  Serial.write(eventBuffer, EVENT_LENGTH);
}

void initEvent(byte* eventBuffer) {
  eventBuffer[0] = EVENT_START_1;
  eventBuffer[1] = EVENT_START_2;
  for (int i = 2; i < EVENT_LENGTH; i++) {
    eventBuffer[i] = EVENT_PAD;
  }

}



void systemDiagnostics() {
  alphaWriteString("TEST", Alphas[0]);

  for (int i = 0; i < 3; i++) {
    ledsOn(LEDs, LEDs_Size);
    delay(3000);

    ledsOff(LEDs, LEDs_Size);
    delay(500);
  }

  alphaClear(Alphas[0]);

  for (int i = 0; i < LEDs_Size; i++) {
    LEDs[i]->on();
    delay(250);
  }
  ledsOff(LEDs, LEDs_Size);


  alphaWriteString("X   ", Alphas[0]);
  delay(250);
  alphaWriteString(" X  ", Alphas[0]);
  delay(250);
  alphaWriteString("  X ", Alphas[0]);
  delay(250);
  alphaWriteString("   X", Alphas[0]);
  delay(250);
  alphaClear(Alphas[0]);
}
