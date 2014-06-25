#include <Wire.h>
#include <SPI.h>
#include <Adafruit_CAP1188.h>
#include<math.h>
#include <ADXL362.h>

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

ADXL362 xl;

int16_t temp;
int16_t x, y, z, t;


long xy_max = 470;
long xy_min = 550;

long xMin, xMax, yMin, yMax, zMin, zMax;
const double alpha = 0.5;
        double fXg = 0;
        double fYg = 0;
        double fZg = 0;

void setup() {
  
   pinMode(TILT, INPUT);
   
  Serial.begin(9600);

  if (cap.begin()) {
     uint8_t reg = cap.readRegister( 0x1f ) & 0x0f;
    cap.writeRegister( 0x1f, reg | 0x60 ); // or whatever value you want
  }
   
 
  
  xl.begin(10);                   // Setup SPI protocol, issue device soft reset
  xl.beginMeasure(); 
}
boolean isConnected = true;


void loop() {
  uint8_t touched = cap.touched();

  String touches = "T";
  String accData = "A";
  
  for (uint8_t i=0; i<8; i+=2) {
    if (touched & (1 << i)) 
    {
        touches+="1";
    }
    else
    {
      touches+="0";
    }
  }
  touches+="#";
  if(isConnected)
  {
        Serial.print(touches);
  }
  
   xl.readXYZTData(x, y, z, t);  
  
   //Low Pass Filter
    fXg = x * alpha + (fXg * (1.0 - alpha));
    fYg = y* alpha + (fYg * (1.0 - alpha));
    fZg = z * alpha + (fZg * (1.0 - alpha));
    //Roll & Pitch Equations
    double pitch = (atan2(-fYg, fZg) * 180.0) / 3.14;
    double roll = (atan2(fXg, sqrt(fYg * fYg + fZg * fZg)) * 180.0) / 3.14;
    pitch = (pitch >= 0) ? (180 - pitch) : (-pitch - 180);
  
  if(x>xMax) xMax = x;
  if(x<xMin) xMin = x;
  
  if(y>yMax) yMax = y;
  if(y<yMin) yMin = y;
//  
  if(z>zMax) zMax = z;
  if(z<zMin) zMin = z;

 Serial.print('A');
  Serial.print(",");
 Serial.print(x);
 Serial.print(",");
 Serial.print(y);
 Serial.print(",");
 Serial.print(z);
 Serial.print(",");
 Serial.print(fXg);
 Serial.print(",");
 Serial.print(fYg);
 Serial.print(",");
 Serial.print(fZg);
  Serial.print(",");
 Serial.print(pitch);
 Serial.print(",");
 Serial.print(roll);
 Serial.print("#");  
}
