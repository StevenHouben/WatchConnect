using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Watch.Toolkit
{
    public static class Helper
    {
        public static void WriteToFile(StringBuilder data, string fileName)
        {
            File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + fileName, data.ToString());
        }

        public static DataTable ReadCsvToDataTable(string filePath)
        {
            var filename = filePath;

            if(!File.Exists(filename))
                throw new IOException("File not found");

            var reader = File.ReadAllLines(filename);

            var data = new DataTable();

            var headers = reader.First().Split(',');
            foreach (var header in headers)
                data.Columns.Add(header);

            var records = reader.Skip(1);
            foreach (var record in records.Where(record => record != null))
                data.Rows.Add(record.Split(','));
            return data;
        }
    }
}
