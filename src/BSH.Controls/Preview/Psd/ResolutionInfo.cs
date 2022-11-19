namespace SimplePsd
{
    /// <summary>
    /// ResolutionInfo class
    /// </summary>
    public class ResolutionInfo
    {
        public short HRes { get; set; }

        public int HResUnit { get; set; }

        public short WidthUnit { get; set; }

        public short VRes { get; set; }

        public int VResUnit { get; set; }

        public short HeightUnit { get; set; }

        public ResolutionInfo()
        {
            HRes = -1;
            HResUnit = -1;
            WidthUnit = -1;
            VRes = -1;
            VResUnit = -1;
            HeightUnit = -1;
        }
    }
}
