﻿namespace DMS_Viewer
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compareToDATToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateSQLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rebuildScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findAndReplaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.queryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.databaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disconnectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compareToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ignoreVersionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ignoreDatesTimesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hideEmptyTablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.legendToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.legendNew = new System.Windows.Forms.ToolStripMenuItem();
            this.legendChanged = new System.Windows.Forms.ToolStripMenuItem();
            this.LegendMissing = new System.Windows.Forms.ToolStripMenuItem();
            this.legendSame = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.label2 = new System.Windows.Forms.Label();
            this.txtVersion = new System.Windows.Forms.TextBox();
            this.txtDatabase = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtStarted = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.columnList = new System.Windows.Forms.ListView();
            this.colKey = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblRowCount = new System.Windows.Forms.Label();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.label7 = new System.Windows.Forms.Label();
            this.whereClause = new System.Windows.Forms.TextBox();
            this.dataViewer = new System.Windows.Forms.Button();
            this.copyTables = new System.Windows.Forms.Button();
            this.copyWhereClause = new System.Windows.Forms.Button();
            this.btnRecordMeta = new System.Windows.Forms.Button();
            this.btnCompareToDB = new System.Windows.Forms.Button();
            this.tableList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.databaseToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.legendToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(3, 1, 0, 1);
            this.menuStrip1.Size = new System.Drawing.Size(736, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.compareToDATToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 22);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Enabled = false;
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.saveAsToolStripMenuItem.Text = "Save As...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // compareToDATToolStripMenuItem
            // 
            this.compareToDATToolStripMenuItem.Name = "compareToDATToolStripMenuItem";
            this.compareToDATToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.compareToDATToolStripMenuItem.Text = "Compare DAT Files...";
            this.compareToDATToolStripMenuItem.Click += new System.EventHandler(this.CompareToDATToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.generateSQLToolStripMenuItem,
            this.rebuildScriptToolStripMenuItem,
            this.findAndReplaceToolStripMenuItem,
            this.queryToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(46, 22);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // generateSQLToolStripMenuItem
            // 
            this.generateSQLToolStripMenuItem.Enabled = false;
            this.generateSQLToolStripMenuItem.Name = "generateSQLToolStripMenuItem";
            this.generateSQLToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.generateSQLToolStripMenuItem.Text = "Generate SQL";
            this.generateSQLToolStripMenuItem.Click += new System.EventHandler(this.generateSQLToolStripMenuItem_Click);
            // 
            // rebuildScriptToolStripMenuItem
            // 
            this.rebuildScriptToolStripMenuItem.Name = "rebuildScriptToolStripMenuItem";
            this.rebuildScriptToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.rebuildScriptToolStripMenuItem.Text = "Rebuild Script";
            this.rebuildScriptToolStripMenuItem.Click += new System.EventHandler(this.rebuildScriptToolStripMenuItem_Click);
            // 
            // findAndReplaceToolStripMenuItem
            // 
            this.findAndReplaceToolStripMenuItem.Name = "findAndReplaceToolStripMenuItem";
            this.findAndReplaceToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.findAndReplaceToolStripMenuItem.Text = "Find and Replace";
            this.findAndReplaceToolStripMenuItem.Click += new System.EventHandler(this.findAndReplaceToolStripMenuItem_Click);
            // 
            // queryToolStripMenuItem
            // 
            this.queryToolStripMenuItem.Name = "queryToolStripMenuItem";
            this.queryToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.queryToolStripMenuItem.Text = "Query...";
            this.queryToolStripMenuItem.Click += new System.EventHandler(this.queryToolStripMenuItem_Click);
            // 
            // databaseToolStripMenuItem
            // 
            this.databaseToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectToolStripMenuItem,
            this.disconnectToolStripMenuItem});
            this.databaseToolStripMenuItem.Name = "databaseToolStripMenuItem";
            this.databaseToolStripMenuItem.Size = new System.Drawing.Size(67, 22);
            this.databaseToolStripMenuItem.Text = "Database";
            // 
            // connectToolStripMenuItem
            // 
            this.connectToolStripMenuItem.Name = "connectToolStripMenuItem";
            this.connectToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.connectToolStripMenuItem.Text = "Connect...";
            this.connectToolStripMenuItem.Click += new System.EventHandler(this.ConnectToolStripMenuItem_Click);
            // 
            // disconnectToolStripMenuItem
            // 
            this.disconnectToolStripMenuItem.Name = "disconnectToolStripMenuItem";
            this.disconnectToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.disconnectToolStripMenuItem.Text = "Disconnect";
            this.disconnectToolStripMenuItem.Visible = false;
            this.disconnectToolStripMenuItem.Click += new System.EventHandler(this.DisconnectToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.compareToolStripMenuItem,
            this.hideEmptyTablesToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 22);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // compareToolStripMenuItem
            // 
            this.compareToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ignoreVersionToolStripMenuItem,
            this.ignoreDatesTimesToolStripMenuItem});
            this.compareToolStripMenuItem.Name = "compareToolStripMenuItem";
            this.compareToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.compareToolStripMenuItem.Text = "Compares";
            // 
            // ignoreVersionToolStripMenuItem
            // 
            this.ignoreVersionToolStripMenuItem.CheckOnClick = true;
            this.ignoreVersionToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.ignoreVersionToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.ignoreVersionToolStripMenuItem.Name = "ignoreVersionToolStripMenuItem";
            this.ignoreVersionToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.ignoreVersionToolStripMenuItem.Text = "Ignore Version";
            this.ignoreVersionToolStripMenuItem.CheckedChanged += new System.EventHandler(this.IgnoreVersionToolStripMenuItem_CheckedChanged);
            // 
            // ignoreDatesTimesToolStripMenuItem
            // 
            this.ignoreDatesTimesToolStripMenuItem.CheckOnClick = true;
            this.ignoreDatesTimesToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.ignoreDatesTimesToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.ignoreDatesTimesToolStripMenuItem.Name = "ignoreDatesTimesToolStripMenuItem";
            this.ignoreDatesTimesToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.ignoreDatesTimesToolStripMenuItem.Text = "Ignore Dates/Times";
            this.ignoreDatesTimesToolStripMenuItem.CheckedChanged += new System.EventHandler(this.IgnoreDatesTimesToolStripMenuItem_CheckedChanged);
            // 
            // hideEmptyTablesToolStripMenuItem
            // 
            this.hideEmptyTablesToolStripMenuItem.CheckOnClick = true;
            this.hideEmptyTablesToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.hideEmptyTablesToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.hideEmptyTablesToolStripMenuItem.Name = "hideEmptyTablesToolStripMenuItem";
            this.hideEmptyTablesToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.hideEmptyTablesToolStripMenuItem.Text = "Hide Empty Tables";
            this.hideEmptyTablesToolStripMenuItem.TextImageRelation = System.Windows.Forms.TextImageRelation.Overlay;
            this.hideEmptyTablesToolStripMenuItem.CheckedChanged += new System.EventHandler(this.hideEmptyTablesToolStripMenuItem_CheckedChanged);
            this.hideEmptyTablesToolStripMenuItem.Click += new System.EventHandler(this.HideEmptyTablesToolStripMenuItem_Click);
            // 
            // legendToolStripMenuItem
            // 
            this.legendToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.legendNew,
            this.legendChanged,
            this.LegendMissing,
            this.legendSame});
            this.legendToolStripMenuItem.Name = "legendToolStripMenuItem";
            this.legendToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            this.legendToolStripMenuItem.Text = "Compare Legend";
            // 
            // legendNew
            // 
            this.legendNew.BackColor = System.Drawing.Color.LawnGreen;
            this.legendNew.Name = "legendNew";
            this.legendNew.Size = new System.Drawing.Size(180, 22);
            this.legendNew.Text = "New";
            // 
            // legendChanged
            // 
            this.legendChanged.BackColor = System.Drawing.Color.Yellow;
            this.legendChanged.Name = "legendChanged";
            this.legendChanged.Size = new System.Drawing.Size(180, 22);
            this.legendChanged.Text = "Changed";
            // 
            // LegendMissing
            // 
            this.LegendMissing.BackColor = System.Drawing.Color.LightCoral;
            this.LegendMissing.Name = "LegendMissing";
            this.LegendMissing.Size = new System.Drawing.Size(180, 22);
            this.LegendMissing.Text = "Missing";
            // 
            // legendSame
            // 
            this.legendSame.BackColor = System.Drawing.Color.White;
            this.legendSame.Name = "legendSame";
            this.legendSame.Size = new System.Drawing.Size(180, 22);
            this.legendSame.Text = "Same";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 32);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Version:";
            // 
            // txtVersion
            // 
            this.txtVersion.Location = new System.Drawing.Point(52, 32);
            this.txtVersion.Margin = new System.Windows.Forms.Padding(2);
            this.txtVersion.Name = "txtVersion";
            this.txtVersion.ReadOnly = true;
            this.txtVersion.Size = new System.Drawing.Size(104, 20);
            this.txtVersion.TabIndex = 9;
            // 
            // txtDatabase
            // 
            this.txtDatabase.Location = new System.Drawing.Point(222, 32);
            this.txtDatabase.Margin = new System.Windows.Forms.Padding(2);
            this.txtDatabase.Name = "txtDatabase";
            this.txtDatabase.ReadOnly = true;
            this.txtDatabase.Size = new System.Drawing.Size(149, 20);
            this.txtDatabase.TabIndex = 11;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(164, 34);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Database:";
            // 
            // txtStarted
            // 
            this.txtStarted.Location = new System.Drawing.Point(438, 31);
            this.txtStarted.Margin = new System.Windows.Forms.Padding(2);
            this.txtStarted.Name = "txtStarted";
            this.txtStarted.ReadOnly = true;
            this.txtStarted.Size = new System.Drawing.Size(149, 20);
            this.txtStarted.TabIndex = 13;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(392, 32);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(44, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Started:";
            // 
            // columnList
            // 
            this.columnList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colKey,
            this.colName,
            this.colType,
            this.colSize});
            this.columnList.HideSelection = false;
            this.columnList.Location = new System.Drawing.Point(182, 124);
            this.columnList.Margin = new System.Windows.Forms.Padding(2);
            this.columnList.Name = "columnList";
            this.columnList.Size = new System.Drawing.Size(447, 361);
            this.columnList.TabIndex = 14;
            this.columnList.UseCompatibleStateImageBehavior = false;
            this.columnList.View = System.Windows.Forms.View.Details;
            this.columnList.MouseClick += new System.Windows.Forms.MouseEventHandler(this.columnList_MouseClick);
            // 
            // colKey
            // 
            this.colKey.Text = "Key";
            this.colKey.Width = 34;
            // 
            // colName
            // 
            this.colName.Text = "Name";
            this.colName.Width = 130;
            // 
            // colType
            // 
            this.colType.Text = "Type";
            this.colType.Width = 116;
            // 
            // colSize
            // 
            this.colSize.Text = "Size";
            this.colSize.Width = 100;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(179, 109);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "Columns";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(649, 108);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(37, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "Rows:";
            // 
            // lblRowCount
            // 
            this.lblRowCount.AutoSize = true;
            this.lblRowCount.Location = new System.Drawing.Point(690, 108);
            this.lblRowCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblRowCount.Name = "lblRowCount";
            this.lblRowCount.Size = new System.Drawing.Size(13, 13);
            this.lblRowCount.TabIndex = 17;
            this.lblRowCount.Text = "0";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(179, 82);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(42, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "Where:";
            // 
            // whereClause
            // 
            this.whereClause.Location = new System.Drawing.Point(221, 79);
            this.whereClause.Name = "whereClause";
            this.whereClause.Size = new System.Drawing.Size(349, 20);
            this.whereClause.TabIndex = 19;
            // 
            // dataViewer
            // 
            this.dataViewer.Enabled = false;
            this.dataViewer.Location = new System.Drawing.Point(634, 124);
            this.dataViewer.Name = "dataViewer";
            this.dataViewer.Size = new System.Drawing.Size(90, 23);
            this.dataViewer.TabIndex = 20;
            this.dataViewer.Text = "View Data";
            this.dataViewer.UseVisualStyleBackColor = true;
            this.dataViewer.Click += new System.EventHandler(this.dataViewer_Click);
            // 
            // copyTables
            // 
            this.copyTables.Location = new System.Drawing.Point(9, 433);
            this.copyTables.Name = "copyTables";
            this.copyTables.Size = new System.Drawing.Size(155, 23);
            this.copyTables.TabIndex = 21;
            this.copyTables.Text = "Copy Record List";
            this.copyTables.UseVisualStyleBackColor = true;
            this.copyTables.Click += new System.EventHandler(this.copyTables_Click);
            // 
            // copyWhereClause
            // 
            this.copyWhereClause.Location = new System.Drawing.Point(576, 77);
            this.copyWhereClause.Name = "copyWhereClause";
            this.copyWhereClause.Size = new System.Drawing.Size(53, 23);
            this.copyWhereClause.TabIndex = 22;
            this.copyWhereClause.Text = "Copy";
            this.copyWhereClause.UseVisualStyleBackColor = true;
            this.copyWhereClause.Click += new System.EventHandler(this.copyWhereClause_Click);
            // 
            // btnRecordMeta
            // 
            this.btnRecordMeta.Enabled = false;
            this.btnRecordMeta.Location = new System.Drawing.Point(634, 153);
            this.btnRecordMeta.Name = "btnRecordMeta";
            this.btnRecordMeta.Size = new System.Drawing.Size(90, 23);
            this.btnRecordMeta.TabIndex = 23;
            this.btnRecordMeta.Text = "View Metadata";
            this.btnRecordMeta.UseVisualStyleBackColor = true;
            this.btnRecordMeta.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnCompareToDB
            // 
            this.btnCompareToDB.Enabled = false;
            this.btnCompareToDB.Location = new System.Drawing.Point(9, 462);
            this.btnCompareToDB.Name = "btnCompareToDB";
            this.btnCompareToDB.Size = new System.Drawing.Size(155, 23);
            this.btnCompareToDB.TabIndex = 24;
            this.btnCompareToDB.Text = "Compare Selected to DB";
            this.btnCompareToDB.UseVisualStyleBackColor = true;
            this.btnCompareToDB.Click += new System.EventHandler(this.btnCompareToDB_Click);
            // 
            // tableList
            // 
            this.tableList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.tableList.HideSelection = false;
            this.tableList.Location = new System.Drawing.Point(8, 77);
            this.tableList.Margin = new System.Windows.Forms.Padding(2);
            this.tableList.Name = "tableList";
            this.tableList.Size = new System.Drawing.Size(156, 351);
            this.tableList.TabIndex = 25;
            this.tableList.UseCompatibleStateImageBehavior = false;
            this.tableList.View = System.Windows.Forms.View.Details;
            this.tableList.SelectedIndexChanged += new System.EventHandler(this.tableList_SelectedIndexChanged);
            this.tableList.MouseClick += new System.Windows.Forms.MouseEventHandler(this.TableList_MouseClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Record List";
            this.columnHeader1.Width = 143;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2});
            this.statusStrip1.Location = new System.Drawing.Point(0, 487);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(736, 22);
            this.statusStrip1.TabIndex = 26;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(721, 17);
            this.toolStripStatusLabel1.Spring = true;
            this.toolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(0, 17);
            this.toolStripStatusLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(736, 509);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tableList);
            this.Controls.Add(this.btnCompareToDB);
            this.Controls.Add(this.btnRecordMeta);
            this.Controls.Add(this.copyWhereClause);
            this.Controls.Add(this.copyTables);
            this.Controls.Add(this.dataViewer);
            this.Controls.Add(this.whereClause);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.lblRowCount);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.columnList);
            this.Controls.Add(this.txtStarted);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtDatabase);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtVersion);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "DMS Explorer";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtVersion;
        private System.Windows.Forms.TextBox txtDatabase;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtStarted;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListView columnList;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colSize;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblRowCount;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generateSQLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rebuildScriptToolStripMenuItem;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox whereClause;
        private System.Windows.Forms.Button dataViewer;
        private System.Windows.Forms.Button copyTables;
        private System.Windows.Forms.Button copyWhereClause;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.Button btnRecordMeta;
        private System.Windows.Forms.Button btnCompareToDB;
        private System.Windows.Forms.ListView tableList;
        private System.Windows.Forms.ToolStripMenuItem findAndReplaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem databaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disconnectToolStripMenuItem;
        private System.Windows.Forms.ColumnHeader colKey;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripMenuItem compareToDATToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compareToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ignoreVersionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ignoreDatesTimesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hideEmptyTablesToolStripMenuItem;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ToolStripMenuItem queryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem legendToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem legendNew;
        private System.Windows.Forms.ToolStripMenuItem legendChanged;
        private System.Windows.Forms.ToolStripMenuItem LegendMissing;
        private System.Windows.Forms.ToolStripMenuItem legendSame;
    }
}

