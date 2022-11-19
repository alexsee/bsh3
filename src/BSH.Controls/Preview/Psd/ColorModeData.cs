namespace SimplePsd
{
    /// <summary>
    /// ColorModeData class
    /// </summary>
    public class ColorModeData
    {
        public int NLength { get; set; }

        public byte[] ColourData { get; set; }

        public ColorModeData()
        {
            NLength = -1;
        }
    }
}
