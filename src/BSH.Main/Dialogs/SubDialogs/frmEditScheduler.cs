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

using BSH.Main.Properties;
using Humanizer;
using System;
using System.Data;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class frmEditScheduler
    {
        public frmEditScheduler()
        {
            InitializeComponent();
        }

        private async void frmEditScheduler_Load(object sender, EventArgs e)
        {
            // Zeitplaner lesen
            using (var dbClient = BackupLogic.GlobalBackup.DbClientFactory.CreateDbClient())
            {
                using (var reader = await dbClient.ExecuteDataReaderAsync(CommandType.Text, "SELECT * FROM schedule", null))
                {
                    lwTimeSchedule.Items.Clear();
                    while (reader.Read())
                    {
                        try
                        {
                            var newEntry = new ListViewItem();
                            var parsedDate = DateTime.Parse(reader["timDate"].ToString());

                            switch (reader["timType"].ToString())
                            {
                                case "1":
                                    newEntry.Text = Resources.DLG_EDIT_SCHEDULER_INTERVAL_ONCE;
                                    newEntry.SubItems.Add(reader["timDate"].ToString());
                                    break;

                                case "2":
                                    newEntry.Text = Resources.DLG_EDIT_SCHEDULER_INTERVAL_HOURLY;
                                    newEntry.SubItems.Add(Resources.DLG_EDIT_SCHEDULER_INTERVAL_HOURLY_AT.FormatWith(parsedDate.Minute));
                                    break;

                                case "3":
                                    newEntry.Text = Resources.DLG_EDIT_SCHEDULER_INTERVAL_DAILY;
                                    newEntry.SubItems.Add(parsedDate.ToShortTimeString());
                                    break;

                                case "4":
                                    newEntry.Text = Resources.DLG_EDIT_SCHEDULER_INTERVAL_WEEKLY;
                                    newEntry.SubItems.Add(parsedDate.ToString("dddd") + ", " + parsedDate.ToString("HH:mm"));
                                    break;

                                case "5":
                                    newEntry.Text = Resources.DLG_EDIT_SCHEDULER_INTERVAL_MONTHLY;
                                    newEntry.SubItems.Add(Resources.DLG_EDIT_SCHEDULER_INTERVAL_MONTHLY_AT.FormatWith(parsedDate.Day, parsedDate.ToString("HH:mm")));
                                    break;
                            }

                            newEntry.Tag = parsedDate;
                            lwTimeSchedule.Items.Add(newEntry);
                        }
                        catch
                        {
                            // ignore error
                        }
                    }

                    reader.Close();
                }
            }

            // Löschintervalle festlegen
            switch (BackupLogic.GlobalBackup.ConfigurationManager.IntervallDelete ?? "")
            {
                case var @case when @case == "":
                    rdDontDelete.Checked = true;
                    break;

                case "auto":
                    rdDeleteAuto.Checked = true;
                    break;

                default:
                    rdDeleteIntervall.Checked = true;

                    // Intervall auslesen
                    var Intervall = BackupLogic.GlobalBackup.ConfigurationManager.IntervallDelete.Split('|');
                    txtIntervall.Text = Intervall[1];
                    switch (Intervall[0] ?? "")
                    {
                        case "hour":
                            cboIntervall.SelectedIndex = 0;
                            break;

                        case "day":
                            cboIntervall.SelectedIndex = 1;
                            break;

                        case "week":
                            cboIntervall.SelectedIndex = 2;
                            break;
                    }

                    break;
            }

            nudIntervallHourBackups.Value = decimal.Parse(BackupLogic.GlobalBackup.ConfigurationManager.IntervallAutoHourBackups);

            // Vollsicherung
            if (!string.IsNullOrEmpty(BackupLogic.GlobalBackup.ConfigurationManager.ScheduleFullBackup))
            {
                chkFullBackup.Checked = true;
                var Item = BackupLogic.GlobalBackup.ConfigurationManager.ScheduleFullBackup.Split('|');
                if (Item[0] == "day")
                {
                    cboFullBackup.SelectedIndex = 0;
                }

                nudFullBackup.Value = int.Parse(Item[1]);
            }
            else
            {
                chkFullBackup.Checked = false;
            }
        }

        private void lwTimeSchedule_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Löschen aktivieren, wenn Eintrag markiert
            if (lwTimeSchedule.SelectedItems.Count > 0)
            {
                cmdDeleteTime.Enabled = true;
            }
            else
            {
                cmdDeleteTime.Enabled = false;
            }
        }

        private void cmdAddTime_Click(object sender, EventArgs e)
        {
            // Termin hinzufügen
            using (var dlgAddSchedule = new frmAddSchedule())
            {
                if (dlgAddSchedule.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var newEntry = new ListViewItem();
                newEntry.Text = dlgAddSchedule.cbIntervall.Text;

                switch (dlgAddSchedule.cbIntervall.SelectedIndex)
                {
                    case 0:
                        newEntry.SubItems.Add(dlgAddSchedule.dtpStartTime.Value.ToString());
                        break;

                    case 1:
                        newEntry.SubItems.Add(Resources.DLG_EDIT_SCHEDULER_INTERVAL_HOURLY_AT.FormatWith(dlgAddSchedule.dtpStartTime.Value.Minute));
                        break;

                    case 2:
                        newEntry.SubItems.Add(dlgAddSchedule.dtpStartTime.Value.ToString("HH:mm"));
                        break;

                    case 3:
                        newEntry.SubItems.Add(dlgAddSchedule.dtpStartTime.Value.ToString("dddd") + ", " + dlgAddSchedule.dtpStartTime.Value.ToString("HH:mm"));
                        break;

                    case 4:
                        newEntry.SubItems.Add(Resources.DLG_EDIT_SCHEDULER_INTERVAL_MONTHLY_AT.FormatWith(dlgAddSchedule.dtpStartTime.Value.Day, dlgAddSchedule.dtpStartTime.Value.ToString("HH:mm")));
                        break;
                }

                newEntry.Tag = dlgAddSchedule.dtpStartTime.Value;
                newEntry.SubItems[0].Tag = dlgAddSchedule.cbIntervall.SelectedIndex;

                lwTimeSchedule.Items.Add(newEntry);
            }
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            await BackupLogic.GlobalBackup.ExecuteNonQueryAsync("DELETE FROM schedule");

            // Automatische Backups
            foreach (ListViewItem entry in lwTimeSchedule.Items)
            {
                string parsedDate = Convert.ToDateTime(entry.Tag).ToString("dd.MM.yyyy HH:mm:ss");
                switch (entry.SubItems[0].Tag)
                {
                    case 0:
                        await BackupLogic.GlobalBackup.ExecuteNonQueryAsync("INSERT INTO schedule ( timType, timDate) VALUES ( 1, '" + parsedDate + "' )");
                        break;

                    case 1:
                        await BackupLogic.GlobalBackup.ExecuteNonQueryAsync("INSERT INTO schedule ( timType, timDate) VALUES ( 2, '" + parsedDate + "' )");
                        break;

                    case 2:
                        await BackupLogic.GlobalBackup.ExecuteNonQueryAsync("INSERT INTO schedule ( timType, timDate) VALUES ( 3, '" + parsedDate + "' )");
                        break;

                    case 3:
                        await BackupLogic.GlobalBackup.ExecuteNonQueryAsync("INSERT INTO schedule ( timType, timDate) VALUES ( 4, '" + parsedDate + "' )");
                        break;

                    case 4:
                        await BackupLogic.GlobalBackup.ExecuteNonQueryAsync("INSERT INTO schedule ( timType, timDate) VALUES ( 5, '" + parsedDate + "' )");
                        break;
                }
            }

            // Löschintervalle speichern
            if (rdDontDelete.Checked)
            {
                BackupLogic.GlobalBackup.ConfigurationManager.IntervallDelete = "";
            }
            else if (rdDeleteIntervall.Checked)
            {
                if (!string.IsNullOrEmpty(txtIntervall.Text) && !string.IsNullOrEmpty(cboIntervall.Text))
                {
                    // Manuelles Intervall
                    switch (cboIntervall.SelectedIndex)
                    {
                        case 0:
                            // Stündlich
                            BackupLogic.GlobalBackup.ConfigurationManager.IntervallDelete = "hour|" + txtIntervall.Text;
                            break;

                        case 1:
                            // Täglich
                            BackupLogic.GlobalBackup.ConfigurationManager.IntervallDelete = "day|" + txtIntervall.Text;
                            break;

                        case 2:
                            // Wöchentlich
                            BackupLogic.GlobalBackup.ConfigurationManager.IntervallDelete = "week|" + txtIntervall.Text;
                            break;
                    }
                }
            }
            else if (rdDeleteAuto.Checked)
            {
                BackupLogic.GlobalBackup.ConfigurationManager.IntervallDelete = "auto";
                BackupLogic.GlobalBackup.ConfigurationManager.IntervallAutoHourBackups = nudIntervallHourBackups.Value.ToString();
            }

            // Vollsicherung nach Sicherung oder Tag anlegen
            if (chkFullBackup.Checked)
            {
                if (cboFullBackup.SelectedIndex == 0)
                {
                    BackupLogic.GlobalBackup.ConfigurationManager.ScheduleFullBackup = "day|" + nudFullBackup.Value.ToString();
                }
            }
            else
            {
                BackupLogic.GlobalBackup.ConfigurationManager.ScheduleFullBackup = "";
            }

            Close();
        }

        private void cmdDeleteTime_Click(object sender, EventArgs e)
        {
            if (lwTimeSchedule.SelectedItems.Count > 0)
            {
                lwTimeSchedule.Items.Remove(lwTimeSchedule.SelectedItems[0]);
            }
        }

        private void rdDeleteIntervall_CheckedChanged(object sender, EventArgs e)
        {
            plDelete.Enabled = rdDeleteIntervall.Checked;
        }

        private void rdDeleteAuto_CheckedChanged(object sender, EventArgs e)
        {
            nudIntervallHourBackups.Enabled = rdDeleteAuto.Checked;
        }

        private void chkFullBackup_CheckedChanged(object sender, EventArgs e)
        {
            nudFullBackup.Enabled = chkFullBackup.Checked;
            cboFullBackup.Enabled = chkFullBackup.Checked;
            cboFullBackup.SelectedIndex = 0;
        }
    }
}