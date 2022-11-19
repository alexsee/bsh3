namespace SimplePsd
{
    /// <summary>
    /// Summary description for HeaderInfo class.
    /// </summary>
    public class HeaderInfo
    {
        public short NChannels { get; set; }

        public int NHeight { get; set; }

        public int NWidth { get; set; }

        public short NBitsPerPixel { get; set; }

        public short NColourMode { get; set; }

        public HeaderInfo()
        {
            NChannels = -1;
            NHeight = -1;
            NWidth = -1;
            NBitsPerPixel = -1;
            NColourMode = -1;
        }
    }

}
