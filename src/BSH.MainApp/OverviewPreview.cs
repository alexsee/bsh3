// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

#if DEBUG
using System.Globalization;
using BSH.MainApp.Models;
using BSH.MainApp.Views;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;

namespace BSH.MainApp;

/// <summary>Opt-in, read-only screenshot fixtures. No database, scheduler or backup is started.</summary>
internal static class OverviewPreview
{
    public static bool IsRequested => Environment.GetCommandLineArgs().Contains("--overview-preview");

    public static bool TryShow()
    {
        var args = Environment.GetCommandLineArgs();
        if (!IsRequested)
        {
            return false;
        }

        var page = new MainPage();
        page.Loaded += (_, _) => page.DispatcherQueue.TryEnqueue(
            Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () => Populate(page, args));
        var window = App.MainWindow;
        var root = (FrameworkElement)window.Content;
        root.RequestedTheme = args.Contains("--dark") ? ElementTheme.Dark : ElementTheme.Light;
        // Keep fixture commands and shell navigation from touching real services.
        root.IsHitTestVisible = false;
        root.PreviewKeyDown += (_, e) => e.Handled = true;
        window.ContentArea.Content = page;
        window.AppWindow.Resize(new global::Windows.Graphics.SizeInt32(args.Contains("--narrow") ? 1060 : 1680, 1080));
        window.Activate();
        return true;
    }

    private static void Populate(MainPage page, string[] args)
    {
        var vm = page.ViewModel;
        vm.ActionItems.Clear();
        vm.HasActionItems = false;
        vm.Severity = OverviewSeverity.Protected;
        vm.StatusTitle = "Deine Dateien sind geschützt";
        vm.StatusText = "Alles auf dem neuesten Stand. Deine nächste Sicherung startet automatisch.";
        vm.HasLastBackup = true;
        vm.LastBackupWhen = "Heute, 09:41 Uhr";
        vm.LastBackupResultText = "Erfolgreich abgeschlossen";
        vm.LastBackupName = "Dokumente & Erinnerungen";
        vm.HasLastBackupName = true;
        vm.HasChangeStatistics = true;
        vm.AddedFilesText = string.Format(CultureInfo.CurrentCulture, "Overview_Changes_Added".GetLocalized()!, 24);
        vm.ModifiedFilesText = string.Format(CultureInfo.CurrentCulture, "Overview_Changes_Modified".GetLocalized()!, 8);
        vm.DeletedFilesText = string.Format(CultureInfo.CurrentCulture, "Overview_Changes_Deleted".GetLocalized()!, 2);
        vm.LocationKindText = "Lokales Laufwerk / Netzwerk";
        vm.LocationPath = @"E:\Sicherungen\Zuhause";
        vm.LocationConnectionText = "Verbunden und bereit";
        vm.FreeSpaceText = "684 GB verfügbar";
        vm.TotalsText = "32 Sicherungen · 18.426 Dateien · 126,4 GB gesichert";
        vm.ModeAndNextBackupText = "Zeitplan · Nächste Sicherung heute um 18:00 Uhr";
        vm.CanToggleSystem = true;

        if (args.Contains("--warning"))
        {
            vm.Severity = OverviewSeverity.Warning;
            vm.StatusTitle = "Einige Dateien wurden übersprungen";
            vm.StatusText = "Deine Sicherung ist verfügbar. Prüfe die Dateien, die nicht gesichert werden konnten.";
            vm.LastBackupResultSeverity = OverviewSeverity.Warning;
            vm.LastBackupResultText = "Abgeschlossen mit 3 übersprungenen Dateien";
            vm.ActionItems.Add(new ViewModels.OverviewActionItem(
                OverviewIssueKind.FilesSkipped, OverviewSeverity.Warning,
                "3 Dateien waren während der letzten Sicherung nicht verfügbar.", "Dateien prüfen", vm.ViewFailedFilesCommand));
            vm.HasActionItems = true;
        }
        if (args.Contains("--running"))
        {
            vm.StatusTitle = "Deine Dateien werden gesichert";
            vm.StatusText = "Du kannst weiterarbeiten. Wir kümmern uns um deine Sicherung.";
            vm.NextBackupGridVisibility = false;
            vm.ProgressGridVisibility = true;
            vm.CurrentProgressStatusTitle = "Dateien werden gesichert";
            vm.CurrentProgressMax = 100;
            vm.CurrentProgressValue = 64;
            vm.CurrentFileText = @"Dokumente\Projekte\Reiseplanung.pdf";
            vm.ProgressCountText = "1.280 / 2.000 Dateien";
        }
    }
}
#endif
