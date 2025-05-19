using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DMSLib;

namespace DMS_Viewer
{
    public partial class DATCompareDialog : Form
    {
        DMSFile leftFile = null;
        private string leftPath = "";
        DMSFile rightFile = null;
        private string rightPath = "";

        public DATCompareDialog(string initialPath)
        {
            InitializeComponent();
            if (initialPath?.Length > 0)
            {
                try
                {
                    leftFile = DMSReader.Read(initialPath);
                }catch (FormatException fe)
                {
                    MessageBox.Show(this, fe.Message, "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                leftFile.FileName = new FileInfo(initialPath).Name;
                leftPath = initialPath;
            }

            UpdateUI(true);
        }

        void UpdateUI(bool leftSide)
        {

            if (leftSide && leftFile == null)
            {
                return;
            }

            if (!leftSide && rightFile == null)
            {
                return;
            }

            ListView list = null;
            DMSFile file = null;
            Button btn = null;
            Label lbl = null;
            if (leftSide)
            {
                list = lstLeft;
                file = leftFile;
                btn = btnViewDataLeft;
                lbl = lblLeft;
            }
            else
            {
                list = lstRight;
                file = rightFile;
                btn = btnViewDataRight;
                lbl = lblRight;
            }

            if (file == null)
            {
                lbl.Text = @"Select a file...";
                return;
            }
            else
            {
                lbl.Text = file.FileName;
            }

            list.Items.Clear();
            Dictionary<string, CombinedTableSet> tableMap = new Dictionary<string, CombinedTableSet>();
            foreach (var table in file.Tables.OrderBy(t => t.Name))
            {
                /* ignore empty tables */
                if (table.Rows.Count == 0)
                {
                    continue;
                }

                if (!tableMap.ContainsKey(table.Name))
                {
                    tableMap[table.Name] = new CombinedTableSet() { Tables = new List<DMSTable>() };
                }

                tableMap[table.Name].Tables.Add(table);
            }

            foreach(var tableSet in tableMap.Values)
            {
                var backgroundColor = Color.White;
                switch (tableSet.CompareResult.Status)
                {
                    case DMSCompareStatus.NEW:
                        backgroundColor = Color.LawnGreen;
                        break;
                    case DMSCompareStatus.UPDATE:
                        backgroundColor = Color.Yellow;
                        break;
                    case DMSCompareStatus.MISSING:
                        backgroundColor = Color.LightCoral;
                        break;
                }

                list.Items.Add(new ListViewItem() { Tag = tableSet, Text = tableSet.Tables[0].Name, BackColor = backgroundColor });
            }


            btn.Enabled = false;
            btnCompareSelected.Enabled = false;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            GetDATFile(true);
            UpdateUI(true);
        }

        private void GetDATFile(bool isLeft)
        {
            openFileDialog1.Filter = @"Data Mover Data Files|*.dat;*.DAT";
            var result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                DMSFile dmsFile = null;

                try
                {
                    dmsFile = DMSReader.Read(openFileDialog1.FileName);
                }catch(FormatException fe)
                {
                    MessageBox.Show(this, fe.Message, "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                /* Set the file name */
                dmsFile.FileName = new FileInfo(openFileDialog1.FileName).Name;

                if (isLeft)
                {
                    leftFile = dmsFile;
                    leftPath = openFileDialog1.FileName;
                }
                else
                {
                    rightFile = dmsFile;
                    rightPath = openFileDialog1.FileName;
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            GetDATFile(false);
            UpdateUI(false);
        }

        private void btnViewDataLeft_Click(object sender, EventArgs e)
        {
            var viewer = new DataViewer(lstLeft.SelectedItems[0].Tag as CombinedTableSet, "");
            viewer.Show(this);
        }

        private void lstLeft_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstLeft.SelectedItems.Count == 0) { return; }
            if (lstLeft.SelectedItems.Count == 1)
            {
                btnViewDataLeft.Enabled = true;
                lblLeftRows.Text = $@"Rows: {(lstLeft.SelectedItems[0].Tag as CombinedTableSet)?.RowCount}";
            } else
            {
                btnViewDataLeft.Enabled = false;
                lblLeftRows.Text = $@"";
            }

            btnCompareSelected.Enabled = true;
        }

        private void lstRight_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstRight.SelectedItems.Count == 1)
            {
                btnViewDataRight.Enabled = true;
                lblRightRows.Text = $@"Rows: {(lstRight.SelectedItems[0].Tag as CombinedTableSet).RowCount}";
            }
        }

        private void btnViewDataRight_Click(object sender, EventArgs e)
        {
            var viewer = new DataViewer(lstRight.SelectedItems[0].Tag as CombinedTableSet, "");
            viewer.Show(this);
        }

        private void btnCompareRight_Click(object sender, EventArgs e)
        {
            CombinedTableSet[] selectedTableSets =
                lstLeft.SelectedItems.Cast<ListViewItem>().Select(i => (CombinedTableSet) i.Tag).ToArray();

            List<CombinedTableSet> rightTableSets = new List<CombinedTableSet>();

            var worker = new CompareWorker();

            foreach (var tableSet in selectedTableSets)
            {
                /* find the corresponding tableset as a tag on item in lstRight */
                var rightTableSet = lstRight.Items.Cast<ListViewItem>().FirstOrDefault(i => (i.Tag as CombinedTableSet).Name == tableSet.Name)?.Tag as CombinedTableSet;
                rightTableSets.Add(rightTableSet);
                worker.AddTableSet(tableSet, rightTableSet);
            }


            progressBar1.Value = 0;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = selectedTableSets.Sum(t => t.RowCount) + (rightTableSets.Where(t => t != null).Sum(t => t.RowCount));

            worker.ProgressChanged += (o, args) =>
            {
                progressBar1.Value += args.ProgressPercentage;
                progressBar1.Update();
            };

            worker.RunWorkerCompleted += (o, args) =>
            {
                UpdateUI(true);
                UpdateUI(false);
                var firstTable = selectedTableSets[0];
                var index = lstLeft.Items.IndexOf(lstLeft.Items.Cast<ListViewItem>().First(i => (i.Tag as CombinedTableSet).Name == firstTable.Name));
                lstLeft.Focus();
                lstLeft.EnsureVisible(index);

                MessageBox.Show(@"Compare has completed!");
            };

            worker.RunWorkerAsync();
        }

        private void lstLeft_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var selectedTableSets = lstLeft.SelectedItems.Cast<ListViewItem>().Select(i => (CombinedTableSet)i.Tag).ToList();
                var selectedTables = selectedTableSets.SelectMany(t => t.Tables).ToList();
                if (selectedTables.Count > 0)
                {
                    ContextMenu m = new ContextMenu();
                    MenuItem generateSQL = new MenuItem("Generate SQL...");
                    generateSQL.Tag = selectedTables;
                    generateSQL.Click += (o, args) =>
                    {
                        var sqlGen = new SQLGeneratorOptions(leftFile, leftPath, selectedTables);
                        sqlGen.ShowDialog(this);
                    };

                    m.MenuItems.Add(generateSQL);

                    if (leftFile.Tables.Any(t =>
                        t.CompareResult.Status == DMSCompareStatus.NEW || t.CompareResult.Status == DMSCompareStatus.UPDATE))
                    {
                        MenuItem saveDiffs = new MenuItem("Save DAT diff...");
                        saveDiffs.Tag = selectedTables;
                        saveDiffs.Click += (o, args) => { SaveDATDiff(leftFile); };
                        m.MenuItems.Add(saveDiffs);
                    }

                    m.Show(lstLeft, new Point(e.X, e.Y));
                }
            }
        }

