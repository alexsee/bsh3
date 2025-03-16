// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.IO;

namespace C4F.DevKit.PreviewHandler.PreviewHandlerFramework
{
    public abstract class FileBasedPreviewHandler : PreviewHandler, IInitializeWithFile
    {
        private string _filePath;

        void IInitializeWithFile.Initialize(string pszFilePath, uint grfMode)
        {
            _filePath = pszFilePath;
        }

        protected override void Load(PreviewHandlerControl c)
        {
            c.Load(new FileInfo(_filePath));
        }
    }
}
