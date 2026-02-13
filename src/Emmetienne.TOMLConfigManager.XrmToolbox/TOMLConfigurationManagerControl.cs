using Emmetienne.TOMLConfigManager.Components;
using Emmetienne.TOMLConfigManager.Eventbus;
using Emmetienne.TOMLConfigManager.Logger;
using Emmetienne.TOMLConfigManager.Services;
using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Specialized;
using System.Windows.Forms;
using XrmToolBox.Extensibility;

namespace Emmetienne.TOMLConfigManager
{
    public partial class TOMLConfigurationManagerControl : MultipleConnectionsPluginControlBase
    {
        private Settings mySettings;

        private readonly XrmToolboxTOMLLogger logger;

        private readonly PanelCardComponent panelCardComponent;
        private readonly XrmToolboxLoggingComponent loggingComponent;
        private readonly TOMLRichTextBoxComponent tomlRichTextBoxComponent;
        private readonly OpenTOMLButtonComponent openTOMLButtonComponent;
        private readonly ParseTOMLButtonComponent parseTOMLButtonComponent;
        private readonly ExecuteTOMLOperationsButtonComponent executeTOMLOperationsButtonComponent;
        private readonly ToolstripComponent toolstripComponent;


        private readonly XRMToolboxTOMLParsingService xrmToolboxTOMLParsingService;
        private readonly XRMToolboxTOMLConfigurationsExecuteService xrmToolboxTOMLConfigurationsPortingService;
        private readonly OpenTOMLService openTOMLService;


        public TOMLConfigurationManagerControl()
        {
            InitializeComponent();

            logger = new XrmToolboxTOMLLogger();

            loggingComponent = new XrmToolboxLoggingComponent(this.logDataGridView);
            panelCardComponent = new PanelCardComponent(this.panelCards);
            tomlRichTextBoxComponent = new TOMLRichTextBoxComponent(this.tomlRichTextBox);
            openTOMLButtonComponent = new OpenTOMLButtonComponent(this.openFileToolStripButton);
            parseTOMLButtonComponent = new ParseTOMLButtonComponent(this.parseButton);
            executeTOMLOperationsButtonComponent = new ExecuteTOMLOperationsButtonComponent(this.executeOperationButton);
            toolstripComponent = new ToolstripComponent(this.toolStripMenu);


            openTOMLService = new OpenTOMLService(logger);
            xrmToolboxTOMLParsingService = new XRMToolboxTOMLParsingService(this, logger);
            xrmToolboxTOMLConfigurationsPortingService = new XRMToolboxTOMLConfigurationsExecuteService(this, logger);
        }

        private void MyPluginControl_Load(object sender, EventArgs e)
        {
            ShowInfoNotification("Visit my GitHub", new Uri("https://github.com/emmetienne"));
        }

        /// <summary>
        /// This event occurs when the plugin is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyPluginControl_OnCloseTool(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// This event occurs when the connection has been updated in XrmToolBox
        /// </summary>
        /// <summary>
        /// This event occurs when the connection has been updated in XrmToolBox
        /// </summary>
        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
        {
            if (actionName.Equals("AdditionalOrganization", StringComparison.OrdinalIgnoreCase) && newService != null)
            {
                base.UpdateConnection(newService, detail, actionName, parameter);
                return;
            }

            if (newService == null || newService == Service)
                return;

            base.UpdateConnection(newService, detail, actionName, parameter);


            logger.LogWarning($"Source environment connection has changed to: {this.ConnectionDetail.WebApplicationUrl}");
        }

        private void OnSecondEnvironmentButtonClick(object sender, EventArgs e)
        {
            if (this.Service == null)
            {
                MessageBox.Show("Please connect to a source environment first.", "Connection Required", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                AddAdditionalOrganization();
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred while connecting to the target environment: {ex.Message}");
                EventbusSingleton.Instance.disableUiElements?.Invoke(false);
            }
        }

        protected override void ConnectionDetailsUpdated(NotifyCollectionChangedEventArgs e)
        {
            EventbusSingleton.Instance.disableUiElements?.Invoke(true);

            if (this.AdditionalConnectionDetails.Count == 0)
            {
                EventbusSingleton.Instance.disableUiElements?.Invoke(false);
                this.secondEnvToolStripButton.Text = "🔌Connect to Target Environment";
                return;
            }

            if (this.AdditionalConnectionDetails != null && this.AdditionalConnectionDetails.Count > 1)
                this.RemoveAdditionalOrganization(this.AdditionalConnectionDetails[0]);

            this.secondEnvToolStripButton.Text = $"🔌 Connected to {this.AdditionalConnectionDetails[0].ConnectionName}";
            logger.LogWarning($"Target environment connection has changed to: {this.AdditionalConnectionDetails[0].WebApplicationUrl}");


            EventbusSingleton.Instance.disableUiElements?.Invoke(false);
        }

        private void configurationToolStripButton_Click(object sender, EventArgs e)
        {
            //using (var frm = new ConfigurationsForm())
            //{
            //    frm.StartPosition = FormStartPosition.CenterParent;
            //    frm.ShowDialog(this);
            //}
        }
    }
}