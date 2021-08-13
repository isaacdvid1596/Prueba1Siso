using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;

namespace PruebaSISO
{

    class Ticker
    {
        public DateTime Date { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
    }

    class Resultado
    {
        public decimal result { get; set; }
        public DateTime year { get; set; }
    }

    class Program
    {

        private static readonly HttpClient _httpClient = new HttpClient();
        private static object lockObject = new object();
        
        static async Task Main(string[] args)
        {
            var microsoftTicker = await _httpClient.GetStreamAsync("https://stooq.com/q/d/l/?s=msft.us&i=d");
            var appleTicker = await _httpClient.GetStreamAsync("https://stooq.com/q/d/l/?s=msft.us&i=d");
            var amazonTicker = await _httpClient.GetStreamAsync("https://stooq.com/q/d/l/?s=msft.us&i=d");
            var facebookTicker = await _httpClient.GetStreamAsync("https://stooq.com/q/d/l/?s=msft.us&i=d");
            var netflixTicker = await _httpClient.GetStreamAsync("https://stooq.com/q/d/l/?s=msft.us&i=d");

            var data = ParseFile(microsoftTicker);
            var sum = new List<decimal>();
            var avg = new List<decimal>();

            Parallel.ForEach(data, d =>
            {
                lock (lockObject)
                {
                    sum.Add(d.Close);

                    foreach (var s in sum)
                    {
                        decimal temp = 0;
                        temp += s;
                        avg.Add(temp);
                    }
                }
            });

            var jsonFile = JsonConvert.SerializeObject(avg);

            Parallel.ForEach(jsonFile, j =>
            {
                File.WriteAllTextAsync(@"C:\Users\isaac\Documents", jsonFile);
            });

            Console.WriteLine($"Promedio {avg}");

        }

        private static IEnumerable<Ticker> ParseFile(Stream res)
        {
            using (var reader = new StreamReader(res))
            {
                using (var csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    MissingFieldFound = null
                }))
                {
                    return csvReader.GetRecords<Ticker>().ToList();
                }
            }
        }
    }
}
