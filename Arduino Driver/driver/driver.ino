#include <Wire.h>
#include <SPI.h>
#include <Adafruit_CAP1188.h>

// Reset Pin is used for I2C or SPI
#define CAP1188_RESET  9

// CS pin is used for software or hardware SPI
#define CAP1188_CS  10

// These are defined for software SPI, for hardware SPI, check your 
// board's SPI pins in the Arduino documentation
#define CAP1188_MOSI  11
#define CAP1188_MISO  12
#define CAP1188_CLK  13

#define TILT 2
int buttonState = 0;

Adafruit_CAP1188 cap = Adafruit_CAP1188();

void setup() {
  
   pinMode(TILT, INPUT);
   
  Serial.begin(9600);

  if (!cap.begin()) {
    while (1);
  }
   
  uint8_t reg = cap.readRegister( 0x1f ) & 0x0f;
  cap.writeRegister( 0x1f, reg | 0x60 ); // or whatever value you want
}
boolean isConnected = true;


void loop() {
  uint8_t touched = cap.touched();

  String toSend = "";
   
  for (uint8_t i=0; i<8; i+=2) {
    if (touched & (1 << i)) 
    {
        toSend+="1";
    }
    else
    {
      toSend+="0";
    }
  }
  toSend+="#";
  if(isConnected)
  {
        Serial.print(toSend);
  }
  delay(250);
}
