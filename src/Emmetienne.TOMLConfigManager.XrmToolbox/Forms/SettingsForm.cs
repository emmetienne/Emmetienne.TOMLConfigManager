using Emmetienne.TOMLConfigManager.Logger;
using System;
using System.Windows.Forms;
using XrmToolBox.Extensibility;

namespace Emmetienne.TOMLConfigManager.Forms
{
    public partial class SettingsForm : Form
    {
        private readonly Settings settings;
        private readonly ILogger logger;

        public SettingsForm(Settings settings, ILogger logger)
        {
            InitializeComponent();
            this.settings = settings;
            this.logger = logger;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            this.fileBasePathTextBox.Text = settings.FileAndImageBasePath;        
        }

        private void OnClickOkButton(object sender, EventArgs e)
        {
            SaveSettings();
            this.Close();
        }

        private void OnClickCancelButton(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OnClickApplyButton(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            this.settings.FileAndImageBasePath = this.fileBasePathTextBox.Text;

            SettingsManager.Instance.Save(typeof(TOMLConfigurationManagerControl),settings);

            logger.LogDebug($"New base path for files and images: {this.settings.FileAndImageBasePath}");
        }

        private void OnClickSelectFolderButton(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select a folder";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var selectedPath = dialog.SelectedPath;
                    this.fileBasePathTextBox.Text = selectedPath;
                }
            }
        }
    }
}
