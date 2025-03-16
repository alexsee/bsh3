// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Brightbits.BSH.Main;

public partial class frmCreateBackup
{
    public frmCreateBackup()
    {
        InitializeComponent();
    }

    private void frmCreateBackup_Load(object sender, EventArgs e)
    {
        // Quellordner auflisten
        foreach (var Folder in BackupLogic.ConfigurationManager.SourceFolder.Split('|'))
        {
            // Ordner hinzufügen
            clstSources.Items.Add(Folder, true);
        }
    }
}