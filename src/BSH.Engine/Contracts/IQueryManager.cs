// Copyright 2022 Alexander Seeliger
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Models;

namespace Brightbits.BSH.Engine.Contracts;
public interface IQueryManager
{
    Task<string> GetBackVersionWhereFileAsync(string startVersion, string searchString);
    Task<string> GetBackVersionWhereFilesInFolderAsync(string startVersion, string path);
    string GetFileNameFromDrive(FileTableRow file);
    Task<ValueTuple<string, bool>> GetFileNameFromDriveAsync(int versionId, string fileName, string filePath, string password);
    Task<List<FileTableRow>> GetFilesByVersionAsync(string version, string path);
    Task<List<string>> GetFolderListAsync(string version, string path);
    Task<string> GetFullRestoreFolderAsync(string folder, string version);
    Task<VersionDetails> GetLastBackupAsync();
    Task<VersionDetails> GetLastFullBackupAsync();
    Task<string> GetLocalizedPathAsync(string path);
    Task<string> GetNextVersionWhereFileAsync(string startVersion, string searchString);
    Task<string> GetNextVersionWhereFilesInFolderAsync(string startVersion, string path);
    Task<int> GetNumberOfVersionsAsync();
    Task<int> GetNumberOfFilesAsync();
    Task<double> GetTotalFileSizeAsync();
    Task<VersionDetails> GetOldestBackupAsync();
    Task<VersionDetails> GetVersionByIdAsync(string id);
    List<VersionDetails> GetVersions(bool desc = true);
    Task<List<FileTableRow>> GetVersionsByFileAsync(string fileName, string filePath);
    Task<bool> HasChangesOrNewAsync(string path, string versionId);
}