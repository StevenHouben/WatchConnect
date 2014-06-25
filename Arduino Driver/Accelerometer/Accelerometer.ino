#include<math.h>
#include <SPI.h>
#include <ADXL362.h>

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
        
void setup(){
  
  Serial.begin(9600);
  xl.begin(10);                   // Setup SPI protocol, issue device soft reset
  xl.beginMeasure();           // Switch ADXL362 to measure mode  
}

void loop(){
    
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
 
 Serial.print(x);
 Serial.print("|");
 Serial.print(y);
 Serial.print("|");
 Serial.print(z);
 Serial.print("|");
 Serial.print(fXg);
 Serial.print("|");
 Serial.print(fYg);
 Serial.print("|");
 Serial.print(fZg);
  Serial.print("|");
 Serial.print(pitch);
 Serial.print("|");
 Serial.print(roll);
 Serial.print("#");   

delay(0); 
}

