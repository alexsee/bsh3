namespace SimplePsd
{
    /// <summary>
    /// This structure contains display information about each channel
    /// </summary>
    public class DisplayInfo
    {
        public short ColourSpace
        {
            get; set;
        }

        public short[] Colour { get; set; } = new short[4];

        public short Opacity
        {
            get; set;
        }

        public bool Kind
        {
            get; set;
        }

        public byte Padding
        {
            get; set;
        }

        public DisplayInfo()
        {
            ColourSpace = -1;
            for (int n = 0; n < 4; ++n)
            {
                Colour[n] = 0;
            }

            Opacity = -1;
            Kind = false;
            Padding = 0x00;
        }
    }
}
