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

namespace BSH.Main.Model.CommandLine
{
    [Verb("startbackup", HelpText = "Starts a manual backup with default configuration.")]
    public class StartBackupCommandOptions
    {
        [Option("title", Default = "Manuelle Sicherung", HelpText = "Specifies the title of the backup.")]
        public string Title { get; set; }

        [Option("description", Default = "", HelpText = "Specifies the description of the backup.")]
        public string Description { get; set; }

        [Option("shutdownapp", Default = false)]
        public bool ShutdownApp { get; set; }

        [Option("shutdownpc", Default = false)]
        public bool ShutdownPC { get; set; }

        [Option("autodelete", Default = false)]
        public bool AutoDeletion { get; set; }
    }
}
