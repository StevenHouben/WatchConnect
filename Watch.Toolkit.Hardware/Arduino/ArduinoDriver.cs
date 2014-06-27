using System;
using System.IO.Ports;

namespace Watch.Toolkit.Hardware.Arduino
{
    internal class ArduinoDriver:AbstractHardwarePlatform
    {
        public string Port { get; private set; }
        public string ReadData { get; private set; }

        private const int BaudRate = 9600;

        private SafeSerialPort _serialPort;
        private string _output;

        public ArduinoDriver(string port)
        {
            ConnectToDevice(port);
        }
        public void Stop()
        {
            _serialPort.Close();
        }

        private void ConnectToDevice(string port)
        {
            Port = port;
            ConnectToArduino(port);
        }

        ~ArduinoDriver()
        {
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                    _serialPort.Dispose();
                }
            }
        }

        private void ConnectToArduino(string portname)
        {
            try
            {
                _serialPort = null;
                _serialPort = new SafeSerialPort(portname, BaudRate);
                _serialPort.DataReceived += serialPort_DataReceived;
                _serialPort.Open();

                Console.WriteLine("Found device at: " + portname);
            }
            catch (Exception ex)
            {
                Console.WriteLine("NOT connected to: " + portname);
                Console.WriteLine(ex.ToString());
            }
        }

        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _output += _serialPort.ReadTo("#");

            OnMessageReceived(this, new MessagesReceivedEventArgs(-1,_output));
            _output = "";
        }
    }
}
