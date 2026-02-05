using Emmetienne.TOMLConfigManager.Eventbus;
using Emmetienne.TOMLConfigManager.Logger;
using System;
using System.Windows.Forms;

namespace Emmetienne.TOMLConfigManager.Services
{
    public class OpenTOMLService
    {
        private readonly ILogger logger;

        public OpenTOMLService(ILogger logger)
        {
            this.logger = logger;
            EventbusSingleton.Instance.openTOMLDialog += OpenTOML;
        }

        public void OpenTOML()
        {
            try
            {
                var openFileDialog = new OpenFileDialog();

                openFileDialog.Filter = "TOML files (*.toml)|*.toml|All files (*.*)|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var tomlContent = System.IO.File.ReadAllText(openFileDialog.FileName);
                    EventbusSingleton.Instance.setTOMLText?.Invoke(tomlContent);
                }
                else
                {
                    logger.LogInfo("TOML file loading canceled by user.");
                    return;
                }

                logger.LogInfo($"TOML file '{openFileDialog.FileName}' loaded.");

            }
            catch (Exception ex)
            {
                logger.LogError($"Error loading TOML file: {ex.Message}");
            }
        }
    }
}