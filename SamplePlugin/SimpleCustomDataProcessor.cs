using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Processing;


namespace SamplePlugin
{
    public sealed partial class SimpleCustomDataProcessor : CustomDataProcessor
    {
        private string[] filePaths;
        private LineItem[] lineItems;
        private DataSourceInfo dataSourceInfo;

        public SimpleCustomDataProcessor(
            string[] _filePaths,
            ProcessorOptions options,
            IApplicationEnvironment applicationEnvironment,
            IProcessorEnvironment processorEnvironment)
            : base(options, applicationEnvironment, processorEnvironment)
        {

            // Store the file paths for all of the data sources this processor will eventually 
            // need to process in a field for later
            //
            filePaths = _filePaths;

        }

        public override DataSourceInfo GetDataSourceInfo()
        {
            return dataSourceInfo;
        }

        protected override void BuildTableCore(TableDescriptor tableDescriptor, ITableBuilder tableBuilder)
        {
            Console.WriteLine("inside the build table core ");
            switch (tableDescriptor.Guid)
            {
                case var g when (g == WordTable.TableDescriptor.Guid):
                    new WordTable(this.lineItems).Build(tableBuilder);
                    Console.WriteLine("Inside the BuildTableCore for our specific table ");

                    break;
                default:
                    break;
            }

        }

        protected override Task ProcessAsyncCore(IProgress<int> progress, CancellationToken cancellationToken)
        {
            var startTime = DateTime.UtcNow;
            Console.WriteLine("Inside process Async core "+startTime);


            var returnable = ParseFiles(this.filePaths, progress, cancellationToken);
            this.lineItems = returnable;
            var totalEventDuration = (returnable.Last().TimeStamp - returnable.First().TimeStamp).Nanoseconds;
            var differenceBetweenFirsteventAndNow = (startTime - returnable[0].TimeStamp).Nanoseconds;
            this.dataSourceInfo = new DataSourceInfo(totalEventDuration, totalEventDuration+50000, startTime);




            return Task.CompletedTask;

        }

        private LineItem[] ParseFiles(string[] _filePaths, IProgress<int> progress, CancellationToken cancellationToken)
        {

            var totalLines = File.ReadLines(_filePaths[0]);
            int count = totalLines.Count();
            LineItem[] linesFromFile = new LineItem[count];

            if (count < 1) { throw new ArgumentOutOfRangeException("File has no data"); }

            int counter = 0;
            foreach (var item in totalLines)
            {
                var itemData = item.Split(','); // 2/4/2019 9:40:00 AM, Word1 Word2 Word3 
                DateTime.TryParse(itemData[0], out var timeStamp);
                List<string> otherData = itemData[1].Split(" ").ToList();

                linesFromFile[counter] = new LineItem
                {
                    LineContent = item,
                    LineNUmber = counter,
                    TimeStamp = timeStamp,
                    ProcessID = otherData[0],
                    ProcessName = otherData[1],
                    EventDescription = otherData[2],
                    Words = otherData,

                };


                progress.Report((counter / count) * 100);
                counter++;

            }


            return linesFromFile;
        }

    }
}
