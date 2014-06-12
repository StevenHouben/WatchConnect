using System;
using Watch.Toolkit.Hardware.Arduino;

namespace Watch.Hardware.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var arduino = new Arduino();
            arduino.MessageReceived += arduino_MessageReceived;
            arduino.Start("COM4");
            Console.ReadLine();
        }

        static void arduino_MessageReceived(object sender, Toolkit.Hardware.MessagesReceivedEventArgs e)
        {
            var data = e.Message.Split('|');

            if (data.Length != 5) return;

            var ad = new AccelerometerData(
                Convert.ToInt32(data[0]),
                Convert.ToInt32(data[1]),
                Convert.ToInt32(data[2]));

            Console.WriteLine(ad);
        }
    }

    public class AccelerometerData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public AccelerometerData(){}

        public AccelerometerData(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return "X: " + X + " Y: " + Y + " Z: " + Z;
        }
    }
}
