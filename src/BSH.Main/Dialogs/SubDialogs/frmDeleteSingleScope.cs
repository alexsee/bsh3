// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Models;
using BSH.Main.Properties;

namespace Brightbits.BSH.Main;

/// <summary>
/// Dialog that lets the user choose whether to delete a file/folder from all backups
/// or only from a selected range (last N, last X days, or explicit versions).
/// </summary>
[ExcludeFromCodeCoverage]
public partial class frmDeleteSingleScope
{
    private readonly IReadOnlyList<VersionDetails> versions = Array.Empty<VersionDetails>();

    public frmDeleteSingleScope()
    {
        InitializeComponent();
    }

    public frmDeleteSingleScope(IReadOnlyList<VersionDetails> versions, bool isFile)
        : this()
    {
        this.versions = versions ?? Array.Empty<VersionDetails>();
        ApplyLocalizedTexts(isFile);
        ConfigureCountBoxes();
        PopulateVersions();
    }

    /// <summary>
    /// Returns a scope result. <see cref="DeleteSingleScopeResult.DeleteFromAllVersions"/> means all history.
    /// </summary>
    public DeleteSingleScopeResult ResolveScope()
    {
        return DeleteSingleScope.Resolve(
            GetSelectedMode(),
            versions,
            (int)numLastN.Value,
            (int)numLastDays.Value,
            lstVersions.Items
                .Cast<ListViewItem>()
                .Where(item => item.Checked && item.Tag != null)
                .Select(item => item.Tag.ToString())
                .ToArray());
    }

    private void ApplyLocalizedTexts(bool isFile)
    {
        Text = Resources.DLG_DELETE_SINGLE_SCOPE_TITLE;
        lblIntro.Text = string.Format(
            Resources.DLG_DELETE_SINGLE_SCOPE_INTRO,
            isFile ? Resources.DLG_DELETE_SINGLE_SCOPE_FILE : Resources.DLG_DELETE_SINGLE_SCOPE_FOLDER);
        radioAll.Text = Resources.DLG_DELETE_SINGLE_SCOPE_ALL;
        radioLastN.Text = Resources.DLG_DELETE_SINGLE_SCOPE_LAST_N;
        radioLastDays.Text = Resources.DLG_DELETE_SINGLE_SCOPE_LAST_DAYS;
        radioSelected.Text = Resources.DLG_DELETE_SINGLE_SCOPE_SELECTED;
        ColumnHeader1.Text = Resources.DLG_DELETE_SINGLE_SCOPE_COLUMN_DATE;
        cmdOK.Text = Resources.DLG_DELETE_SINGLE_SCOPE_OK;
        cmdCancel.Text = Resources.DLG_DELETE_SINGLE_SCOPE_CANCEL;
    }

    private void ConfigureCountBoxes()
    {
        var lastNMaximum = Math.Max(1, versions.Count);
        numLastN.Maximum = lastNMaximum;
        numLastN.Value = Math.Min(3, lastNMaximum);
    }

    private void PopulateVersions()
    {
        lstVersions.Items.Clear();
        foreach (var version in versions)
        {
            lstVersions.Items.Add(new ListViewItem(version.CreationDate.ToLocalTime().ToString("g"))
            {
                Tag = version.Id
            });
        }
    }

    private DeleteSingleScopeMode GetSelectedMode()
    {
        if (radioLastN.Checked)
        {
            return DeleteSingleScopeMode.LastN;
        }

        if (radioLastDays.Checked)
        {
            return DeleteSingleScopeMode.LastDays;
        }

        if (radioSelected.Checked)
        {
            return DeleteSingleScopeMode.SelectedVersions;
        }

        return DeleteSingleScopeMode.AllVersions;
    }

    private void radioScope_CheckedChanged(object sender, EventArgs e)
    {
        numLastN.Enabled = radioLastN.Checked;
        numLastDays.Enabled = radioLastDays.Checked;
        lstVersions.Enabled = radioSelected.Checked;
    }

    private void numLastN_Enter(object sender, EventArgs e)
    {
        radioLastN.Checked = true;
    }

    private void numLastDays_Enter(object sender, EventArgs e)
    {
        radioLastDays.Checked = true;
    }

    private void lstVersions_Enter(object sender, EventArgs e)
    {
        radioSelected.Checked = true;
    }

    private void lstVersions_SizeChanged(object sender, EventArgs e)
    {
        ResizeVersionColumn();
    }

    private void frmDeleteSingleScope_Shown(object sender, EventArgs e)
    {
        ResizeVersionColumn();
    }

    private void ResizeVersionColumn()
    {
        if (lstVersions.Columns.Count == 0)
        {
            return;
        }

        var width = lstVersions.ClientSize.Width - 8;
        if (width > 80)
        {
            lstVersions.Columns[0].Width = width;
        }
    }
}
