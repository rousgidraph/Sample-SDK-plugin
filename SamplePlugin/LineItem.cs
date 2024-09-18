namespace SamplePlugin
{

    public class LineItem
    {
        public int LineNUmber { get; set; }
        public String LineContent { get; set; }
        public DateTime TimeStamp { get; set; }
        public string ProcessName { get; set; }
        public string ProcessID { get; set; }
        public string EventDescription { get; set; }
        public List<string> Words { get; set; }
    }

}

