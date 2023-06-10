﻿// Copyright 2022 Alexander Seeliger
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

namespace Brightbits.BSH.Engine.Models;

public class ExplorerWindow
{
    public string Path
    {
        get; set;
    }

    public string WindowTitle
    {
        get; set;
    }

    public ExplorerWindow(string path, string windowTitle)
    {
        Path = path;
        WindowTitle = windowTitle;
    }
}
