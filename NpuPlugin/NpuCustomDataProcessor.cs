using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace NpuPlugin
{
   public class NpuCustomDataProcessor : CustomDataProcessor

    {
            private readonly string filePath;

            private DataSourceInfo dataSourceInfo;

            private ReadOnlyCollection<NpuEventWithRelativeTimestamp> npuEvents;
            public NpuCustomDataProcessor(string filePath,
                                          ProcessorOptions options,
                                          IApplicationEnvironment applicationEnvironment,
                                          IProcessorEnvironment processorEnvironment)
                : base(options, applicationEnvironment, processorEnvironment)
            {
                this.filePath = filePath;
            }

            public override DataSourceInfo GetDataSourceInfo()
            {
                return this.dataSourceInfo;
            }

            protected override Task ProcessAsyncCore(
               IProgress<int> progress,
               CancellationToken cancellationToken)
            {
                List<NpuEvent> npuEvents = ParseXml(progress);

                var startEvent = npuEvents.First();
                var startTimestamp = Timestamp.FromNanoseconds(startEvent.StartTime.Ticks * 100);

                var endEvent = npuEvents.Last();
                var endTimestamp = Timestamp.FromNanoseconds(endEvent.StartTime.Ticks * 100);

                npuEvents = npuEvents.Except(new NpuEvent[] { startEvent, endEvent }).ToList();

                var npuEventsWithRelativeTimestamp = npuEvents.Select(npuEvent =>
                {
                    var offset = npuEvent.StartTime.Subtract(startEvent.StartTime);
                    var relativeTimestamp = Timestamp.FromNanoseconds(offset.Ticks * 100);
                    return new NpuEventWithRelativeTimestamp(npuEvent, relativeTimestamp);
                }).ToList();

                
                this.dataSourceInfo = new DataSourceInfo(0, (endTimestamp - startTimestamp).ToNanoseconds, startEvent.StartTime);
                this.npuEvents = new ReadOnlyCollection<NpuEventWithRelativeTimestamp>(npuEventsWithRelativeTimestamp);

               
                progress.Report(100);
                return Task.CompletedTask;
            }

         
            private List<NpuEvent> ParseXml(IProgress<int> progress)
            {
                var npuEvents = new List<NpuEvent>();
                using (FileStream stream = File.OpenRead(this.filePath))
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    bool inEvents = false;
                    string eventClass = null;

                    string textData = string.Empty, applicationName = string.Empty, ntUserName = string.Empty, loginName = string.Empty;
                    int? cpu = null, reads = null, writes = null, duration = null, clientProcessId = null, spid = null;
                    DateTime? startTime = null, endTime = null;

                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:

                                if (reader.Name == "Events")
                                {
                                    inEvents = true;
                                }
                                else if (inEvents && reader.Name == "Event")
                                {
                                    eventClass = reader.GetAttribute("name");
                                }
                                else if (eventClass != null && reader.Name == "Column")
                                {
                                    switch (reader.GetAttribute("name"))
                                    {
                                        case "TextData":
                                            textData = reader.ReadElementContentAsString();
                                            break;
                                        case "ApplicationName":
                                            applicationName = reader.ReadElementContentAsString();
                                            break;
                                        case "NTUserName":
                                            ntUserName = reader.ReadElementContentAsString();
                                            break;
                                        case "LoginName":
                                            loginName = reader.ReadElementContentAsString();
                                            break;
                                        case "CPU":
                                            cpu = reader.ReadElementContentAsInt();
                                            break;
                                        case "Reads":
                                            reads = reader.ReadElementContentAsInt();
                                            break;
                                        case "Writes":
                                            writes = reader.ReadElementContentAsInt();
                                            break;
                                        case "Duration":
                                            duration = reader.ReadElementContentAsInt();
                                            break;
                                        case "ClientProcessID":
                                            clientProcessId = reader.ReadElementContentAsInt();
                                            break;
                                        case "SPID":
                                            spid = reader.ReadElementContentAsInt();
                                            break;
                                        case "StartTime":
                                            startTime = reader.ReadElementContentAsDateTime().ToUniversalTime();
                                            break;
                                        case "EndTime":
                                            endTime = reader.ReadElementContentAsDateTime().ToUniversalTime();
                                            break;
                                    }
                                }

                                break;

                            case XmlNodeType.EndElement:

                                if (reader.Name == "Events")
                                {
                                    inEvents = false;
                                }
                                else if (inEvents && eventClass != null && reader.Name == "Event")
                                {
                                    var npuEvent = new NpuEvent(eventClass,
                                                textData,
                                                applicationName,
                                                ntUserName,
                                                loginName,
                                                cpu,
                                                reads,
                                                writes,
                                                duration,
                                                clientProcessId,
                                                spid,
                                                startTime.Value,
                                                endTime);
                                    eventClass = null;
                                    textData = string.Empty;
                                    applicationName = string.Empty;
                                    ntUserName = string.Empty;
                                    loginName = string.Empty;
                                    cpu = null;
                                    reads = null;
                                    writes = null;
                                    duration = null;
                                    clientProcessId = null;
                                    spid = null;
                                    startTime = null;
                                    endTime = null;

                                    npuEvents.Add(npuEvent);
                                }

                                break;
                        }

                        progress.Report((int)(100.0 * stream.Position / stream.Length));
                    }
                }

                return npuEvents;
            }

            protected override void BuildTableCore(TableDescriptor tableDescriptor, ITableBuilder tableBuilder)
            {

                var table = new NpuTable(this.npuEvents);
                table.Build(tableBuilder);
            }
        }
    }

