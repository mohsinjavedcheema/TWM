using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Microsoft.EntityFrameworkCore.Storage;

namespace Twm.Core.DataProviders.DataReaders.Csv
{
    public class CsvDataReader<TEntity, TEntityMapper> where TEntity : class where TEntityMapper : class
    {
        public static TEntity[] Read(string filePath, bool hasHeaderRecord = false, string delimiter = ";")
        {

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = hasHeaderRecord,
                Delimiter = delimiter,
                MissingFieldFound = null,
                TrimOptions = TrimOptions.Trim
            };

            

            
                using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    using (CsvReader csvReader = new CsvReader(reader, config))
                    {
                        csvReader.Context.RegisterClassMap(typeof(TEntityMapper));
                        var arr = csvReader.GetRecords<TEntity>().ToArray();
                        reader.Close();
                        return arr;
                    }
                    
                }                
            
        }


    }


}
