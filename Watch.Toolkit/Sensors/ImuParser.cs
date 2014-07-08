using System;
using System.Globalization;
using Watch.Toolkit.Hardware.Arduino;

namespace Watch.Toolkit.Sensors
{
    public class ImuParser
    {
        public event EventHandler<ImuDataReceivedEventArgs> AccelerometerDataReceived = delegate { };

        private Arduino _arduino;
        public void Start()
        {
            _arduino = new Arduino();
            _arduino.MessageReceived += arduino_MessageReceived;
            _arduino.Start();
        }

        public void Stop()
        {
            _arduino.Stop();
        }
        void arduino_MessageReceived(object sender, Hardware.MessagesReceivedEventArgs e)
        {
            if (!e.Message.StartsWith("A"))
                return;

            var data = e.Message.Split(',');

            if (data.Length != 13) return;

            var acc = new Imu();
            acc.Update(new Vector(
                Convert.ToDouble(data[1], CultureInfo.InvariantCulture),
                Convert.ToDouble(data[2], CultureInfo.InvariantCulture),
                Convert.ToDouble(data[3], CultureInfo.InvariantCulture)),
                new Vector(
                    Convert.ToDouble(data[4], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[5], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[6], CultureInfo.InvariantCulture)),
                new Vector(
                    Convert.ToDouble(data[7], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[8], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[9], CultureInfo.InvariantCulture)),
                new Vector(
                    Convert.ToDouble(data[10], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[11], CultureInfo.InvariantCulture),
                    Convert.ToDouble(data[12], CultureInfo.InvariantCulture)));

           AccelerometerDataReceived(this, new ImuDataReceivedEventArgs(acc));
                
        }

    }
}
