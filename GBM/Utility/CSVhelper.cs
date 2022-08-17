﻿using CsvHelper;
using CsvHelper.Configuration;
using PartnerLed.Model;
using PartnerLed.Providers;
using System.Globalization;
using System.Text;

namespace PartnerLed.Utility
{
    /// <summary>
    /// The CsvProvider class.
    /// </summary>
    internal class CSVhelper : IExportImportProvider
    {
        /// <summary>
        /// Write data from CSV file async.
        /// </summary>
        /// <typeparam name="T">The type of data to be translated to CSV format.</typeparam>
        /// <param name="data">The list of data to export to CSV.</param>
        /// <param name="fileName">The filename to write CSV data to.</param>
        /// <returns>No return.</returns>
        public async Task WriteAsync<T>(IEnumerable<T> data, string fileName)
        {
            int index = fileName.LastIndexOf('/');
            var directory = fileName[..index];
            Directory.CreateDirectory(directory);

            using var subscriptionsWriter = new StreamWriter(fileName, false, Encoding.UTF8);
            using var subscriptionsCsvWriter = new CsvWriter(subscriptionsWriter, CultureInfo.InvariantCulture);
            await subscriptionsCsvWriter.WriteRecordsAsync(data);
        }

        /// <summary>
        /// Read data from CSV file async.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Task<List<T?>> ReadAsync<T>(string fileName)
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Encoding = Encoding.UTF8, // Our file uses UTF-8 encoding.
                Delimiter = ",", // The delimiter is a comma.
                ShouldSkipRecord = record => record.Record.All(field => string.IsNullOrWhiteSpace(field))      
            };

            using TextReader fileReader = File.OpenText($"{fileName}");
            using var csvReader = new CsvReader(fileReader, configuration);
            Type type = typeof(T);
            if (type.Name == "DelegatedAdminRelationship")
            {
                csvReader.Context.RegisterClassMap<DelegatedAdminRelationshipMap>();
            }
            List<T> input = csvReader.GetRecords<T>().ToList();
            csvReader.Dispose();
            fileReader.Close();
            return Task.FromResult(input);
        }

    }
}
