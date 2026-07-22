// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
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
public sealed class frmDeleteSingleScope : Form
{
    internal RadioButton radioAll;
    internal RadioButton radioLastN;
    internal RadioButton radioLastDays;
    internal RadioButton radioSelected;
    internal NumericUpDown numLastN;
    internal NumericUpDown numLastDays;
    internal ListView lstVersions;

    private readonly IReadOnlyList<VersionDetails> versions;

    public frmDeleteSingleScope(IReadOnlyList<VersionDetails> versions, bool isFile)
    {
        this.versions = versions ?? Array.Empty<VersionDetails>();

        Text = Resources.DLG_DELETE_SINGLE_SCOPE_TITLE;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(520, 460);
        Font = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);

        var intro = new Label
        {
            AutoSize = false,
            Location = new Point(18, 16),
            Size = new Size(480, 40),
            Text = string.Format(
                Resources.DLG_DELETE_SINGLE_SCOPE_INTRO,
                isFile ? Resources.DLG_DELETE_SINGLE_SCOPE_FILE : Resources.DLG_DELETE_SINGLE_SCOPE_FOLDER)
        };

        radioAll = CreateRadio(Resources.DLG_DELETE_SINGLE_SCOPE_ALL, new Point(22, 64), true);
        radioLastN = CreateRadio(Resources.DLG_DELETE_SINGLE_SCOPE_LAST_N, new Point(22, 96), false);
        numLastN = new NumericUpDown
        {
            Location = new Point(42, 124),
            Size = new Size(120, 25),
            Minimum = 1,
            Maximum = Math.Max(1, this.versions.Count),
            Value = Math.Min(3, Math.Max(1, this.versions.Count)),
            Enabled = false
        };

        radioLastDays = CreateRadio(Resources.DLG_DELETE_SINGLE_SCOPE_LAST_DAYS, new Point(22, 160), false);
        numLastDays = new NumericUpDown
        {
            Location = new Point(42, 188),
            Size = new Size(120, 25),
            Minimum = 1,
            Maximum = 3650,
            Value = 30,
            Enabled = false
        };

        radioSelected = CreateRadio(Resources.DLG_DELETE_SINGLE_SCOPE_SELECTED, new Point(22, 224), false);
        lstVersions = new ListView
        {
            Location = new Point(42, 252),
            Size = new Size(440, 140),
            CheckBoxes = true,
            View = View.Details,
            FullRowSelect = true,
            Enabled = false
        };
        lstVersions.Columns.Add(Resources.DLG_DELETE_SINGLE_SCOPE_COLUMN_DATE, 300);

        foreach (var version in this.versions)
        {
            lstVersions.Items.Add(new ListViewItem(version.CreationDate.ToLocalTime().ToString("g"))
            {
                Tag = version.Id
            });
        }

        radioAll.CheckedChanged += (_, _) => UpdateEnabledState();
        radioLastN.CheckedChanged += (_, _) => UpdateEnabledState();
        radioLastDays.CheckedChanged += (_, _) => UpdateEnabledState();
        radioSelected.CheckedChanged += (_, _) => UpdateEnabledState();

        var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 48 };
        var cmdOk = new Button
        {
            Text = Resources.DLG_DELETE_SINGLE_SCOPE_OK,
            DialogResult = DialogResult.OK,
            Size = new Size(93, 26),
            Location = new Point(310, 10)
        };
        var cmdCancel = new Button
        {
            Text = Resources.DLG_DELETE_SINGLE_SCOPE_CANCEL,
            DialogResult = DialogResult.Cancel,
            Size = new Size(93, 26),
            Location = new Point(410, 10)
        };
        buttonPanel.Controls.Add(cmdOk);
        buttonPanel.Controls.Add(cmdCancel);

        Controls.Add(intro);
        Controls.Add(radioAll);
        Controls.Add(radioLastN);
        Controls.Add(numLastN);
        Controls.Add(radioLastDays);
        Controls.Add(numLastDays);
        Controls.Add(radioSelected);
        Controls.Add(lstVersions);
        Controls.Add(buttonPanel);

        AcceptButton = cmdOk;
        CancelButton = cmdCancel;
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

    private static RadioButton CreateRadio(string text, Point location, bool isChecked)
    {
        return new RadioButton
        {
            AutoSize = true,
            Location = location,
            Text = text,
            Checked = isChecked
        };
    }

    private void UpdateEnabledState()
    {
        numLastN.Enabled = radioLastN.Checked;
        numLastDays.Enabled = radioLastDays.Checked;
        lstVersions.Enabled = radioSelected.Checked;
    }
}
