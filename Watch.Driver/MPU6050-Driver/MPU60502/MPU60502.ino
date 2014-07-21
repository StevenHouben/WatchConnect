#include <SPI.h>
#include <Adafruit_CAP1188.h>
#include "I2Cdev.h"
#include "MPU6050_6Axis_MotionApps20.h"
#if I2CDEV_IMPLEMENTATION == I2CDEV_ARDUINO_WIRE
    #include "Wire.h"
#endif


//---------------------------------------------------------------------------
// FlexpotSensor
//---------------------------------------------------------------------------
int potPin = A3;    

//---------------------------------------------------------------------------
// LightSensor
//---------------------------------------------------------------------------
int photocellPin = A0;       

//---------------------------------------------------------------------------
// Proximity Sensor
//---------------------------------------------------------------------------
int pin[2] = { A1, A2 };
int distance[2];

//---------------------------------------------------------------------------
// Capacitive Sensor
//---------------------------------------------------------------------------
Adafruit_CAP1188 cap = Adafruit_CAP1188();

//---------------------------------------------------------------------------
// IMU Sensor
//---------------------------------------------------------------------------
MPU6050 mpu;
bool dmpReady = false;  // set true if DMP init was successful
uint8_t mpuIntStatus;   // holds actual interrupt status byte from MPU
uint8_t devStatus;      // return status after each device operation (0 = success, !0 = error)
uint16_t packetSize;    // expected DMP packet size (default is 42 bytes)
uint16_t fifoCount;     // count of all bytes currently in FIFO
uint8_t fifoBuffer[64]; // FIFO storage buffer


Quaternion q;           // [w, x, y, z]         quaternion container
int32_t gyro[3];          // [x, y, z]            gyro sensor measurements
VectorInt16 aa;         // [x, y, z]            accel sensor measurements
VectorInt16 aaReal;     // [x, y, z]            gravity-free accel sensor measurements
VectorInt16 aaWorld;    // [x, y, z]            world-frame accel sensor measurements
VectorFloat gravity;    // [x, y, z]            gravity vector
float euler[3];         // [psi, theta, phi]    Euler angle container
float ypr[3];           // [yaw, pitch, roll]   yaw/pitch/roll container and gravity vector
int16_t ax,ay,az,gx,gy,gz;

//---------------------------------------------------------------------------
// Lists connected sensors
//---------------------------------------------------------------------------
boolean lightSensorFound = true;
boolean proximitySensorFound = true;
boolean imuFound = false;
boolean touchSensorFound = false;

//---------------------------------------------------------------------------
// Handles interrupt
//---------------------------------------------------------------------------
volatile bool mpuInterrupt = false;

void dmpDataReady() {
    mpuInterrupt = true;
}

//---------------------------------------------------------------------------
// Setup
//---------------------------------------------------------------------------=

void StartCapactiveSensor()
{
  //Start and calibrate the touch sensor
  if (cap.begin()) {
     uint8_t reg = cap.readRegister( 0x1f ) & 0x0f;
    cap.writeRegister( 0x1f, reg | 0x60 ); // or whatever value you want
    touchSensorFound = true;
  }
}

void StartIMU()
{
   // join I2C bus (I2Cdev library doesn't do this automatically)
    #if I2CDEV_IMPLEMENTATION == I2CDEV_ARDUINO_WIRE
        Wire.begin();
        TWBR = 24; // 400kHz I2C clock (200kHz if CPU is 8MHz)
    #elif I2CDEV_IMPLEMENTATION == I2CDEV_BUILTIN_FASTWIRE
        Fastwire::setup(400, true);
    #endif

  
    Serial.begin(115200);
    
    mpu.initialize();
    devStatus = mpu.dmpInitialize();
    
    mpu.setXAccelOffset(-772.5); 
    mpu.setYAccelOffset(455);
    mpu.setZAccelOffset(1454);
    mpu.setXGyroOffset(50);    
    mpu.setYGyroOffset(-34);    
    mpu.setZGyroOffset(6);   
    
    mpu.setDMPEnabled(true);
    
    mpuIntStatus = mpu.getIntStatus();
    
     packetSize = mpu.dmpGetFIFOPacketSize();
}
void setup() {
    StartIMU();
    StartCapactiveSensor();
}

