using Microsoft.Performance.SDK.Processing;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NpuPlugin
{ 
        [ProcessingSource("7309FAED-6A34-4FD1-8551-7AEB5006C71E",
                          "Npu Utilization Plugin",
                          "Processes Npu trace files exported as XML.")]
        [FileDataSource(".xml", "XML files exported from TRC files")]
        public class NpuProcessingSource
            : ProcessingSource
        {
            private IApplicationEnvironment applicationEnvironment;

        public override ProcessingSourceInfo GetAboutInfo()
        {
            return new ProcessingSourceInfo
            {
                CopyrightNotice = "Copyright 2024 Microsoft Corporation. All Rights Reserved.",
                LicenseInfo = new LicenseInfo
                {
                    Name = "MIT",
                    Text = "Please see the link for the full license text.",
                    Uri = "https://github.com/microsoft/microsoft-performance-toolkit-sdk/blob/main/LICENSE.txt",
                },
                Owners = new[]
             {
                    new ContactInfo
                    {
                        Address = "Microsoft ADC Kenya",
                        EmailAddresses = new[]
                        {
                            "noreply@microsoft.com",
                        },
                    },
                },
                ProjectInfo = new ProjectInfo
                {
                    Uri = "https://github.com/microsoft/microsoft-performance-toolkit-sdk",
                },
            };
    }


            protected override ICustomDataProcessor CreateProcessorCore(IEnumerable<IDataSource> dataSources,
                                                                        IProcessorEnvironment processorEnvironment,
                                                                        ProcessorOptions options)
            {
               

                var filePath = dataSources.First().Uri.LocalPath;
                return new NpuCustomDataProcessor(filePath,
                                                  options,
                                                  this.applicationEnvironment,
                                                  processorEnvironment);
            }

            protected override bool IsDataSourceSupportedCore(IDataSource source)
            {
                if (source is FileDataSource fileDataSource)
                {
                    using (var reader = new StreamReader(fileDataSource.FullPath))
                    {
                        reader.ReadLine();

                        var line = reader.ReadLine();

                        if (line != null)
                        {
                            return line.Contains(NpuPluginConstants.NpuXmlNamespace);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                return false;
            }

            protected override void SetApplicationEnvironmentCore(IApplicationEnvironment applicationEnvironment)
            {
                this.applicationEnvironment = applicationEnvironment;
            }
        }
    }

