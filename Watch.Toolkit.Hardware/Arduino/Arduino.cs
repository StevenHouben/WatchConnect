                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using LibUsbDotNet.DeviceNotify;


namespace Watch.Toolkit.Hardware.Arduino
{
    public class Arduino : AbstractHardwarePlatform
    {
        public string Port { get; private set; }
        public string ReadData { get; private set; }

        private const string HandShakeCommand = "A";
        private const string HandShakeReply = "B";
        private const int BaudRate = 9600;
        private const int ReadDelay = 100; //ms
        private const int ReadTimeOut = 200; //ms

        private SafeSerialPort _serialPort;
        private string _output;

        private readonly IDeviceNotifier _usbDeviceNotifier = DeviceNotifier.OpenDeviceNotifier();

        public void Stop()
        {
            _serialPort.Close();
        }

        private void Connect()
        {
            var port = FindDevice();
            if (port != null)
            {
                ConnectToDevice(port);
            }
            else
                Console.WriteLine("No  Device found");
        }

        private void ResetConnection()
        {
            try
            {
                if (_serialPort != null)
                    _serialPort.Write("Any value");
            }
            catch (IOException)
            {
                if (_serialPort == null) return;
                _serialPort.Dispose();
                _serialPort.Close();
            }
        }

        private void UsbDeviceNotifier_OnDeviceNotify(object sender, DeviceNotifyEventArgs e)
        {
            try
            {
                if (e.Object.ToString().Split('\n')[1].Contains("0x2341"))
                {
                    if (e.EventType == EventType.DeviceArrival)
                    {
                        Connect();
                    }
                    else if (e.EventType == EventType.DeviceRemoveComplete)
                    {
                        ResetConnection();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private string FindDevice()
        {
            var ports = SerialPort.GetPortNames();
            foreach (var portname in ports)
            {
                var sp = new SerialPort(portname, BaudRate);
                try
                {
                    sp.ReadTimeout = ReadTimeOut;
                    sp.Open();
                    sp.Write(HandShakeCommand);
                    Thread.Sleep(ReadDelay);

                    string received = sp.ReadExisting();

                    if (received == HandShakeReply)
                        return portname;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    sp.Close();
                }
            }
            return null;
        }

        public void Start(string port)
        {
            ConnectToDevice(port);
            _usbDeviceNotifier.OnDeviceNotify += UsbDeviceNotifier_OnDeviceNotify;
        }

        public void Start()
        {
            Connect();
            _usbDeviceNotifier.OnDeviceNotify += UsbDeviceNotifier_OnDeviceNotify;
        }

        private void ConnectToDevice(string port)
        {
            Port = port;
            ConnectToArduino(port);
        }

        ~Arduino()
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

                IsRunning = true;
                Console.WriteLine("Found device at: " + portname);
            }
            catch (Exception ex)
            {
                Console.WriteLine("NOT connected to: " + portname);
                Console.WriteLine(ex.ToString());
            }
        }

        private void WriteToDevice(string msg)
        {
            try
            {
                if (_serialPort != null)
                    if (_serialPort.IsOpen)
                        _serialPort.Write(msg);
            }
            catch (IOException)
            {
                ResetConnection();
                //var success  = PortHelper.TryResetPortByName(Port);
                //Thread.Sleep(10000);
                //ConnectToDevice(Port);
            }
        }


        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _output += _serialPort.ReadTo("#");

            if (_output.Length != 4) return;

            OnMessageReceived(this, new MessagesReceivedEventArgs(-1,_output));
            _output = "";
        }

    }
}
