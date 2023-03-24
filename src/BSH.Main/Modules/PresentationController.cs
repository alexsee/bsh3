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

using BSH.Main.Dialogs.SubDialogs;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    /// <summary>
    /// The PresentationController is responsible for handling all window and user interface
    /// activities (e.g., show and hide windows).
    /// </summary>
    public class PresentationController
    {
        private static PresentationController _presentationController;

        public static PresentationController Current
        {
            get
            {
                if (_presentationController == null)
                {
                    _presentationController = new PresentationController();
                }

                return _presentationController;
            }
        }

        private frmStatusBackup statusWindow;

        public void ShowStatusWindow()
        {
            if (statusWindow != null)
            {
                if (!statusWindow.IsDisposed)
                {
                    return;
                }

                statusWindow.Dispose();
            }

            statusWindow = new frmStatusBackup();
            statusWindow.Show();
            StatusController.Current.AddObserver(statusWindow);
        }

        public TaskCompleteAction CloseStatusWindow()
        {
            if (statusWindow == null)
            {
                return TaskCompleteAction.NoAction;
            }

            StatusController.Current.RemoveObserver(statusWindow);
            TaskCompleteAction CloseStatusWindowRet;
            if (statusWindow.chkOptions.Checked && statusWindow.cboOptions.SelectedIndex == 0)
            {
                CloseStatusWindowRet = TaskCompleteAction.ShutdownPC;
            }
            else if (statusWindow.chkOptions.Checked && statusWindow.cboOptions.SelectedIndex == 1)
            {
                CloseStatusWindowRet = TaskCompleteAction.HibernatePC;
            }
            else
            {
                CloseStatusWindowRet = TaskCompleteAction.NoAction;
            }

            statusWindow.Close();
            statusWindow.Dispose();
            statusWindow = null;
            return CloseStatusWindowRet;
        }

        private frmMain mainWindow;

        public void ShowMainWindow()
        {
            if (mainWindow != null)
            {
                if (!mainWindow.IsDisposed)
                {
                    return;
                }

                mainWindow.Dispose();
            }

            mainWindow = new frmMain();
            mainWindow.Show();
        }

        public void CloseMainWindow()
        {
            if (mainWindow == null)
            {
                return;
            }

            mainWindow.Close();
            mainWindow.Dispose();
            mainWindow = null;
        }

        private frmBrowser browserWindow;

        public void ShowBackupBrowserWindow()
        {
            if (browserWindow != null)
            {
                if (!browserWindow.IsDisposed)
                {
                    return;
                }

                browserWindow.Dispose();
            }

            browserWindow = new frmBrowser();
            browserWindow.Show();
        }

        public void CloseBackupBrowserWindow()
        {
            if (browserWindow == null)
            {
                return;
            }

            browserWindow.Close();
            browserWindow.Dispose();
            browserWindow = null;
        }


        public void ShowAboutWindow(IWin32Window owner)
        {
            using (var aboutWindow = new frmAbout())
            {
                aboutWindow.ShowDialog(owner);
            }
        }

        public (string password, bool persist) RequestPassword()
        {
            using (var passwordWindow = new frmPassword())
            {
                if (passwordWindow.ShowDialog() == DialogResult.OK)
                {
                    return (passwordWindow.txtPassword.Text, passwordWindow.chkSavePwd.Checked);
                }
            }

            return (null, false);
        }

        public void ShowErrorInsufficientDiskSpace()
        {
            using (var errorWindow = new frmErrorInsufficientDiskSpace())
            {
                errorWindow.ShowDialog();
            }
        }

        public async Task ShowCreateBackupWindow()
        {
            using (var dlgCreateBackup = new frmCreateBackup())
            {
                if (dlgCreateBackup.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                // retrieve sources
                string sources = string.Join("|", dlgCreateBackup.clstSources.CheckedItems.Cast<string>());
                if (string.IsNullOrEmpty(sources))
                {
                    return;
                }

                // start backup
                await BackupLogic.BackupController.CreateBackupAsync(dlgCreateBackup.txtTitle.Text, dlgCreateBackup.txtDescription.Text, true, dlgCreateBackup.cbFullBackup.Checked, dlgCreateBackup.chkShutdownPC.Checked, sourceFolders: sources);
            }
        }
    }
}