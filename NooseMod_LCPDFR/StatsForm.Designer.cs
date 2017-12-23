namespace NooseMod_LCPDFR
{
    partial class StatsForm
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
            this.components = new System.ComponentModel.Container();
            this.statsDataSet = new NooseMod_LCPDFR.StatsDataSet();
            this.missionStatsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.missionStatsTableAdapter = new NooseMod_LCPDFR.StatsDataSetTableAdapters.MissionStatsTableAdapter();
            this.tableAdapterManager = new NooseMod_LCPDFR.StatsDataSetTableAdapters.TableAdapterManager();
            this.missionStatsDataGridView = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.overallStatsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.overallStatsTableAdapter = new NooseMod_LCPDFR.StatsDataSetTableAdapters.OverallStatsTableAdapter();
            this.overallStatsDataGridView = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn11 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn12 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn13 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn14 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn15 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.statsDataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.missionStatsBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.missionStatsDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.overallStatsBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.overallStatsDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // statsDataSet
            // 
            this.statsDataSet.DataSetName = "StatsDataSet";
            this.statsDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // missionStatsBindingSource
            // 
            this.missionStatsBindingSource.DataMember = "MissionStats";
            this.missionStatsBindingSource.DataSource = this.statsDataSet;
            // 
            // missionStatsTableAdapter
            // 
            this.missionStatsTableAdapter.ClearBeforeFill = true;
            // 
            // tableAdapterManager
            // 
            this.tableAdapterManager.BackupDataSetBeforeUpdate = false;
            this.tableAdapterManager.MissionStatsTableAdapter = this.missionStatsTableAdapter;
            this.tableAdapterManager.OverallStatsTableAdapter = null;
            this.tableAdapterManager.UpdateOrder = NooseMod_LCPDFR.StatsDataSetTableAdapters.TableAdapterManager.UpdateOrderOption.InsertUpdateDelete;
            // 
            // missionStatsDataGridView
            // 
            this.missionStatsDataGridView.AutoGenerateColumns = false;
            this.missionStatsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.missionStatsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4,
            this.dataGridViewTextBoxColumn5,
            this.dataGridViewTextBoxColumn6,
            this.dataGridViewTextBoxColumn7,
            this.dataGridViewTextBoxColumn8});
            this.missionStatsDataGridView.DataSource = this.missionStatsBindingSource;
            this.missionStatsDataGridView.Location = new System.Drawing.Point(12, 32);
            this.missionStatsDataGridView.Name = "missionStatsDataGridView";
            this.missionStatsDataGridView.RowTemplate.Height = 28;
            this.missionStatsDataGridView.Size = new System.Drawing.Size(849, 220);
            this.missionStatsDataGridView.TabIndex = 1;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "Mission Number";
            this.dataGridViewTextBoxColumn1.HeaderText = "Mission Number";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.DataPropertyName = "Suspects Killed";
            this.dataGridViewTextBoxColumn2.HeaderText = "Suspects Killed";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.DataPropertyName = "Suspects Arrested";
            this.dataGridViewTextBoxColumn3.HeaderText = "Suspects Arrested";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.DataPropertyName = "Hostages Killed";
            this.dataGridViewTextBoxColumn4.HeaderText = "Hostages Killed";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.DataPropertyName = "Hostages Rescued";
            this.dataGridViewTextBoxColumn5.HeaderText = "Hostages Rescued";
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            // 
            // dataGridViewTextBoxColumn6
            // 
            this.dataGridViewTextBoxColumn6.DataPropertyName = "Officer Casualties";
            this.dataGridViewTextBoxColumn6.HeaderText = "Officer Casualties";
            this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            // 
            // dataGridViewTextBoxColumn7
            // 
            this.dataGridViewTextBoxColumn7.DataPropertyName = "Squad Casualties";
            this.dataGridViewTextBoxColumn7.HeaderText = "Squad Casualties";
            this.dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
            // 
            // dataGridViewTextBoxColumn8
            // 
            this.dataGridViewTextBoxColumn8.DataPropertyName = "Income";
            this.dataGridViewTextBoxColumn8.HeaderText = "Income";
            this.dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
            // 
            // overallStatsBindingSource
            // 
            this.overallStatsBindingSource.DataMember = "OverallStats";
            this.overallStatsBindingSource.DataSource = this.statsDataSet;
            // 
            // overallStatsTableAdapter
            // 
            this.overallStatsTableAdapter.ClearBeforeFill = true;
            // 
            // overallStatsDataGridView
            // 
            this.overallStatsDataGridView.AutoGenerateColumns = false;
            this.overallStatsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.overallStatsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn9,
            this.dataGridViewTextBoxColumn10,
            this.dataGridViewTextBoxColumn11,
            this.dataGridViewTextBoxColumn12,
            this.dataGridViewTextBoxColumn13,
            this.dataGridViewTextBoxColumn14,
            this.dataGridViewTextBoxColumn15});
            this.overallStatsDataGridView.DataSource = this.overallStatsBindingSource;
            this.overallStatsDataGridView.Location = new System.Drawing.Point(12, 288);
            this.overallStatsDataGridView.Name = "overallStatsDataGridView";
            this.overallStatsDataGridView.RowTemplate.Height = 28;
            this.overallStatsDataGridView.Size = new System.Drawing.Size(849, 220);
            this.overallStatsDataGridView.TabIndex = 2;
            // 
            // dataGridViewTextBoxColumn9
            // 
            this.dataGridViewTextBoxColumn9.DataPropertyName = "Session";
            this.dataGridViewTextBoxColumn9.HeaderText = "Session";
            this.dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
            // 
            // dataGridViewTextBoxColumn10
            // 
            this.dataGridViewTextBoxColumn10.DataPropertyName = "Suspects Killed";
            this.dataGridViewTextBoxColumn10.HeaderText = "Suspects Killed";
            this.dataGridViewTextBoxColumn10.Name = "dataGridViewTextBoxColumn10";
            // 
            // dataGridViewTextBoxColumn11
            // 
            this.dataGridViewTextBoxColumn11.DataPropertyName = "Suspects Arrested";
            this.dataGridViewTextBoxColumn11.HeaderText = "Suspects Arrested";
            this.dataGridViewTextBoxColumn11.Name = "dataGridViewTextBoxColumn11";
            // 
            // dataGridViewTextBoxColumn12
            // 
            this.dataGridViewTextBoxColumn12.DataPropertyName = "Hostages Killed";
            this.dataGridViewTextBoxColumn12.HeaderText = "Hostages Killed";
            this.dataGridViewTextBoxColumn12.Name = "dataGridViewTextBoxColumn12";
            // 
            // dataGridViewTextBoxColumn13
            // 
            this.dataGridViewTextBoxColumn13.DataPropertyName = "Hostages Rescued";
            this.dataGridViewTextBoxColumn13.HeaderText = "Hostages Rescued";
            this.dataGridViewTextBoxColumn13.Name = "dataGridViewTextBoxColumn13";
            // 
            // dataGridViewTextBoxColumn14
            // 
            this.dataGridViewTextBoxColumn14.DataPropertyName = "Officer Casualties";
            this.dataGridViewTextBoxColumn14.HeaderText = "Officer Casualties";
            this.dataGridViewTextBoxColumn14.Name = "dataGridViewTextBoxColumn14";
            // 
            // dataGridViewTextBoxColumn15
            // 
            this.dataGridViewTextBoxColumn15.DataPropertyName = "Squad Casualties";
            this.dataGridViewTextBoxColumn15.HeaderText = "Squad Casualties";
            this.dataGridViewTextBoxColumn15.Name = "dataGridViewTextBoxColumn15";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "Mission Stats";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 265);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 20);
            this.label2.TabIndex = 4;
            this.label2.Text = "Overall Stats";
            // 
            // StatsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(873, 525);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.overallStatsDataGridView);
            this.Controls.Add(this.missionStatsDataGridView);
            this.Name = "StatsForm";
            this.Text = "StatsForm";
            this.Load += new System.EventHandler(this.StatsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.statsDataSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.missionStatsBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.missionStatsDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.overallStatsBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.overallStatsDataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private StatsDataSet statsDataSet;
        private System.Windows.Forms.BindingSource missionStatsBindingSource;
        private StatsDataSetTableAdapters.MissionStatsTableAdapter missionStatsTableAdapter;
        private StatsDataSetTableAdapters.TableAdapterManager tableAdapterManager;
        private System.Windows.Forms.DataGridView missionStatsDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
        private System.Windows.Forms.BindingSource overallStatsBindingSource;
        private StatsDataSetTableAdapters.OverallStatsTableAdapter overallStatsTableAdapter;
        private System.Windows.Forms.DataGridView overallStatsDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn10;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn11;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn12;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn13;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn14;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn15;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}