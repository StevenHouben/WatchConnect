using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watch.Toolkit.Hardware.Arduino;

namespace Watch.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            var ard = new Arduino();
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