void ReadProximity()
{
      String proximity = "P";
      //int right = analogRead(rightProximityPin);
      //int left = analogRead(leftProximityPin);
      
      // make 2 measurements 
      for (int p = 0; p < 2; p++)   // do it twice for index 0 and index 1
      {
        distance[p] = 0;
        for (int i=0; i< 16; i++) distance[p] += analogRead( pin[p] );   // make 16 readings
        distance[p] /= 16;  // average them
        // optional delay(100) here..
      }
      
      proximity+=",";
      proximity+=distance[0];
      proximity+=",";
      proximity+=distance[1];
      proximity+="#";
      Serial.print(proximity);
}

void ReadLight()
{
      String light = "L";
      light+=",";
      light+=analogRead(A0);
      light+="#";
     
      Serial.print(light);   
}

void ReadTouches()
{
    uint8_t touched = cap.touched();

     String touches = "T";
     touches+=",";
     for (uint8_t i=0; i<8; i++) {
    
     if (touched & (1 << i)) 
     {
        touches+="1";
     }
     else
     {
       touches+="0";
   }
    if(i<7)
    {
      touches+=",";
    }
  }
  touches+="#";
  Serial.print(touches);

}

void ReadPotPin()
{
      String slider = "S";
      
      int value = analogRead(A3);
      if(value <90) value = 0;
      slider+=",";
      slider+=value;
      slider+="#";
     
      Serial.print(slider);
}

void ReadImu()
{
    mpuIntStatus = mpu.getIntStatus();

    fifoCount = mpu.getFIFOCount();

    // check for overflow (this should never happen unless our code is too inefficient)
    if ((mpuIntStatus & 0x10) || fifoCount == 1024) {
        // reset so we can continue cleanly
        mpu.resetFIFO();
        Serial.println(F("FIFO overflow!"));

    // otherwise, check for DMP data ready interrupt (this should happen frequently)
    } else if (mpuIntStatus & 0x02) {
        // wait for correct available data length, should be a VERY short wait
        while (fifoCount < packetSize) fifoCount = mpu.getFIFOCount();

        // read a packet from FIFO
        mpu.getFIFOBytes(fifoBuffer, packetSize);
        
        // track FIFO count here in case there is > 1 packet available
        // (this lets us immediately read more without waiting for an interrupt)
        fifoCount -= packetSize;
        
            mpu.getMotion6(&ax, &ay, &az, &gx, &gy, &gz);
            mpu.dmpGetAccel(&aa, fifoBuffer);

            mpu.dmpGetGravity(&gravity, &q);
            mpu.dmpGetYawPitchRoll(ypr, &q, &gravity);
            mpu.dmpGetQuaternion(&q, fifoBuffer);
            mpu.dmpGetLinearAccel(&aaReal, &aa, &gravity);
            mpu.dmpGetLinearAccelInWorld(&aaWorld, &aaReal, &q);
            
            Serial.print("A");
            Serial.print(",");
            Serial.print(ax);
            Serial.print(",");
            Serial.print(ay);
            Serial.print(",");
            Serial.print(az);
            Serial.print(",");
            Serial.print(gx);
            Serial.print(",");
            Serial.print(gy);
            Serial.print(",");
            Serial.print(gz);
            Serial.print(",");
            Serial.print(ypr[0] * 180/M_PI);
            Serial.print(",");
            Serial.print(ypr[1] * 180/M_PI);
            Serial.print(",");
            Serial.print(ypr[2] * 180/M_PI);     
            Serial.print(",");
            Serial.print(aaWorld.x);
            Serial.print(",");
            Serial.print(aaWorld.y);
            Serial.print(",");
            Serial.print(aaWorld.z);
            Serial.print("#");
    }
}
void loop() {
  
   ReadImu();
  ReadProximity(); 
  ReadLight();
  ReadTouches();
  ReadPotPin();
   
}
