namespace SimplePsd
{
    /// <summary>
    /// Image resource block
    /// </summary>
    public class ImageResource
    {
        public int NLength
        {
            get; set;
        }

        public byte[] OSType { get; set; } = new byte[4];

        public short NID
        {
            get; set;
        }

        public byte[] Name
        {
            get; set;
        }

        public int NSize
        {
            get; set;
        }

        public void Reset()
        {
            NLength = -1;
            for (int i = 0; i < 4; i++)
            {
                OSType[i] = 0x00;
            }

            NID = -1;
            NSize = -1;
        }

        public ImageResource()
        {
            Reset();
        }
    }
}
