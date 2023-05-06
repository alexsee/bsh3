// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace BSH.MainApp.Core.Contracts.Services;

public interface IFileService
{
    T Read<T>(string folderPath, string fileName);

    void Save<T>(string folderPath, string fileName, T content);

    void Delete(string folderPath, string fileName);
}
