using System;
using System.Threading;
using Phidgets;

namespace Watch.Toolkit.Hardware.Phidget
{
    public class Phidget:AbstractHardwarePlatform
    {
        private InterfaceKit _kit;

        public void Start(int id)
        {
            var t = new Thread(() =>
            {
                _kit = PhidgetManager.InterfaceKit;
                _kit.SensorChange += kit_SensorChange;
                _kit.OutputChange += kit_OutputChange;
                _kit.InputChange += kit_InputChange;


                _kit.open(id);
                _kit.waitForAttachment();

                IsRunning = true;
            });
            t.Start();
        }

        public void Stop()
        {
            try
            {
                _kit.SensorChange -= kit_SensorChange;
                _kit.OutputChange -= kit_OutputChange;
                _kit.InputChange -= kit_InputChange;
                _kit.close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                
            }

            IsRunning = false;
        }

        void kit_InputChange(object sender, Phidgets.Events.InputChangeEventArgs e)
        {
            OnDigitalInReceived(this, new DigitalDataReivedHandler(e.Index,e.Value));
        }

        void kit_OutputChange(object sender, Phidgets.Events.OutputChangeEventArgs e)
        {
            OnDigitalOutReceived(this, new DigitalDataReivedHandler(e.Index, e.Value));
        }

        void kit_SensorChange(object sender, Phidgets.Events.SensorChangeEventArgs e)
        {
            OnAnalogDataReceived(this, new AnalogDataReceivedEventArgs(e.Index, e.Value));
        }
    }
}
