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

using System.Text;

namespace Brightbits.BSH.Engine.Security
{
    public static class Hash
    {
        public static string GetMD5Hash(string input)
        {
            // create a new instance of the MD5 object
            System.Security.Cryptography.MD5.Create();
            var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();

            // convert the input string to a byte array and compute the hash
            var data = md5.ComputeHash(Encoding.Default.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes and create a string.
            var sBuilder = new StringBuilder();

            // loop through each byte of the hashed data and format each one as a hexadecimal string
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string
            return sBuilder.ToString();
        }
    }
}
