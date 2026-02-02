using Emmetienne.TOMLConfigManager.Controls;
using System.Collections.Specialized;

namespace Emmetienne.TOMLConfigManager
{
    partial class TOMLConfigurationManagerControl
    {
        /// <summary> 
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TOMLConfigurationManagerControl));
            this.toolStripMenu = new System.Windows.Forms.ToolStrip();
            this.openFileToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.tssSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.secondEnvToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.mainSplitContainer = new SplitContainerWithHandles();
            this.controlsSplitContainer = new SplitContainerWithHandles();
            this.tomlTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.parseButton = new System.Windows.Forms.Button();
            this.tomlRichTextBox = new System.Windows.Forms.RichTextBox();
            this.operationTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.panelCards = new System.Windows.Forms.FlowLayoutPanel();
            this.executeOperationButton = new System.Windows.Forms.Button();
            this.logGroupBox = new System.Windows.Forms.GroupBox();
            this.logDataGridView = new System.Windows.Forms.DataGridView();
            this.timestamp = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.message = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Severity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.toolStripMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
            this.mainSplitContainer.Panel1.SuspendLayout();
            this.mainSplitContainer.Panel2.SuspendLayout();
            this.mainSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controlsSplitContainer)).BeginInit();
            this.controlsSplitContainer.Panel1.SuspendLayout();
            this.controlsSplitContainer.Panel2.SuspendLayout();
            this.controlsSplitContainer.SuspendLayout();
            this.tomlTableLayoutPanel.SuspendLayout();
            this.operationTableLayout.SuspendLayout();
            this.logGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStripMenu
            // 
            this.toolStripMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openFileToolStripButton,
            this.tssSeparator1,
            this.secondEnvToolStripButton});
            this.toolStripMenu.Location = new System.Drawing.Point(0, 0);
            this.toolStripMenu.Name = "toolStripMenu";
            this.toolStripMenu.Size = new System.Drawing.Size(1035, 25);
            this.toolStripMenu.TabIndex = 4;
            this.toolStripMenu.Text = "toolStrip1";
            // 
            // openFileToolStripButton
            // 
            this.openFileToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.openFileToolStripButton.Name = "openFileToolStripButton";
            this.openFileToolStripButton.Size = new System.Drawing.Size(75, 22);
            this.openFileToolStripButton.Text = "Open TOML";
            this.openFileToolStripButton.Click += new System.EventHandler(this.openTomlToolStripButton_Click);
            // 
            // tssSeparator1
            // 
            this.tssSeparator1.Name = "tssSeparator1";
            this.tssSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // secondEnvToolStripButton
            // 
            this.secondEnvToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.secondEnvToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("secondEnvToolStripButton.Image")));
            this.secondEnvToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.secondEnvToolStripButton.Name = "secondEnvToolStripButton";
            this.secondEnvToolStripButton.Size = new System.Drawing.Size(107, 22);
            this.secondEnvToolStripButton.Text = "Second Env: None";
            this.secondEnvToolStripButton.Click += new System.EventHandler(this.secondEnvToolStripButton_Click);
            // 
            // mainSplitContainer
            // 
            this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainSplitContainer.Location = new System.Drawing.Point(0, 25);
            this.mainSplitContainer.Name = "mainSplitContainer";
            this.mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.mainSplitContainer.TabStop = false;
            // 
            // mainSplitContainer.Panel1
            // 
            this.mainSplitContainer.Panel1.Controls.Add(this.controlsSplitContainer);
            // 
            // mainSplitContainer.Panel2
            // 
            this.mainSplitContainer.Panel2.Controls.Add(this.logGroupBox);
            this.mainSplitContainer.Size = new System.Drawing.Size(1035, 668);
            this.mainSplitContainer.SplitterDistance = 497;
            this.mainSplitContainer.TabIndex = 5;
            // 
            // controlsSplitContainer
            // 
            this.controlsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlsSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.controlsSplitContainer.Name = "controlsSplitContainer";
            this.controlsSplitContainer.TabStop = false;
            // 
            // controlsSplitContainer.Panel1
            // 
            this.controlsSplitContainer.Panel1.Controls.Add(this.tomlTableLayoutPanel);
            // 
            // controlsSplitContainer.Panel2
            // 
            this.controlsSplitContainer.Panel2.Controls.Add(this.operationTableLayout);
            this.controlsSplitContainer.Panel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.controlsSplitContainer.Size = new System.Drawing.Size(1035, 497);
            this.controlsSplitContainer.SplitterDistance = 223;
            this.controlsSplitContainer.TabIndex = 0;
            // 
            // tomlTableLayoutPanel
            // 
            this.tomlTableLayoutPanel.ColumnCount = 1;
            this.tomlTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tomlTableLayoutPanel.Controls.Add(this.parseButton, 0, 1);
            this.tomlTableLayoutPanel.Controls.Add(this.tomlRichTextBox, 0, 0);
            this.tomlTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tomlTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tomlTableLayoutPanel.Name = "tomlTableLayoutPanel";
            this.tomlTableLayoutPanel.RowCount = 2;
            this.tomlTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tomlTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tomlTableLayoutPanel.Size = new System.Drawing.Size(223, 497);
            this.tomlTableLayoutPanel.TabIndex = 0;
            // 
            // parseButton
            // 
            this.parseButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.parseButton.Location = new System.Drawing.Point(3, 460);
            this.parseButton.Name = "parseButton";
            this.parseButton.Size = new System.Drawing.Size(217, 34);
            this.parseButton.TabIndex = 0;
            this.parseButton.Text = "Parse";
            this.parseButton.UseVisualStyleBackColor = true;
            this.parseButton.Click += new System.EventHandler(this.parseButton_Click);
            // 
            // tomlRichTextBox
            // 
            this.tomlRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tomlRichTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tomlRichTextBox.Location = new System.Drawing.Point(3, 3);
            this.tomlRichTextBox.Name = "tomlRichTextBox";
            this.tomlRichTextBox.Size = new System.Drawing.Size(217, 451);
            this.tomlRichTextBox.TabIndex = 0;
            this.tomlRichTextBox.Text = "";
            // 
            // operationTableLayout
            // 
            this.operationTableLayout.ColumnCount = 1;
            this.operationTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.operationTableLayout.Controls.Add(this.panelCards, 0, 0);
            this.operationTableLayout.Controls.Add(this.executeOperationButton, 0, 1);
            this.operationTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.operationTableLayout.Location = new System.Drawing.Point(0, 0);
            this.operationTableLayout.Name = "operationTableLayout";
            this.operationTableLayout.RowCount = 2;
            this.operationTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.operationTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.operationTableLayout.Size = new System.Drawing.Size(808, 497);
            this.operationTableLayout.TabIndex = 0;
            // 
            // panelCards
            // 
            this.panelCards.AutoScroll = true;
            this.panelCards.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelCards.Location = new System.Drawing.Point(3, 3);
            this.panelCards.Name = "panelCards";
            this.panelCards.Size = new System.Drawing.Size(802, 451);
            this.panelCards.TabIndex = 0;
            // 
            // executeOperationButton
            // 
            this.executeOperationButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.executeOperationButton.Location = new System.Drawing.Point(3, 460);
            this.executeOperationButton.Name = "executeOperationButton";
            this.executeOperationButton.Size = new System.Drawing.Size(802, 34);
            this.executeOperationButton.TabIndex = 1;
            this.executeOperationButton.Text = "Execute operations";
            this.executeOperationButton.UseVisualStyleBackColor = true;
            this.executeOperationButton.Click += new System.EventHandler(this.executeOperationButton_Click);
            // 
            // logGroupBox
            // 
            this.logGroupBox.Controls.Add(this.logDataGridView);
            this.logGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logGroupBox.Location = new System.Drawing.Point(0, 0);
            this.logGroupBox.Name = "logGroupBox";
            this.logGroupBox.Size = new System.Drawing.Size(1035, 167);
            this.logGroupBox.TabIndex = 0;
            this.logGroupBox.TabStop = false;
            this.logGroupBox.Text = "Logs";
            // 
            // logDataGridView
            // 
            this.logDataGridView.AllowUserToAddRows = false;
            this.logDataGridView.AllowUserToDeleteRows = false;
            this.logDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.ColumnHeader;
            this.logDataGridView.BackgroundColor = System.Drawing.SystemColors.Window;
            this.logDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.logDataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.logDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.logDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.timestamp,
            this.message,
            this.Severity});
            this.logDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logDataGridView.Location = new System.Drawing.Point(3, 16);
            this.logDataGridView.Name = "logDataGridView";
            this.logDataGridView.ReadOnly = true;
            this.logDataGridView.Size = new System.Drawing.Size(1029, 148);
            this.logDataGridView.TabIndex = 0;
            // 
            // timestamp
            // 
            this.timestamp.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.timestamp.HeaderText = "Timestamp";
            this.timestamp.Name = "timestamp";
            this.timestamp.ReadOnly = true;
            // 
            // message
            // 
            this.message.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.message.HeaderText = "Message";
            this.message.Name = "message";
            this.message.ReadOnly = true;
            // 
            // Severity
            // 
            this.Severity.HeaderText = "Severity";
            this.Severity.Name = "Severity";
            this.Severity.ReadOnly = true;
            this.Severity.Width = 70;
            // 
            // TOMLConfigurationManagerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mainSplitContainer);
            this.Controls.Add(this.toolStripMenu);
            this.Name = "TOMLConfigurationManagerControl";
            this.Size = new System.Drawing.Size(1035, 693);
            this.Load += new System.EventHandler(this.MyPluginControl_Load);
            this.toolStripMenu.ResumeLayout(false);
            this.toolStripMenu.PerformLayout();
            this.mainSplitContainer.Panel1.ResumeLayout(false);
            this.mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
            this.mainSplitContainer.ResumeLayout(false);
            this.controlsSplitContainer.Panel1.ResumeLayout(false);
            this.controlsSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.controlsSplitContainer)).EndInit();
            this.controlsSplitContainer.ResumeLayout(false);
            this.tomlTableLayoutPanel.ResumeLayout(false);
            this.operationTableLayout.ResumeLayout(false);
            this.logGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.logDataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        protected override void ConnectionDetailsUpdated(NotifyCollectionChangedEventArgs e)
        {
            // non mi ricordo cosa fa sto coso
            return;
        }

        #endregion
        private System.Windows.Forms.ToolStrip toolStripMenu;
        private System.Windows.Forms.ToolStripButton openFileToolStripButton;
        private System.Windows.Forms.ToolStripSeparator tssSeparator1;
        private System.Windows.Forms.SplitContainer mainSplitContainer;
        private System.Windows.Forms.SplitContainer controlsSplitContainer;
        private System.Windows.Forms.RichTextBox tomlRichTextBox;
        private System.Windows.Forms.Button parseButton;
        private System.Windows.Forms.TableLayoutPanel tomlTableLayoutPanel;
        private System.Windows.Forms.FlowLayoutPanel panelCards;
        private System.Windows.Forms.TableLayoutPanel operationTableLayout;
        private System.Windows.Forms.Button executeOperationButton;
        private System.Windows.Forms.ToolStripButton secondEnvToolStripButton;
        private System.Windows.Forms.GroupBox logGroupBox;
        private System.Windows.Forms.DataGridView logDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn timestamp;
        private System.Windows.Forms.DataGridViewTextBoxColumn message;
        private System.Windows.Forms.DataGridViewTextBoxColumn Severity;
    }
}
