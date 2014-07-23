using System;
using Watch.Toolkit.Hardware.Arduino;

namespace Watch.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            var ard = new Arduino("COM7");
            ard.MessageReceived += ard_MessageReceived;
            ard.Start();

            while (true) ;
        }

        static void ard_MessageReceived(object sender, Toolkit.Hardware.MessagesReceivedEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
