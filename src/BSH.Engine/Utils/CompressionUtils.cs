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
using System.Linq;

namespace Brightbits.BSH.Engine.Utils
{
    public static class CompressionUtils
    {
        private static readonly string[] EXCLUDED_FILE_EXTENSIONS =
        {
            ".zip",
            ".lnk",
            ".avi",
            ".cr2",
            ".jpg",
            ".gif",
            ".mov",
            ".mp4",
            ".nef",
            ".png",
            ".odg",
            ".odt",
            ".thm",
            ".docx",
            ".xlsx",
            ".pptx",
            ".7z",
            ".rar",
            ".jar",
            ".war",
            ".tgz",
            ".gz",
        };

        public static bool IsCompressedFile(string fileExt)
        {
            return EXCLUDED_FILE_EXTENSIONS.Contains(fileExt);
        }
    }
}
