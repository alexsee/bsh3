// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace SimplePsd
{
    /// <summary>
    /// ThumbNail class
    /// </summary>
    public class ThumbNail
    {
        public int NFormat
        {
            get; set;
        }

        public int NWidth
        {
            get; set;
        }

        public int NHeight
        {
            get; set;
        }

        public int NWidthBytes
        {
            get; set;
        }

        public int NSize
        {
            get; set;
        }

        public int NCompressedSize
        {
            get; set;
        }

        public short NBitPerPixel
        {
            get; set;
        }

        public short NPlanes
        {
            get; set;
        }

        public byte[] Data
        {
            get; set;
        }

        public ThumbNail()
        {
            NFormat = -1;
            NWidth = -1;
            NHeight = -1;
            NWidthBytes = -1;
            NSize = -1;
            NCompressedSize = -1;
            NBitPerPixel = -1;
            NPlanes = -1;
        }
    }
}
