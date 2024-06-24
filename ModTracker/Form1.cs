using System;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace ModTracker
{
    public partial class Form1 : Form
    {
        private readonly string apiKey = "17A92B9729754720E7EF8AA8897F3E94"; // Your Steam API key
        private readonly string listFilePath = "list.txt"; // Path to the list file

        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            btnAddMod.Click += btnAddMod_Click;
            btnRemoveMod.Click += btnRemoveMod_Click;
            buildModList.Click += buildModList_Click;
            btnCopytoClipboard.Click += btnCopytoClipboard_Click;
            btnUp.Click += btnUp_Click;
            btnDown.Click += btnDown_Click;
            dataGridView1.CellValueChanged += dataGridView1_CellValueChanged;
            dataGridView1.CurrentCellDirtyStateChanged += dataGridView1_CurrentCellDirtyStateChanged;
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;
            dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;

            saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
            loadToolStripMenuItem.Click += loadToolStripMenuItem_Click;
            readmeToolStripMenuItem.Click += readmeToolStripMenuItem_Click;
            aboutToolStripMenuItem1.Click += aboutToolStripMenuItem1_Click;
            refreshToolStripMenuItem.Click += refreshToolStripMenuItem_Click; // Add this line

            LoadModList();
            UpdateRichTextBoxFromDataGridView(); // Add this line to update the rich text box after loading the list
            UpdateSelectedModCount(); // Add this line to update the selected mod count label after loading the list
            UpdateTotalModSize(); // Add this line to update the total mod size label after loading the list
        }

        private void LoadModList()
        {
            if (File.Exists(listFilePath))
            {
                string[] lines = File.ReadAllLines(listFilePath);
                dataGridView1.Rows.Clear(); // Clear existing rows before loading
                foreach (string line in lines)
                {
                    string[] parts = line.Split('|');
                    if (parts.Length == 6)
                    {
                        dataGridView1.Rows.Add(Convert.ToBoolean(parts[0]), parts[1], parts[2], parts[3], parts[4], parts[5]);
                    }
                }
            }
        }

        private void SaveModList()
        {
            var lines = new StringBuilder();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow) continue;
                lines.AppendLine(string.Join("|", row.Cells["modEnabled"].Value, row.Cells["modName"].Value, row.Cells["colModSize"].Value, row.Cells["VersionMod"].Value, row.Cells["modID"].Value, row.Cells["modURL"].Value));
            }
            File.WriteAllText(listFilePath, lines.ToString());
        }

        private async void btnAddMod_Click(object sender, EventArgs e)
        {
            string url = txtURL.Text;
            if (!string.IsNullOrEmpty(url))
            {
                string modID = ExtractModID(url);
                (string modName, string modVersion, long modSize) = await FetchModDetailsAsync(modID);

                // Ensure the correct columns are assigned the correct values
                dataGridView1.Rows.Add(false, modName, modSize, modVersion, modID, url);
                txtURL.Clear();
                SaveModList();
                UpdateRichTextBoxFromDataGridView(); // Update the rich text box after adding a mod
                UpdateSelectedModCount(); // Update the selected mod count label after adding a mod
                UpdateTotalModSize(); // Update the total mod size label after adding a mod
            }
        }

        private string ExtractModID(string url)
        {
            var uri = new Uri(url);
            var query = uri.Query;
            var queryDictionary = HttpUtility.ParseQueryString(query);
            return queryDictionary["id"];
        }

        private async Task<(string modName, string modVersion, long modSize)> FetchModDetailsAsync(string modID)
        {
            using (HttpClient client = new HttpClient())
            {
                var requestContent = new StringContent($"key={apiKey}&itemcount=1&publishedfileids[0]={modID}", Encoding.UTF8, "application/x-www-form-urlencoded");
                var response = await client.PostAsync("https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/", requestContent);
                var jsonString = await response.Content.ReadAsStringAsync();

                var jsonDoc = JsonDocument.Parse(jsonString);
                var modDetails = jsonDoc.RootElement.GetProperty("response").GetProperty("publishedfiledetails")[0];

                string modName = modDetails.GetProperty("title").GetString() ?? "Unknown";
                string modVersion = modDetails.GetProperty("time_updated").GetInt32().ToString(); // For example, using the update timestamp as version

                // Handle the file size as a string and parse it to long
                string modSizeString = modDetails.GetProperty("file_size").GetString() ?? "0";
                long modSize = long.Parse(modSizeString); // Mod size in bytes

                return (modName, modVersion, modSize);
            }
        }

        private async void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow) continue;

                string modID = row.Cells["modID"].Value.ToString();
                (string modName, string modVersion, long modSize) = await FetchModDetailsAsync(modID);

                // Update the row with the new data
                row.Cells["modName"].Value = modName;
                row.Cells["VersionMod"].Value = modVersion;
                row.Cells["colModSize"].Value = modSize;
            }

            SaveModList(); // Save the updated mod list
            UpdateTotalModSize(); // Update the total mod size label after refreshing the data
        }

        private void btnRemoveMod_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                if (Convert.ToBoolean(row.Cells["modEnabled"].Value))
                {
                    UpdateRichTextBox(row.Cells["modID"].Value.ToString(), false);
                }
                dataGridView1.Rows.Remove(row);
            }
            SaveModList();
            UpdateRichTextBoxFromDataGridView(); // Update the rich text box after removing a mod
            UpdateSelectedModCount(); // Update the selected mod count label after removing a mod
            UpdateTotalModSize(); // Update the total mod size label after removing a mod
        }

        private void buildModList_Click(object sender, EventArgs e)
        {
            UpdateRichTextBoxFromDataGridView();
        }

        private void btnCopytoClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(richTextBox1.Text);
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["modEnabled"].Index)
            {
                var row = dataGridView1.Rows[e.RowIndex];
                var modID = row.Cells["modID"].Value.ToString();
                var isEnabled = Convert.ToBoolean(row.Cells["modEnabled"].Value);

                UpdateRichTextBox(modID, isEnabled);
                SaveModList();
                UpdateSelectedModCount(); // Update the selected mod count label after changing mod enabled state
                UpdateTotalModSize(); // Update the total mod size label after changing mod enabled state
            }
        }

        private void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView1.IsCurrentCellDirty)
            {
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["modEnabled"].Index)
            {
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void UpdateRichTextBox(string modID, bool add)
        {
            var modIDs = new StringBuilder(richTextBox1.Text);
            if (add)
            {
                if (!modIDs.ToString().Contains(modID))
                {
                    if (modIDs.Length > 0)
                    {
                        modIDs.Append(",");
                    }
                    modIDs.Append(modID);
                }
            }
            else
            {
                var modIDList = modIDs.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                modIDs.Clear();
                foreach (var id in modIDList)
                {
                    if (id != modID)
                    {
                        if (modIDs.Length > 0)
                        {
                            modIDs.Append(",");
                        }
                        modIDs.Append(id);
                    }
                }
            }
            richTextBox1.Text = modIDs.ToString();
        }

        private void UpdateRichTextBoxFromDataGridView()
        {
            var modIDs = new StringBuilder();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (Convert.ToBoolean(row.Cells["modEnabled"].Value))
                {
                    modIDs.Append(row.Cells["modID"].Value.ToString() + ",");
                }
            }

            if (modIDs.Length > 0)
            {
                modIDs.Length--; // Remove the last comma
            }

            richTextBox1.Text = modIDs.ToString();
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            UpdateButtonStates();
            UpdateSelectedModCount(); // Update the selected mod count label when selection changes
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            MoveRow(-1);
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            MoveRow(1);
        }

        private void MoveRow(int direction)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int rowIndex = dataGridView1.SelectedRows[0].Index;
                int newRowIndex = rowIndex + direction;

                // Ensure the new row index is within bounds
                if (newRowIndex < 0 || newRowIndex >= dataGridView1.Rows.Count - 1)
                    return;

                // Commit any edits to the current row
                dataGridView1.EndEdit();

                // Handle the new row if it exists
                if (dataGridView1.Rows[dataGridView1.Rows.Count - 1].IsNewRow)
                {
                    dataGridView1.AllowUserToAddRows = false;
                    dataGridView1.AllowUserToAddRows = true;
                }

                var selectedRow = dataGridView1.Rows[rowIndex];
                dataGridView1.Rows.Remove(selectedRow);
                dataGridView1.Rows.Insert(newRowIndex, selectedRow);
                dataGridView1.ClearSelection();
                selectedRow.Selected = true;

                UpdateButtonStates();
                UpdateRichTextBoxFromDataGridView();
                SaveModList();
                UpdateTotalModSize(); // Update the total mod size label after moving a mod
            }
        }

        private void UpdateButtonStates()
        {
            btnUp.Enabled = dataGridView1.SelectedRows.Count > 0 && dataGridView1.SelectedRows[0].Index > 0;
            btnDown.Enabled = dataGridView1.SelectedRows.Count > 0 && dataGridView1.SelectedRows[0].Index < dataGridView1.Rows.Count - 2;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveModList();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadModList();
            UpdateRichTextBoxFromDataGridView(); // Add this line to update the rich text box after loading the list
            UpdateSelectedModCount(); // Update the selected mod count label after loading the list
            UpdateTotalModSize(); // Add this line to update the total mod size label after loading the list
        }

        private void readmeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Form3 form3 = new Form3();
            form3.Show();
        }

        private void UpdateSelectedModCount()
        {
            int selectedModCount = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (Convert.ToBoolean(row.Cells["modEnabled"].Value))
                {
                    selectedModCount++;
                }
            }
            lblnumMod.Text = $"Selected Mods: {selectedModCount}";
        }

        private void UpdateTotalModSize()
        {
            long totalSize = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (Convert.ToBoolean(row.Cells["modEnabled"].Value))
                {
                    totalSize += Convert.ToInt64(row.Cells["colModSize"].Value);
                }
            }
            lblSpace.Text = $"Total Required Space: {totalSize / (1024 * 1024)} MB"; // Convert bytes to MB
        }
    }
}
