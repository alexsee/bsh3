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

using AutoUpdaterDotNET;
using BSH.Main.Model.CommandLine;
using BSH.Main.Properties;
using CommandLine;
using Serilog;
using System;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;

namespace Brightbits.BSH.Main;

static class Program
{
    public const string APP_TITLE = "Backup Service Home";

    private static System.Threading.Mutex mutex;

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

            // setup software updater
            var uniqueUserId = GetUniqueUserId();

            AutoUpdater.ApplicationExitEvent += AutoUpdater_ApplicationExitEvent;
            AutoUpdater.HttpUserAgent = $"{APP_TITLE}/{CurrentVersion} {uniqueUserId}";
            AutoUpdater.TopMost = true;
            AutoUpdater.CheckForUpdateEvent += AutoUpdater_CheckForUpdateEvent;

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

    private static void AutoUpdater_CheckForUpdateEvent(UpdateInfoEventArgs args)
    {
        if (!args.IsUpdateAvailable)
        {
            Log.Information("No updates founds; Current version: {}", CurrentVersion);

            MessageBox.Show(Resources.MSG_NO_UPDATE_FOUND_TEXT, Resources.MSG_NO_UPDATE_FOUND_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        AutoUpdater.ShowUpdateForm(args);
    }

    private static void AutoUpdater_ApplicationExitEvent()
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
        mutex = new System.Threading.Mutex(true, "BackupServiceHome3", out var exclusive);
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

    public static string GetUniqueUserId()
    {
        if (string.IsNullOrEmpty(Settings.Default.UniqueUserId))
        {
            Settings.Default.UniqueUserId = Guid.NewGuid().ToString();
            Settings.Default.Save();
        }

        return Settings.Default.UniqueUserId;
    }
}
