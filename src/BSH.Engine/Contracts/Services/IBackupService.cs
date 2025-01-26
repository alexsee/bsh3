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

using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;

namespace Brightbits.BSH.Engine.Contracts.Services;
public interface IBackupService
{
    bool CheckMedia(bool quickCheck = false);
    string GetPassword();
    bool HasPassword();
    void SetPassword(string password);
    Task SetStableAsync(string version, bool stable);
    Task UpdateVersionAsync(string version, VersionDetails versionDetails);
    Task StartBackup(string title, string description, ref IJobReport jobReport, CancellationToken cancellationToken, bool fullBackup = false, string sources = "", bool silent = false);
    Task StartDelete(string version, ref IJobReport jobReport, CancellationToken cancellationToken, bool silent = false);
    Task StartDeleteSingle(string fileFilter, string pathFilter, ref IJobReport jobReport, CancellationToken cancellationToken, bool silent = false);
    Task StartEdit(ref IJobReport jobReport, CancellationToken cancellationToken, bool silent = false);
    Task StartRestore(string version, string file, string destination, ref IJobReport jobReport, CancellationToken cancellationToken, FileOverwrite overwrite = FileOverwrite.Ask, bool silent = false);
    void UpdateDatabaseFile(string databaseFile);
}