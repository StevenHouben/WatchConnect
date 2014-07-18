using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Watch.Toolkit.Hardware;
using Watch.Toolkit.Hardware.Arduino;

namespace Watch.Toolkit.Sensors
{
    public class ImuParser
    {
        public event EventHandler<ImuDataReceivedEventArgs> AccelerometerDataReceived = delegate { };
        public event EventHandler<String> EventTriggered = delegate { };

        private readonly Dictionary<string,Func<Imu, bool>> _events = new Dictionary<string, Func<Imu, bool>>();

        private Imu _imu;
        public void AddEvent(string name, Func<Imu, bool> condition)
        {
            _events.Add(name,condition);

        }

        private Arduino _arduino;
        public void Start()
        {
            _arduino = new Arduino();
            _arduino.DataPacketReceived += _arduino_DataPacketReceived;    
            _arduino.AddPacketListener("IMU",
                (message) =>
                {
                    if (message.StartsWith("A"))
                        return message.Split(',').Length == 13;
                    return false;
                },
                (message) => new DataPacket(message.Split(',')));
            _arduino.Start();

            _imu = new Imu();
        }

        private void _arduino_DataPacketReceived(object sender, Hardware.DataPacketReceivedEventArgs e)
        {
            switch (e.DataPacket.Header)
            {
                case "A":
                    _imu.Update(new Vector(
                        Convert.ToDouble(e.DataPacket.Body[0], CultureInfo.InvariantCulture),
                        Convert.ToDouble(e.DataPacket.Body[1], CultureInfo.InvariantCulture),
                        Convert.ToDouble(e.DataPacket.Body[2], CultureInfo.InvariantCulture)),
                        new Vector(
                            Convert.ToDouble(e.DataPacket.Body[3], CultureInfo.InvariantCulture),
                            Convert.ToDouble(e.DataPacket.Body[4], CultureInfo.InvariantCulture),
                            Convert.ToDouble(e.DataPacket.Body[5], CultureInfo.InvariantCulture)),
                        new Vector(
                            Convert.ToDouble(e.DataPacket.Body[6], CultureInfo.InvariantCulture),
                            Convert.ToDouble(e.DataPacket.Body[7], CultureInfo.InvariantCulture),
                            Convert.ToDouble(e.DataPacket.Body[8], CultureInfo.InvariantCulture)),
                        new Vector(
                            Convert.ToDouble(e.DataPacket.Body[9], CultureInfo.InvariantCulture),
                            Convert.ToDouble(e.DataPacket.Body[10], CultureInfo.InvariantCulture),
                            Convert.ToDouble(e.DataPacket.Body[11], CultureInfo.InvariantCulture)));

                    AccelerometerDataReceived(this, new ImuDataReceivedEventArgs(_imu));

                    foreach (var ev in _events.Where(ev => ev.Value(_imu)).Where(ev => EventTriggered != null))
                    {
                        EventTriggered(_imu, ev.Key);
                    }
                    break;
            }
        }

        public void Stop()
        {
            _arduino.Stop();
        }
    }
}
