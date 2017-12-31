
// Command structure 
#define COMMAND_START_1             0xF7
#define COMMAND_START_2             0x82
#define COMMAND_PAD                 0xFF
#define COMMAND_LENGTH              10

#define COMMAND_OFFSET_INSTRUCTION  3
#define COMMAND_OFFSET_PARAMETER    4

// Commands
#define COMMAND_LED_OFF             0
#define COMMAND_LED_ON              1
#define COMMAND_LED_OFF_FOR         2
#define COMMAND_LED_ON_FOR          3
#define COMMAND_LED_BLINK           4
#define COMMAND_SET_ALPHA           5
#define COMMAND_GET_INPUT           6
#define COMMAND_GET_ANALOG          7


// Event structure 
#define EVENT_START_1             0x7F
#define EVENT_START_2             0x28
#define EVENT_PAD                 0xFF
#define EVENT_LENGTH              8

#define EVENT_OFFSET_INSTRUCTION  3
#define EVENT_OFFSET_PARAMETER    4
#define EVENT_INPUT_CHANGED       0
#define EVENT_ANALOG_VALUE        1

// Inputs
#define JOYSTICK_X                0xA0
#define JOYSTICK_Y                0xA1
#define FREE_MEMORY               0xAF

#define BUTTON_TEST_1             0x00
#define BUTTON_TEST_2             0x01


#define ERROR_PIN                 13



