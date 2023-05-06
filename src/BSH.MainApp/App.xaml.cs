// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Contracts.Storage;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Services;
using Brightbits.BSH.Engine.Storage;
using BSH.MainApp.Activation;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Core.Contracts.Services;
using BSH.MainApp.Core.Services;
using BSH.MainApp.Models;
using BSH.MainApp.Notifications;
using BSH.MainApp.Services;
using BSH.MainApp.ViewModels;
using BSH.MainApp.Views;
using H.NotifyIcon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace BSH.MainApp;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public TaskbarIcon? TrayIcon
    {
        get; private set;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static MainWindow MainWindow { get; } = new MainWindow();

    public static string DatabaseFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Alexosoft\Backup Service Home 3\backupservicehome.bshdb";

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers
            services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();

            // Services
            services.AddSingleton<IAppNotificationService, AppNotificationService>();
            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();

            // Core Services
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<IStatusService, StatusService>();
            services.AddTransient<IWaitForMediaService, WaitForMediaService>();
            services.AddSingleton<IScheduledBackupService, ScheduledBackupService>();
            services.AddSingleton<IOrchestrationService, OrchestrationService>();
            services.AddSingleton<IJobService, JobService>();
            services.AddSingleton<IPresentationService, PresentationService>();

            // Engine Services
            services.AddSingleton<IConfigurationManager, ConfigurationManager>();
            services.AddSingleton<IQueryManager, QueryManager>();

            services.AddSingleton<IDbClientFactory, DbClientFactory>();
            services.AddSingleton<IDbMigrationService, DbMigrationService>();

            services.AddSingleton<IBackupService, BackupService>();

            services.AddSingleton<IStorageFactory, StorageFactory>();

            services.AddSingleton<DispatcherQueue>((x) => DispatcherQueue.GetForCurrentThread());

            // Views and ViewModels
            services.AddTransient<BrowserViewModel>();
            services.AddTransient<BrowserPage>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainPage>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsPage>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        App.GetService<IAppNotificationService>().Initialize();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        // init database, read configuration, and perform potential schema migrations
        await App.GetService<IDbClientFactory>().InitializeAsync(DatabaseFile);
        await App.GetService<IConfigurationManager>().InitializeAsync();
        await App.GetService<IDbMigrationService>().InitializeAsync();

        // init scheduled backups
        await App.GetService<IScheduledBackupService>().InitializeAsync();

        // start application
        App.GetService<IStatusService>().Initialize();

        await App.GetService<IOrchestrationService>().InitializeAsync();
        await App.GetService<IActivationService>().ActivateAsync(args);

        InitializeTrayIcon();
    }

    private void InitializeTrayIcon()
    {
        var exitApplicationCommand = (XamlUICommand)Resources["ExitApplicationCommand"];
        exitApplicationCommand.ExecuteRequested += ExitApplicationCommand_ExecuteRequested;

        TrayIcon = (TaskbarIcon)Resources["TrayIcon"];
        TrayIcon.ForceCreate();
    }

    private void ExitApplicationCommand_ExecuteRequested(object? _, ExecuteRequestedEventArgs args)
    {
        TrayIcon?.Dispose();
        Environment.Exit(0);
    }
}
