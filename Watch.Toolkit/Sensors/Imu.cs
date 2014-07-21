using System;
using System.Collections.Generic;
using System.Linq;

namespace Watch.Toolkit.Sensors
{
    public class Imu : ISensor
    {
        public event EventHandler<String> EventTriggered = delegate { };
        private readonly Dictionary<string, Func<Imu, bool>> _events = new Dictionary<string, Func<Imu, bool>>();
        public Imu AddEvent(string name, Func<Imu, bool> condition)
        {
            _events.Add(name, condition);
            return this;
        }

        public void RemoveEvent(string name)
        {
            _events.Remove(name);
        }
        
        public Vector RawGyroValue { get; set; }
        public Vector RawAccelerometerValues{ get; set; }
        public Vector RealWorldAccelerationValues { get; set; }
        public Vector YawPitchRollValues { get; set; }
        public Vector RawMagnetometerValues { get; set; }

        public event EventHandler ImuUpdated = delegate { }; 

        public void Update(Imu acc)
        {
            Update(acc.RawAccelerometerValues, acc.RawGyroValue, acc.YawPitchRollValues, acc.RealWorldAccelerationValues);
        }
        public void Update(Vector rawAccData, Vector rawGyroData, Vector yawPitchRoll, Vector worldAcceleration)
        {
            RawAccelerometerValues = rawAccData;
            RawGyroValue = rawGyroData;
            YawPitchRollValues = yawPitchRoll;
            RealWorldAccelerationValues = worldAcceleration;

            ImuUpdated(this, new EventArgs());
            
            foreach (var ev in _events.ToList().Where(ev => ev.Value(this)).Where(ev => EventTriggered != null))
            {
                EventTriggered(this, ev.Key);
            }
        }
        public string ToFormattedString()
        {
            return "Yaw: " + YawPitchRollValues.X +
                   "\nPitch: " + YawPitchRollValues.Y +
                   "\nRoll: " + YawPitchRollValues.Z +
                   "\nAccX: " + RawAccelerometerValues.X +
                   "\nAccY: " + RawAccelerometerValues.Y +
                   "\nAccZ: " + RawAccelerometerValues.Z +
                   "\nGyrX: " + RawGyroValue.X +
                   "\nGyrY: " + RawGyroValue.Y +
                   "\nGyrZ:" + RawGyroValue.Z;
        }

        public string Name { get; set; }
        public int Id { get; set; }
        public double Value { get; set; }

     }
 }
