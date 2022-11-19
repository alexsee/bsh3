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
using System.ComponentModel;
using static Brightbits.BSH.Engine.Win32Stuff;

namespace Brightbits.BSH.Engine.Security
{
    public class NetworkConnection : IDisposable
    {
        public string RemoteShare { get; set; } = null;

        public NetworkConnection(string remoteHost, string remoteUser, string remotePassword)
        {
            if (string.IsNullOrEmpty(remoteUser) || string.IsNullOrEmpty(remotePassword) || !Uri.TryCreate(remoteHost, UriKind.Absolute, out Uri loc) || !loc.IsUnc)
            {
                return;
            }

            string auth = loc.Host;
            string[] segments = loc.Segments;

            // decrypt password
            var pw = Crypto.DecryptString(remotePassword, System.Security.Cryptography.DataProtectionScope.LocalMachine);

            if (pw.Length == 0)
            {
                return;
            }

            // expected format is '\\machine\share'
            this.RemoteShare = String.Format(@"\\{0}\{1}", auth, segments[1].Trim('\\', '/'));
            this.Connect(remoteUser, Crypto.ToInsecureString(pw));
        }

        ~NetworkConnection()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Disconnect();
        }

        void Connect(string remoteUser, string remotePassword)
        {
            var netResource = new NetResource()
            {
                Scope = ResourceScope.GlobalNetwork,
                ResourceType = ResourceType.Disk,
                DisplayType = ResourceDisplaytype.Share,
                RemoteName = this.RemoteShare
            };

            // user must be qualified 'authority\user'
            if (remoteUser.IndexOf('\\') < 0)
            {
                remoteUser = String.Format(@"{0}\{1}", new Uri(RemoteShare).Host, remoteUser);
            }

            int dwResult = WNetAddConnection2(netResource, remotePassword, remoteUser, 0);

            // are we already connected?
            if (1219 == dwResult)
            {
                RemoteShare = null;
                return;
            }

            if (0 != dwResult)
            {
                throw new Win32Exception(dwResult);
            }
        }

        void Disconnect()
        {
            if (RemoteShare != null)
            {
                WNetCancelConnection2(this.RemoteShare, 0, true);
            }
        }
    }
}
