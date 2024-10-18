using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace Twm.Core.DataProviders.DataReaders.Csv
{
    public class CsvDataWriter<TEntity, TEntityMapper> where TEntity : class where TEntityMapper : class
    {
        public static void Write(string filePath,IEnumerable<TEntity> records,  bool hasHeaderRecord = false, string delimiter = ";")
        {

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = hasHeaderRecord,
                Delimiter = delimiter,
                MissingFieldFound = null,
                TrimOptions = TrimOptions.Trim
            };
         
            //using (Stream stream =  File.OpenWrite(filePath))
            {
                using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    using (CsvWriter csvWriter = new CsvWriter(writer, config))
                    {
                        csvWriter.Context.RegisterClassMap(typeof(TEntityMapper));
                        csvWriter.WriteRecords(records);
                    }
                    writer.Close();
                }
              //  stream.Close();
            }
        }

        public static async Task<bool> WriteAsync(string filePath, IEnumerable<TEntity> records, bool hasHeaderRecord = false, string delimiter = ";")
        {

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = hasHeaderRecord,
                Delimiter = delimiter,
                MissingFieldFound = null,
                TrimOptions = TrimOptions.Trim
            };

            using (Stream stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
            {
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    using (CsvWriter csvWriter = new CsvWriter(writer, config))
                    {
                        csvWriter.Context.RegisterClassMap(typeof(TEntityMapper));
                        await csvWriter.WriteRecordsAsync(records);
                        return true;
                    }
                }
            }
        }


    }
}
