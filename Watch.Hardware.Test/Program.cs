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
            Console.WriteLine(e.Message);
        }
    }
}
