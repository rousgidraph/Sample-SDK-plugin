using Microsoft.Performance.SDK.Processing;

namespace SamplePlugin
{
    // Guid must be unique for your processing source ,         Processing source must have a name , processing source must have a description 
    [ProcessingSource("{5502EC64-21A4-4057-B559-9A89D5E3C477}", "Simple data source", "A data source that displays dummy data")]
    [FileDataSource(".txt", "Reads simple text files ")]
    public class MyProcessingSource : ProcessingSource
    {
        protected override ICustomDataProcessor CreateProcessorCore(IEnumerable<IDataSource> dataSources, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
        {
            // Create a new instance of a class implementing ICustomDataProcessor here to process the specified data 
            // sources.
            // Note that you can have more advanced logic here to create different processors if you would like based 
            // on the file, or any other criteria.
            // You are not restricted to always returning the same type from this method.
            //
            
         
            string[] filePaths =new String[dataSources.Count()];
            int counter = 0;
            Console.WriteLine("Inside CreateProcessorCore");
            filePaths[0] = dataSources.First().Uri.LocalPath;
            //foreach (var item in dataSources)
            //{
             
            //    filePaths[counter] = item.Uri.LocalPath;
               
            //    Console.WriteLine("values so far : " + filePaths[counter]);
            //    counter++;

            //}

            return new SimpleCustomDataProcessor(filePaths,options,this.ApplicationEnvironment,processorEnvironment);
        }

        protected override bool IsDataSourceSupportedCore(IDataSource dataSource)
        {
            Console.WriteLine("Inside Data Source Supported core");
            if (!(dataSource is FileDataSource fileDataSource))
            {
                return false;
            }

            return Path.GetFileName(fileDataSource.FullPath).StartsWith("myData"); // This is what allows us to process the file
        }
        public MyProcessingSource() : base()
        {

        }

        public override ProcessingSourceInfo GetAboutInfo()
        {
            return new ProcessingSourceInfo
            {
                Owners = new[]
                {
                new ContactInfo
                {
                    Name = "The Gidraph",
                    Address = "The place",
                    EmailAddresses = new []
                    {
                        "someone@gmail.com"
                    },

                },
                },
                LicenseInfo = null,
                ProjectInfo = null,
                CopyrightNotice= $"Copyright (c) {DateTime.Now.Year}",
                AdditionalInformation = null,
            };
        }
    }
}
