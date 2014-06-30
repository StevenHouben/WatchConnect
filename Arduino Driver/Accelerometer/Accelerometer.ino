#include<math.h>
#include <SPI.h>
#include <ADXL362.h>

ADXL362 xl;

int16_t temp;
int16_t x, y, z, t;

long X,Y,Z;

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
  
  X = map(x, -2048, 2048, 0, 1);
  Y = map(y, -2048, 2048, 0, 1);
  Z = map(z, -2048, 2048, 0, 1);
  
  //convert read values to degrees -90 to 90 - Needed for atan2
//  int xAng = map(x, -2048, 2048, -90, 90);
//  int yAng = map(y, -2048, 2048, -90, 90);
//  int zAng = map(z, -2048, 2048, -90, 90);

  //Caculate 360deg values like so: atan2(-yAng, -zAng)
  //atan2 outputs the value of -π to π (radians)
  //We are then converting the radians to degrees
//  x = RAD_TO_DEG * (atan2(-yAng, -zAng) + PI);
//  y = RAD_TO_DEG * (atan2(-xAng, -zAng) + PI);
//  z = RAD_TO_DEG * (atan2(-yAng, -xAng) + PI);

  //Output the caculations
  Serial.print("x: ");
  Serial.print(X);
  Serial.print(" | y: ");
  Serial.print(Y);
  Serial.print(" | z: ");
  Serial.println(Z);

  delay(150);//just here to slow down the serial output - Easier to read

}