        private void SaveDATDiff(DMSFile file)
        {
            saveFileDialog1.Filter = @"Data Mover Data Files|*.dat;*.DAT";
            var result = saveFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                DMSWriter.Write(saveFileDialog1.FileName, file, true);
            }
        }

        private void lstRight_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var selectedTableSets =
                    lstRight.SelectedItems.Cast<ListViewItem>().Select(i => (CombinedTableSet) i.Tag).ToList();
                var selectedTables = selectedTableSets.SelectMany(t => t.Tables).ToList();
                if (selectedTables.Count > 0)
                {
                    ContextMenu m = new ContextMenu();
                    MenuItem generateSQL = new MenuItem("Generate SQL...");
                    generateSQL.Tag = selectedTables;
                    generateSQL.Click += (o, args) =>
                    {
                        var sqlGen = new SQLGeneratorOptions(rightFile, rightPath, selectedTables);
                        sqlGen.ShowDialog(this);
                    };
                    m.MenuItems.Add(generateSQL);

                    if (rightFile.Tables.Any(t =>
                        t.CompareResult.Status == DMSCompareStatus.NEW || t.CompareResult.Status == DMSCompareStatus.UPDATE))
                    {
                        MenuItem saveDiffs = new MenuItem("Save DAT diff...");
                        saveDiffs.Tag = selectedTables;
                        saveDiffs.Click += (o, args) => { SaveDATDiff(rightFile); };
                        m.MenuItems.Add(saveDiffs);
                    }

                    m.Show(lstRight, new Point(e.X, e.Y));
                }
            }
        }
    }

    public class CombinedTableSet
    {
        public List<DMSTable> Tables;
        public int RowCount
        {
            get
            {
                return Tables.Sum(t => t.Rows.Count);
            }
        }
        public string Name
        {
            get
            {
                return Tables[0]?.Name;
            }
        }
        public CompareResult CompareResult
        {
            get
            {
                var result = new CompareResult() { Status = DMSCompareStatus.NONE };

                foreach (var table in Tables)
                {
                    if (table.CompareResult.Status == DMSCompareStatus.UPDATE)
                    {
                        result.Status = DMSCompareStatus.UPDATE;
                        break;
                    }
                    else if (table.CompareResult.Status == DMSCompareStatus.MISSING)
                    {
                        /* If a table has MISSING, it  wont have anything else */
                        result.Status = DMSCompareStatus.MISSING;
                        break;
                    }
                    else if (table.CompareResult.Status == DMSCompareStatus.NEW)
                    {
                        /* If a table has NEW we should try and reflect that, but, if it also has UPDATE
                         * then we should show that instead so continue processing tables */
                        result.Status = DMSCompareStatus.NEW;
                    }
                }

                return result;
            }
        }
    }
    class CompareWorker : BackgroundWorker
    {
        private List<(CombinedTableSet, CombinedTableSet)> selectedSets = new List<(CombinedTableSet, CombinedTableSet)>();

        bool ignoreVersion = Properties.Settings.Default.IgnoreVersion;
        bool ignoreDates = Properties.Settings.Default.IgnoreDates;

        public CompareWorker()
        {
            WorkerReportsProgress = true;
            DoWork += OnDoWork;
        }

        public void AddTableSet(CombinedTableSet left, CombinedTableSet right)
        {
            selectedSets.Add((left, right));
        }

        private void CompareTables(List<DMSTable> leftTables, List<DMSTable> rightTables, bool newIsMissing = false)
        {
            var firstTable = leftTables[0];
            var keyFieldIndexes = firstTable.Metadata.FieldMetadata
                            .Where(m => m.UseEditMask.HasFlag(UseEditFlags.KEY))
                            .Select(t => firstTable.Columns.IndexOf(firstTable.Columns
                                .First(c => c.Name == t.FieldName))).ToArray();

            /* for each row in source table, compare against target tables */

            foreach (var table in leftTables)
            {
                Parallel.ForEach(table.Rows, new ParallelOptions() { MaxDegreeOfParallelism = 1 }, row =>
                {
                    row.CompareResult.Status = DMSCompareStatus.NONE;


                    foreach (var t in rightTables) { 
                        var keyHashes = rightTables.SelectMany(tz => tz.Rows.Select(r => r.KeyHash)).ToList();
                        bool found = keyHashes.Contains(row.KeyHash);
                    }


                    DMSRow targetRow = rightTables.SelectMany(t => t.Rows.Where(r => r.KeyHash == row.KeyHash)).FirstOrDefault();
                    if (targetRow == null)
                    {
                        row.CompareResult.Status = newIsMissing ? DMSCompareStatus.MISSING: DMSCompareStatus.NEW;
                    }
                    else
                    {
                        /* Only compare for same/changed rows if going from left to right. if we're running the other 
                         * direction looking for "missing" rows, we don't need to compare since the left to right will do it */
                        
                        CompareRows(row, targetRow, keyFieldIndexes, newIsMissing);
                        
                    }
                    rowCompareCount++;
                    ReportProgress(1);
                }
                );

                if (table.Rows.Any(r => r.CompareResult.Status == DMSCompareStatus.UPDATE))
                {
                    table.CompareResult.Status = DMSCompareStatus.UPDATE;
                }
                else
                {
                    table.CompareResult.Status = table.Rows.Any(r => r.CompareResult.Status == (newIsMissing ? DMSCompareStatus.MISSING : DMSCompareStatus.NEW))
                        ? (newIsMissing ? DMSCompareStatus.MISSING : DMSCompareStatus.NEW)
                        : DMSCompareStatus.SAME;
                }

                if (table.CompareResult.Status == DMSCompareStatus.SAME && 
                    table.Rows.Any(r => r.CompareResult.Status == DMSCompareStatus.COLUMNS_CHANGED))
                {
                       table.CompareResult.Status = DMSCompareStatus.MISSING;
                }
            }
        }
        private void OnDoWork(object sender, DoWorkEventArgs e)
        {
            Parallel.ForEach(selectedSets, new ParallelOptions() { MaxDegreeOfParallelism = 1}, tableSetPair =>
                {
                    var leftTableSet = tableSetPair.Item1;
                    var rightTableSet = tableSetPair.Item2;

                    if (rightTableSet == null)
                    {
                        foreach (var table in leftTableSet.Tables)
                        {
                            foreach (var row in table.Rows)
                            {
                                row.CompareResult.Status = DMSCompareStatus.NEW;
                            }

                            ReportProgress(table.Rows.Count);
                            rowCompareCount += table.Rows.Count;
                            table.CompareResult.Status = DMSCompareStatus.NEW;
                        }
                    }
                    else
                    {
                        CompareTables(leftTableSet.Tables, rightTableSet.Tables);
                        CompareTables(rightTableSet.Tables, leftTableSet.Tables, true);
                    }

                }
            );
        }
        static int rowCompareCount = 0;
        void CompareRows(DMSRow left, DMSRow right, int[] keyFields, bool newIsMissing = false)
        {
            bool isSame = false;
            if (left.ValueHash == right.ValueHash)
            {
                isSame = true;
                if (!ignoreDates)
                {
                    isSame = left.DateHash == right.DateHash;
                }

                if (!ignoreVersion && isSame)
                {
                    isSame = left.VersionHash == right.VersionHash;
                }
            }

            if (isSame)
            {
                left.CompareResult.Status = DMSCompareStatus.SAME;
            } else
            {
                if (left.KeyHash == right.KeyHash)
                {
                    if (newIsMissing)
                    {
                        left.CompareResult.DeletedIndexes = new List<int>();
                    }
                    else
                    {
                        left.CompareResult.ChangedIndexes = new List<int>();
                        left.CompareResult.AddedIndexes = new List<int>();
                    }

                    /* for each column in the left row, compare against right row */
                    for(var leftColIndex = 0; leftColIndex < left.ColumnCount; leftColIndex++)
                    {
                        var colName = left.OwningTable.Columns[leftColIndex].Name;

                        /* find the index of this column name in the right row */
                        var rightCol = right.OwningTable.GetColumnByName(colName);
                        if (rightCol == null)
                        {
                            if (newIsMissing)
                            {
                                left.CompareResult.DeletedIndexes.Add(leftColIndex);
                            } else
                            {
                                left.CompareResult.AddedIndexes.Add(leftColIndex);
                            }
                            continue;
                        }

                        if (!newIsMissing) // only do this from UI left table to right table (this gets called in both directions)
                        {
                            /* We have the same column in both tables, now compare their values */
                            var rightColIndex = right.OwningTable.Columns.IndexOf(rightCol);

                            var leftValue = left.GetValue(leftColIndex);
                            var rightValue = right.GetValue(rightColIndex);

                            if (!leftValue.Equals(rightValue))
                            {
                                left.CompareResult.ChangedIndexes.Add(leftColIndex);
                                left.CompareResult.Status = DMSCompareStatus.UPDATE;
                            }
                        }
                    }
                    if (left.CompareResult.Status != DMSCompareStatus.UPDATE)
                    {
                        if (left.CompareResult.DeletedIndexes != null && left.CompareResult.DeletedIndexes.Count > 0)
                        {
                            left.CompareResult.Status = DMSCompareStatus.COLUMNS_CHANGED;
                        }

                        if (left.CompareResult.AddedIndexes != null && left.CompareResult.AddedIndexes.Count > 0)
                        {
                            left.CompareResult.Status = DMSCompareStatus.COLUMNS_CHANGED;
                        }
                    }

                } else
                {
                    left.CompareResult.Status = DMSCompareStatus.NEW;
                }
            }
        }
    }
}