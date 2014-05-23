using System;
using System.IO;
using System.Text;

namespace Watch
{
    public static class Helper
    {
        public static void WriteToFile(StringBuilder data, string fileName)
        {
            File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + fileName, data.ToString());

        }
    }
}
