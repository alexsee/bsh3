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

using BSH.Main.Model.CommandLine;
using BSH.Main.Properties;
using CommandLine;
using Serilog;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    static class Program
    {
        public const string APP_TITLE = "Backup Service Home";

        private static System.Threading.Mutex mutex;

#if !WIN_UWP
        private static updateSystemDotNet.updateController _mainUpdateController;

        public static updateSystemDotNet.updateController mainUpdateController
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mainUpdateController;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mainUpdateController != null)
                {
                    _mainUpdateController.updateFound -= mainUpdateController_updateFound;
                    _mainUpdateController.updateInstallerStarted -= mainUpdateController_updateInstallerStarted;
                }

                _mainUpdateController = value;
                if (_mainUpdateController != null)
                {
                    _mainUpdateController.updateFound += mainUpdateController_updateFound;
                    _mainUpdateController.updateInstallerStarted += mainUpdateController_updateInstallerStarted;
                }
            }
        }
#endif

        public static string CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        [STAThread()]
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().ReadFrom.AppSettings().CreateLogger();
            Log.Information($"{APP_TITLE} {CurrentVersion} started.");

            // set current culture
            Application.EnableVisualStyles();
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("de-DE");

            CheckSingleInstance();

            try
            {
                // load application settings
                Settings.Default.Reload();
                Parser.Default.ParseArguments<Options>(args)
                    .WithParsed(RunOptions);

                // activate text rendering
                Application.SetCompatibleTextRenderingDefault(true);

                // activate global exception handling
                Application.ThreadException += ExceptionController.HandleGlobalException;
                AppDomain.CurrentDomain.UnhandledException += ExceptionController.HandleGlobalException;
                Application.ApplicationExit += ApplicationExit;

                NotificationController.Current.InitializeSystemTrayIcon();
                Microsoft.Win32.SystemEvents.SessionEnding += Shutdown;

#if !WIN_UWP
                // init UpdateSystem
                mainUpdateController = new updateSystemDotNet.updateController
                {
                    applicationLocation = "",
                    projectId = "0d342c3a-392f-417e-974f-ca0715d39283",
                    proxyPassword = null,
                    proxyUrl = null,
                    proxyUsername = null,
                    publicKey = "<RSAKeyValue><Modulus>uX5wAIeaOlLNQZ/X3+0vtwf4OZJwjm9odouMWhyQSatjL8marLBGlaSfRYMjP1MxYOdUtJl6z4MkfNQ6K18qoKj0NPxgZAfpofUou7zZRVoUEFV8exLTi5WYacYPP7N3ujC4DkNPbd9335CQSa9RAfzDflVS1YKjH7SV0vN86E0OIHvokfBLOEf5KwLC/mlDpMh7XDAGZXcDwP8Hu29ZBf8BcJfyjlYFgK+EVzPdm8KWdM1zwIkBeJkZ/87NNTEsQiFz5lHAWI1MmfGR3aGgTWyqlPoWLKaX+nFLMmQqFwjTx7r2KIoTdka5Nv/GMEoHqAntPU5F39D3IppnthHJODJLhncoF7Dx5rQgrCbMRKfvkjOZ/VwxwFA22k5kIcnxWIIpmDb51rlcVF2nYjR3e5Z4wqRMKNeMUenCuyD7Yi7TxQGAfLy6CS6p138SdTJ8dZ07HtPt55V6cQmAzYkJL4Pw7pL/4d+Lqs3Mdp6qAjF1VO8JIeIT9/XO9I59H3aL8qFKLuKLHqF7x2Lrjitsk1PJh/cjHWv0DVWth96SzG5rV9DHwhzZf8hCwWmn4Nw1mF2d2pCxAB46Y/ee0Pzu5QWxN3qbomhllroZrGz7uxvqrOwsCCf4duE5Vhv7JbFVO6288izWvFaIjysf1SskJEbxseG1DFiD/yZYitSPIlE=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>",
                    requestElevation = true,
                    updateUrl = "https://updates.brightbits.de/_bsh_3",
                    restartApplication = true,
                };
                mainUpdateController.releaseInfo.Version = Application.ProductVersion;

                if (Settings.Default.AutoSearchUpdates)
                {
                    mainUpdateController.updateCheckInterval = updateSystemDotNet.Interval.Weekly;
                    mainUpdateController.checkForUpdatesAsync();
                }
                else
                {
                    mainUpdateController.updateCheckInterval = updateSystemDotNet.Interval.Custom;
                }
#endif

                // start backup engine
                BackupLogic.StartupAsync().Wait();

                // parse command line if system is configured
                if (BackupLogic.ConfigurationManager.IsConfigured == "1")
                {
                    CheckCommands(args);
                }

                Settings.Default.StartParameters = "";
                Settings.Default.Save();

                Application.Run();
            }
            catch (Exception ex)
            {
                ExceptionController.HandleGlobalException(null, new System.Threading.ThreadExceptionEventArgs(ex));
            }
        }

        private static void ApplicationExit(object sender, EventArgs e)
        {
            Log.CloseAndFlush();
        }

        private static void Shutdown(object sender, Microsoft.Win32.SessionEndingEventArgs e)
        {
            Log.Information("Backup Service Home stopped");
            Log.CloseAndFlush();

            BackupLogic.StopSystem();

            Application.Exit();
            Environment.Exit(0);
        }

        private static void CheckSingleInstance()
        {
            mutex = new System.Threading.Mutex(true, "BackupServiceHome3", out bool exclusive);
            if (!exclusive)
            {
                Log.Warning("Another instance is already running. Exiting...");
                Log.CloseAndFlush();

                Environment.Exit(0);
            }
        }

        private static void RunOptions(Options opts)
        {
            // delayed start
            if (opts.DelayedStart)
            {
                System.Threading.Thread.Sleep(10000);
            }

            // set database file
            if (!string.IsNullOrEmpty(opts.DatabaseFile) && System.IO.File.Exists(opts.DatabaseFile))
            {
                BackupLogic.DatabaseFile = opts.DatabaseFile;
            }

            // (deprecated)
            if (opts.DeleteProtocol)
            {
                Application.Exit();
                Environment.Exit(0);
                return;
            }
        }

        private static void CheckCommands(string[] args)
        {
            Parser.Default.ParseArguments<ShowConfigurationWindowOptions, StartBackupCommandOptions, ShowBackupbrowserCommandOptions>(args)
                .MapResult(
                    (ShowConfigurationWindowOptions _) => ShowConfigurationWindowExitCode(),
                    (StartBackupCommandOptions opts) => StartBackupCommandExitCode(opts),
                    (ShowBackupbrowserCommandOptions _) => ShowBackupbrowserWindowExitCode(),
                    _ => 1
                );

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(HandleOptions);
        }

        private static int ShowConfigurationWindowExitCode()
        {
            // show configuration window
            PresentationController.Current.ShowMainWindow();

            return 0;
        }

        private static int ShowBackupbrowserWindowExitCode()
        {
            // show backup browser window
            PresentationController.Current.ShowBackupBrowserWindow();

            return 0;
        }

        private static int StartBackupCommandExitCode(StartBackupCommandOptions opts)
        {
            // run automatic backup deletion
            if (opts.AutoDeletion)
            {
                BackupLogic.CommandAutoDelete();
            }

            // create manual backup
            BackupLogic.BackupController.CreateBackupAsync(opts.Title, opts.Description, true, shutdownPC: opts.ShutdownPC, shutdownApp: opts.ShutdownApp);

            return 0;
        }

        private static void HandleOptions(Options options)
        {
            if (options.ShowConfig)
            {
                ShowConfigurationWindowExitCode();
            }

            if (options.ShowBrowser)
            {
                ShowBackupbrowserWindowExitCode();
            }
        }

#if !WIN_UWP
        private static void mainUpdateController_updateFound(object sender, updateSystemDotNet.appEventArgs.updateFoundEventArgs e)
        {
            NotificationController.Current.ShowIconBalloon(5000, Resources.INFO_BSH_UPDATE_AVAILABLE_TITLE, Resources.INFO_BSH_UPDATE_AVAILABLE_TEXT, ToolTipIcon.Info);
        }

        private static void mainUpdateController_updateInstallerStarted(object sender, updateSystemDotNet.appEventArgs.updateInstallerStartedEventArgs e)
        {
            // close application
            NotificationController.Current.Shutdown();
            BackupLogic.StopSystem();
            Settings.Default.StartParameters = Environment.CommandLine;
            Settings.Default.Save();

            PresentationController.Current.CloseMainWindow();
            PresentationController.Current.CloseBackupBrowserWindow();

            try
            {
                Application.Exit();
            }
            catch
            {
                Environment.Exit(0);
            }
        }
#endif
    }
}
