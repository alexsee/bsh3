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

using System.Collections.Generic;
using Brightbits.BSH.Engine.Models;

namespace Brightbits.BSH.Engine.Contracts.Services;
public interface IFileCollectorService
{
    List<FolderTableRow> EmptyFolders
    {
        get;
        set;
    }

    List<FileTableRow> GetLocalFileList(string root, bool subFolders = true);
}