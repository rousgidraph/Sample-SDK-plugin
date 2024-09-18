using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Processing;

namespace SamplePlugin
{
    //
    // Makes this table discoverable to SDK
    //
    [Table]
    public sealed class WordTable
    {
        private LineItem[] lineItems;
        private readonly ReadOnlyCollection<LineItem> myLineItemCollection;

        public WordTable(LineItem[] _lineItems)
        {
            lineItems = _lineItems;
            myLineItemCollection = new ReadOnlyCollection<LineItem>(lineItems); // check this line incase of an error 
            Console.WriteLine("After creating the table class here is the size "+myLineItemCollection.Count);
        }

        //
        // Exposes table information to SDK
        //
        public static TableDescriptor TableDescriptor => new TableDescriptor(
            Guid.Parse("{9AEBA80B-2635-46AC-B692-5E6FD609EBA1}"),
            "Word Stats",
            "Statistics for words",
            "words"
            );


        private static readonly ColumnConfiguration lineNumberColumn = new ColumnConfiguration(
                new ColumnMetadata(new Guid("8DACAF24-65DA-4490-A18F-A0792BCD83CD"), "Line Number"),
                new UIHints
                {
                    IsVisible = true,
                    Width = 100,
                });


        private static readonly ColumnConfiguration wordCountColumn = new ColumnConfiguration(
            new ColumnMetadata(new Guid("4952780B-7883-4E52-B4C6-0BE94E915002"), "Number of Words"),
            new UIHints
            {
                IsVisible = true,
                Width = 100
            });



        internal void Build(ITableBuilder tableBuilder)
        {

            ITableBuilderWithRowCount tableBuilderWithRowCount = tableBuilder.SetRowCount(lineItems.Length); // set row count limit 
            //
            // STEP 1: Create projections. For each column, we specify how to get data for a given row index
            //

            var baseProjection = Projection.Index(myLineItemCollection);

            var lineNumberProjection = baseProjection.Compose(lineItem => lineItem.LineNUmber);
            var wordCountProjection = baseProjection.Compose(lineItem => lineItem.Words.Count());
            //
            // BASE PROJECTION: gets a SqlEvent from a row index. Every projection
            // below will use this projection

            tableBuilderWithRowCount.AddColumn(lineNumberColumn, lineNumberProjection);
            tableBuilderWithRowCount.AddColumn(wordCountColumn, wordCountProjection);

            //
            // STEP 2: Create table configuration. We could have more than 1, but here we
            // only define 1 that includes all information.
            //

            //
            // When this configuration is applied, data will be grouped by the EventClassColumn (since that is
            // the only column before TableConfiguration.PivotColumn), and graphed by the RelativeTimestampColumn
            // (since that is the only column after TableConfiguration.GraphColumn).
            //

            TableConfiguration tableConfig = new TableConfiguration("All Data table")
            {
                Columns = new[]
            {
                    lineNumberColumn,
                    wordCountColumn
            }
            };

            tableConfig.AddColumnRole(ColumnRole.StartTime, wordCountColumn.Metadata.Guid);

            tableBuilder.AddTableConfiguration(tableConfig)
                    .SetDefaultTableConfiguration(tableConfig);
            Console.WriteLine("This is the end of the table builder method ");
        }



        //
        // Column definitions
        //


    }
}
