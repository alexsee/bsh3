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

using BSH.Service.Shared;
using ServiceWire.NamedPipes;
using System.ServiceProcess;

namespace BSH.Service
{
    public partial class Service : ServiceBase
    {
        private NpHost host;

        private IVSSRemoteObject remoteObject;

        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // init remote object
            remoteObject = new VSSService();

            // create remote rpc server
            host = new NpHost("backupservicehome");
            host.AddService<IVSSRemoteObject>(remoteObject);
            host.Open();
        }

        protected override void OnStop()
        {
            // close remote rpc server
            host.Close();
        }
    }
}
