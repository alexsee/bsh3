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

using CommandLine;

namespace BSH.Main.Model.CommandLine;

public class Options
{
    [Option("delayedstart", Default = false)]
    public bool DelayedStart
    {
        get; set;
    }

    [Option("databasefile")]
    public string DatabaseFile
    {
        get; set;
    }

    [Option("deleteprotocol", Default = false)]
    public bool DeleteProtocol
    {
        get; set;
    }

    [Option("config")]
    public bool ShowConfig
    {
        get; set;
    }

    [Option("browser")]
    public bool ShowBrowser
    {
        get; set;
    }
}
