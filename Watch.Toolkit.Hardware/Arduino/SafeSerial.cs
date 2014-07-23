using System;
using System.IO;
using System.IO.Ports;

namespace Watch.Toolkit.Hardware.Arduino
{
    public class SafeSerialPort : SerialPort
    {
        Stream _theBaseStream;

        public SafeSerialPort(string portName, int baudRate)
            : base(portName, baudRate) { }

        public new void Open()
        {
            try
            {
                
                base.Open();
                _theBaseStream = BaseStream;
                GC.SuppressFinalize(BaseStream);
            }
            catch { }
        }

        public new void Dispose()
        {
            Dispose(true);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (Container != null))
            {
                Container.Dispose();
            }
            try
            {
                if (_theBaseStream.CanRead)
                {
                    _theBaseStream.Close();
                    GC.ReRegisterForFinalize(_theBaseStream);
                }
            }
            catch
            {
                // ignore exception - bug with USB - serial adapters.
            }
            base.Dispose(disposing);
        }
    }
}