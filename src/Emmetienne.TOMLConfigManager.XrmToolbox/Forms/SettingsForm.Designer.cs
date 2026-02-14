namespace Emmetienne.TOMLConfigManager.Forms
{
    partial class SettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.baseSettingsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.folderSelectorTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.selectBasePathFolderLabel = new System.Windows.Forms.Label();
            this.fileBasePathTextBox = new System.Windows.Forms.TextBox();
            this.openSelectFolderDialogButton = new System.Windows.Forms.Button();
            this.buttonsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.baseSettingsTableLayoutPanel.SuspendLayout();
            this.folderSelectorTableLayoutPanel.SuspendLayout();
            this.buttonsTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // baseSettingsTableLayoutPanel
            // 
            this.baseSettingsTableLayoutPanel.ColumnCount = 1;
            this.baseSettingsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.baseSettingsTableLayoutPanel.Controls.Add(this.folderSelectorTableLayoutPanel, 0, 0);
            this.baseSettingsTableLayoutPanel.Controls.Add(this.buttonsTableLayoutPanel, 0, 2);
            this.baseSettingsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.baseSettingsTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.baseSettingsTableLayoutPanel.Name = "baseSettingsTableLayoutPanel";
            this.baseSettingsTableLayoutPanel.RowCount = 3;
            this.baseSettingsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.baseSettingsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.baseSettingsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.baseSettingsTableLayoutPanel.Size = new System.Drawing.Size(624, 81);
            this.baseSettingsTableLayoutPanel.TabIndex = 0;
            // 
            // folderSelectorTableLayoutPanel
            // 
            this.folderSelectorTableLayoutPanel.ColumnCount = 3;
            this.folderSelectorTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.folderSelectorTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.folderSelectorTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.folderSelectorTableLayoutPanel.Controls.Add(this.selectBasePathFolderLabel, 0, 0);
            this.folderSelectorTableLayoutPanel.Controls.Add(this.fileBasePathTextBox, 1, 0);
            this.folderSelectorTableLayoutPanel.Controls.Add(this.openSelectFolderDialogButton, 2, 0);
            this.folderSelectorTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.folderSelectorTableLayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.folderSelectorTableLayoutPanel.Name = "folderSelectorTableLayoutPanel";
            this.folderSelectorTableLayoutPanel.RowCount = 1;
            this.folderSelectorTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.folderSelectorTableLayoutPanel.Size = new System.Drawing.Size(618, 29);
            this.folderSelectorTableLayoutPanel.TabIndex = 0;
            // 
            // selectBasePathFolderLabel
            // 
            this.selectBasePathFolderLabel.AutoSize = true;
            this.selectBasePathFolderLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectBasePathFolderLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.selectBasePathFolderLabel.Location = new System.Drawing.Point(3, 0);
            this.selectBasePathFolderLabel.Name = "selectBasePathFolderLabel";
            this.selectBasePathFolderLabel.Size = new System.Drawing.Size(144, 29);
            this.selectBasePathFolderLabel.TabIndex = 0;
            this.selectBasePathFolderLabel.Text = "Files/Images Base Path:";
            this.selectBasePathFolderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // fileBasePathTextBox
            // 
            this.fileBasePathTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.fileBasePathTextBox.Location = new System.Drawing.Point(153, 6);
            this.fileBasePathTextBox.Name = "fileBasePathTextBox";
            this.fileBasePathTextBox.Size = new System.Drawing.Size(422, 20);
            this.fileBasePathTextBox.TabIndex = 1;
            // 
            // openSelectFolderDialogButton
            // 
            this.openSelectFolderDialogButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.openSelectFolderDialogButton.Location = new System.Drawing.Point(581, 3);
            this.openSelectFolderDialogButton.Name = "openSelectFolderDialogButton";
            this.openSelectFolderDialogButton.Size = new System.Drawing.Size(34, 23);
            this.openSelectFolderDialogButton.TabIndex = 2;
            this.openSelectFolderDialogButton.Text = "📂";
            this.openSelectFolderDialogButton.UseVisualStyleBackColor = true;
            this.openSelectFolderDialogButton.Click += new System.EventHandler(this.OnClickSelectFolderButton);
            // 
            // buttonsTableLayoutPanel
            // 
            this.buttonsTableLayoutPanel.ColumnCount = 4;
            this.buttonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.buttonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.buttonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.buttonsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.buttonsTableLayoutPanel.Controls.Add(this.okButton, 1, 0);
            this.buttonsTableLayoutPanel.Controls.Add(this.cancelButton, 2, 0);
            this.buttonsTableLayoutPanel.Controls.Add(this.applyButton, 3, 0);
            this.buttonsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonsTableLayoutPanel.Location = new System.Drawing.Point(3, 49);
            this.buttonsTableLayoutPanel.Name = "buttonsTableLayoutPanel";
            this.buttonsTableLayoutPanel.RowCount = 1;
            this.buttonsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.buttonsTableLayoutPanel.Size = new System.Drawing.Size(618, 29);
            this.buttonsTableLayoutPanel.TabIndex = 1;
            // 
            // okButton
            // 
            this.okButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.okButton.Location = new System.Drawing.Point(261, 3);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(114, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OnClickOkButton);
            // 
            // cancelButton
            // 
            this.cancelButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cancelButton.Location = new System.Drawing.Point(381, 3);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(114, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.OnClickCancelButton);
            // 
            // applyButton
            // 
            this.applyButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.applyButton.Location = new System.Drawing.Point(501, 3);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(114, 23);
            this.applyButton.TabIndex = 2;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.OnClickApplyButton);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 81);
            this.Controls.Add(this.baseSettingsTableLayoutPanel);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(640, 120);
            this.MinimumSize = new System.Drawing.Size(640, 120);
            this.Name = "SettingsForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Settings";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.OnLoad);
            this.baseSettingsTableLayoutPanel.ResumeLayout(false);
            this.folderSelectorTableLayoutPanel.ResumeLayout(false);
            this.folderSelectorTableLayoutPanel.PerformLayout();
            this.buttonsTableLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel baseSettingsTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel folderSelectorTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel buttonsTableLayoutPanel;
        private System.Windows.Forms.Label selectBasePathFolderLabel;
        private System.Windows.Forms.TextBox fileBasePathTextBox;
        private System.Windows.Forms.Button openSelectFolderDialogButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button applyButton;
    }
}