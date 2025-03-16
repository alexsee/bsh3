// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.IO;

namespace C4F.DevKit.PreviewHandler.PreviewHandlerFramework
{
    public abstract class StreamBasedPreviewHandlerControl : PreviewHandlerControl
    {
        public sealed override void Load(FileInfo file)
        {
            using (FileStream fs = new FileStream(file.FullName,
                FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                Load(fs);
            }
        }
    }
}
