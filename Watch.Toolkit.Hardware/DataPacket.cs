using System;

namespace Watch.Toolkit.Hardware
{
    public class DataPacket
    {
        public string Header { get; set; }
        public string[] Body { get; set; }

        public DataPacket(string[] raw)
        {
            Body = SubArray(raw, 1, raw.Length-1);
            Header = raw[0];
        }
        public static T[] SubArray<T>(T[] data, int index, int length)
        {
            var result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
    
}
