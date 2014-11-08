namespace Lottery777ManagementSystem
{
    partial class frmMain
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
            this.gbSearchCriteria = new System.Windows.Forms.GroupBox();
            this.lbxLog = new System.Windows.Forms.ListBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.tbSqlQuery = new System.Windows.Forms.TextBox();
            this.lblLastHits = new System.Windows.Forms.Label();
            this.nudLastNRaffles = new System.Windows.Forms.NumericUpDown();
            this.lblHitsInLastRaffles = new System.Windows.Forms.Label();
            this.nudNumHits = new System.Windows.Forms.NumericUpDown();
            this.nud7Hits = new System.Windows.Forms.NumericUpDown();
            this.cb7Hits = new System.Windows.Forms.CheckBox();
            this.nud6Hits = new System.Windows.Forms.NumericUpDown();
            this.cb6Hits = new System.Windows.Forms.CheckBox();
            this.nud5Hits = new System.Windows.Forms.NumericUpDown();
            this.cb5Hits = new System.Windows.Forms.CheckBox();
            this.nudGeneralHits = new System.Windows.Forms.NumericUpDown();
            this.cbGeneralHits = new System.Windows.Forms.CheckBox();
            this.msGeneral = new System.Windows.Forms.MenuStrip();
            this.smiFile = new System.Windows.Forms.ToolStripMenuItem();
            this.smiBackup = new System.Windows.Forms.ToolStripMenuItem();
            this.smiUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.smiOptions = new System.Windows.Forms.ToolStripMenuItem();
            this.smiExit = new System.Windows.Forms.ToolStripMenuItem();
            this.dgvSearchResults = new System.Windows.Forms.DataGridView();
            this.gbSearchCriteria.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudLastNRaffles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNumHits)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud7Hits)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud6Hits)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud5Hits)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGeneralHits)).BeginInit();
            this.msGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSearchResults)).BeginInit();
            this.SuspendLayout();
            // 
            // gbSearchCriteria
            // 
            this.gbSearchCriteria.Controls.Add(this.lbxLog);
            this.gbSearchCriteria.Controls.Add(this.btnSearch);
            this.gbSearchCriteria.Controls.Add(this.tbSqlQuery);
            this.gbSearchCriteria.Controls.Add(this.lblLastHits);
            this.gbSearchCriteria.Controls.Add(this.nudLastNRaffles);
            this.gbSearchCriteria.Controls.Add(this.lblHitsInLastRaffles);
            this.gbSearchCriteria.Controls.Add(this.nudNumHits);
            this.gbSearchCriteria.Controls.Add(this.nud7Hits);
            this.gbSearchCriteria.Controls.Add(this.cb7Hits);
            this.gbSearchCriteria.Controls.Add(this.nud6Hits);
            this.gbSearchCriteria.Controls.Add(this.cb6Hits);
            this.gbSearchCriteria.Controls.Add(this.nud5Hits);
            this.gbSearchCriteria.Controls.Add(this.cb5Hits);
            this.gbSearchCriteria.Controls.Add(this.nudGeneralHits);
            this.gbSearchCriteria.Controls.Add(this.cbGeneralHits);
            this.gbSearchCriteria.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbSearchCriteria.Location = new System.Drawing.Point(0, 24);
            this.gbSearchCriteria.Name = "gbSearchCriteria";
            this.gbSearchCriteria.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.gbSearchCriteria.Size = new System.Drawing.Size(1221, 189);
            this.gbSearchCriteria.TabIndex = 0;
            this.gbSearchCriteria.TabStop = false;
            this.gbSearchCriteria.Text = "קריטריוני חיפוש";
            // 
            // lbxLog
            // 
            this.lbxLog.FormattingEnabled = true;
            this.lbxLog.Location = new System.Drawing.Point(12, 17);
            this.lbxLog.Name = "lbxLog";
            this.lbxLog.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lbxLog.Size = new System.Drawing.Size(581, 160);
            this.lbxLog.TabIndex = 12;
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(796, 157);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(127, 20);
            this.btnSearch.TabIndex = 3;
            this.btnSearch.Text = "חפש";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // tbSqlQuery
            // 
            this.tbSqlQuery.Location = new System.Drawing.Point(599, 17);
            this.tbSqlQuery.Multiline = true;
            this.tbSqlQuery.Name = "tbSqlQuery";
            this.tbSqlQuery.Size = new System.Drawing.Size(324, 134);
            this.tbSqlQuery.TabIndex = 4;
            // 
            // lblLastHits
            // 
            this.lblLastHits.AutoSize = true;
            this.lblLastHits.Location = new System.Drawing.Point(952, 112);
            this.lblLastHits.Name = "lblLastHits";
            this.lblLastHits.Size = new System.Drawing.Size(94, 13);
            this.lblLastHits.TabIndex = 11;
            this.lblLastHits.Text = "הגרלות אחרונות.";
            // 
            // nudLastNRaffles
            // 
            this.nudLastNRaffles.Location = new System.Drawing.Point(1052, 110);
            this.nudLastNRaffles.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudLastNRaffles.Name = "nudLastNRaffles";
            this.nudLastNRaffles.Size = new System.Drawing.Size(46, 20);
            this.nudLastNRaffles.TabIndex = 10;
            // 
            // lblHitsInLastRaffles
            // 
            this.lblHitsInLastRaffles.AutoSize = true;
            this.lblHitsInLastRaffles.Location = new System.Drawing.Point(1104, 112);
            this.lblHitsInLastRaffles.Name = "lblHitsInLastRaffles";
            this.lblHitsInLastRaffles.Size = new System.Drawing.Size(57, 13);
            this.lblHitsInLastRaffles.TabIndex = 9;
            this.lblHitsInLastRaffles.Text = "פגיעות ב-";
            // 
            // nudNumHits
            // 
            this.nudNumHits.Location = new System.Drawing.Point(1169, 110);
            this.nudNumHits.Name = "nudNumHits";
            this.nudNumHits.Size = new System.Drawing.Size(46, 20);
            this.nudNumHits.TabIndex = 8;
            // 
            // nud7Hits
            // 
            this.nud7Hits.Location = new System.Drawing.Point(1052, 87);
            this.nud7Hits.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nud7Hits.Name = "nud7Hits";
            this.nud7Hits.Size = new System.Drawing.Size(46, 20);
            this.nud7Hits.TabIndex = 7;
            // 
            // cb7Hits
            // 
            this.cb7Hits.AutoSize = true;
            this.cb7Hits.Location = new System.Drawing.Point(1143, 87);
            this.cb7Hits.Name = "cb7Hits";
            this.cb7Hits.Size = new System.Drawing.Size(72, 17);
            this.cb7Hits.TabIndex = 6;
            this.cb7Hits.Text = "7 פגיעות";
            this.cb7Hits.UseVisualStyleBackColor = true;
            // 
            // nud6Hits
            // 
            this.nud6Hits.Location = new System.Drawing.Point(1052, 64);
            this.nud6Hits.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nud6Hits.Name = "nud6Hits";
            this.nud6Hits.Size = new System.Drawing.Size(46, 20);
            this.nud6Hits.TabIndex = 5;
            // 
            // cb6Hits
            // 
            this.cb6Hits.AutoSize = true;
            this.cb6Hits.Location = new System.Drawing.Point(1143, 63);
            this.cb6Hits.Name = "cb6Hits";
            this.cb6Hits.Size = new System.Drawing.Size(72, 17);
            this.cb6Hits.TabIndex = 4;
            this.cb6Hits.Text = "6 פגיעות";
            this.cb6Hits.UseVisualStyleBackColor = true;
            // 
            // nud5Hits
            // 
            this.nud5Hits.Location = new System.Drawing.Point(1052, 41);
            this.nud5Hits.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nud5Hits.Name = "nud5Hits";
            this.nud5Hits.Size = new System.Drawing.Size(46, 20);
            this.nud5Hits.TabIndex = 3;
            // 
            // cb5Hits
            // 
            this.cb5Hits.AutoSize = true;
            this.cb5Hits.Location = new System.Drawing.Point(1143, 40);
            this.cb5Hits.Name = "cb5Hits";
            this.cb5Hits.Size = new System.Drawing.Size(72, 17);
            this.cb5Hits.TabIndex = 2;
            this.cb5Hits.Text = "5 פגיעות";
            this.cb5Hits.UseVisualStyleBackColor = true;
            // 
            // nudGeneralHits
            // 
            this.nudGeneralHits.Location = new System.Drawing.Point(1052, 18);
            this.nudGeneralHits.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudGeneralHits.Name = "nudGeneralHits";
            this.nudGeneralHits.Size = new System.Drawing.Size(46, 20);
            this.nudGeneralHits.TabIndex = 1;
            // 
            // cbGeneralHits
            // 
            this.cbGeneralHits.AutoSize = true;
            this.cbGeneralHits.Location = new System.Drawing.Point(1104, 17);
            this.cbGeneralHits.Name = "cbGeneralHits";
            this.cbGeneralHits.Size = new System.Drawing.Size(111, 17);
            this.cbGeneralHits.TabIndex = 0;
            this.cbGeneralHits.Text = "מס\' פגיעות כללי";
            this.cbGeneralHits.UseVisualStyleBackColor = true;
            // 
            // msGeneral
            // 
            this.msGeneral.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.smiFile});
            this.msGeneral.Location = new System.Drawing.Point(0, 0);
            this.msGeneral.Name = "msGeneral";
            this.msGeneral.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.msGeneral.Size = new System.Drawing.Size(1221, 24);
            this.msGeneral.TabIndex = 1;
            this.msGeneral.Text = "menuStrip1";
            // 
            // smiFile
            // 
            this.smiFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.smiBackup,
            this.smiUpdate,
            this.smiOptions,
            this.smiExit});
            this.smiFile.Name = "smiFile";
            this.smiFile.Size = new System.Drawing.Size(46, 20);
            this.smiFile.Text = "קובץ";
            // 
            // smiBackup
            // 
            this.smiBackup.Name = "smiBackup";
            this.smiBackup.Size = new System.Drawing.Size(152, 22);
            this.smiBackup.Text = "גבה";
            // 
            // smiUpdate
            // 
            this.smiUpdate.Name = "smiUpdate";
            this.smiUpdate.Size = new System.Drawing.Size(152, 22);
            this.smiUpdate.Text = "עדכן";
            this.smiUpdate.Click += new System.EventHandler(this.smiUpdate_Click);
            // 
            // smiOptions
            // 
            this.smiOptions.Name = "smiOptions";
            this.smiOptions.Size = new System.Drawing.Size(152, 22);
            this.smiOptions.Text = "אפשרויות";
            // 
            // smiExit
            // 
            this.smiExit.Name = "smiExit";
            this.smiExit.Size = new System.Drawing.Size(152, 22);
            this.smiExit.Text = "יציאה";
            this.smiExit.Click += new System.EventHandler(this.smiExit_Click);
            // 
            // dgvSearchResults
            // 
            this.dgvSearchResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSearchResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvSearchResults.Location = new System.Drawing.Point(0, 213);
            this.dgvSearchResults.MultiSelect = false;
            this.dgvSearchResults.Name = "dgvSearchResults";
            this.dgvSearchResults.ReadOnly = true;
            this.dgvSearchResults.Size = new System.Drawing.Size(1221, 630);
            this.dgvSearchResults.TabIndex = 2;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1221, 843);
            this.Controls.Add(this.dgvSearchResults);
            this.Controls.Add(this.gbSearchCriteria);
            this.Controls.Add(this.msGeneral);
            this.MainMenuStrip = this.msGeneral;
            this.Name = "frmMain";
            this.Text = "ניהול תוצאות לוטו 777";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.gbSearchCriteria.ResumeLayout(false);
            this.gbSearchCriteria.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudLastNRaffles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNumHits)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud7Hits)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud6Hits)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud5Hits)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGeneralHits)).EndInit();
            this.msGeneral.ResumeLayout(false);
            this.msGeneral.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSearchResults)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gbSearchCriteria;
        private System.Windows.Forms.Label lblLastHits;
        private System.Windows.Forms.NumericUpDown nudLastNRaffles;
        private System.Windows.Forms.Label lblHitsInLastRaffles;
        private System.Windows.Forms.NumericUpDown nudNumHits;
        private System.Windows.Forms.NumericUpDown nud7Hits;
        private System.Windows.Forms.CheckBox cb7Hits;
        private System.Windows.Forms.NumericUpDown nud6Hits;
        private System.Windows.Forms.CheckBox cb6Hits;
        private System.Windows.Forms.NumericUpDown nud5Hits;
        private System.Windows.Forms.CheckBox cb5Hits;
        private System.Windows.Forms.NumericUpDown nudGeneralHits;
        private System.Windows.Forms.CheckBox cbGeneralHits;
        private System.Windows.Forms.MenuStrip msGeneral;
        private System.Windows.Forms.ToolStripMenuItem smiFile;
        private System.Windows.Forms.ToolStripMenuItem smiBackup;
        private System.Windows.Forms.ToolStripMenuItem smiUpdate;
        private System.Windows.Forms.ToolStripMenuItem smiOptions;
        private System.Windows.Forms.ToolStripMenuItem smiExit;
        private System.Windows.Forms.DataGridView dgvSearchResults;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.TextBox tbSqlQuery;
        private System.Windows.Forms.ListBox lbxLog;
    }
}

