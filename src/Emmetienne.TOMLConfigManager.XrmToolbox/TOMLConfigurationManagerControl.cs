using Emmetienne.TOMLConfigManager.Components;
using Emmetienne.TOMLConfigManager.Controls;
using Emmetienne.TOMLConfigManager.Logger;
using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Services;
using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using XrmToolBox.Extensibility;

namespace Emmetienne.TOMLConfigManager
{
    public partial class TOMLConfigurationManagerControl : MultipleConnectionsPluginControlBase
    {
        private Settings mySettings;
        private Dictionary<Guid, TOMLOperationExecutable> TOMLOperationsExecutable = new Dictionary<Guid, TOMLOperationExecutable>();
        private readonly XrmToolboxTOMLLogger logger;

        private XrmToolboxLoggingComponent loggingComponent;

        public TOMLConfigurationManagerControl()
        {
            InitializeComponent();

            logger = new XrmToolboxTOMLLogger();

            loggingComponent = new XrmToolboxLoggingComponent(this.logDataGridView);
        }

        private void MyPluginControl_Load(object sender, EventArgs e)
        {
            ShowInfoNotification("This is a notification that can lead to XrmToolBox repository", new Uri("https://github.com/MscrmTools/XrmToolBox"));

            // Loads or creates the settings for the plugin
            if (!SettingsManager.Instance.TryLoad(GetType(), out mySettings))
            {
                mySettings = new Settings();

                LogWarning("Settings not found => a new settings file has been created!");
            }
            else
            {
                LogInfo("Settings found and loaded");
            }
        }

        private void tsbClose_Click(object sender, EventArgs e)
        {
            CloseTool();
        }

        private void openTomlToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog();

                openFileDialog.Filter = "TOML files (*.toml)|*.toml|All files (*.*)|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var tomlContent = System.IO.File.ReadAllText(openFileDialog.FileName);
                    tomlRichTextBox.Text = tomlContent;
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

        private void GetAccounts()
        {
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Getting accounts",
                Work = (worker, args) =>
                {
                    args.Result = Service.RetrieveMultiple(new QueryExpression("account")
                    {
                        TopCount = 50
                    });
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    var result = args.Result as EntityCollection;
                    if (result != null)
                    {
                        MessageBox.Show($"Found {result.Entities.Count} accounts");
                    }
                }
            });
        }

        /// <summary>
        /// This event occurs when the plugin is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyPluginControl_OnCloseTool(object sender, EventArgs e)
        {
            // Before leaving, save the settings
            SettingsManager.Instance.Save(GetType(), mySettings);
        }

        /// <summary>
        /// This event occurs when the connection has been updated in XrmToolBox
        /// </summary>
        /// <summary>
        /// This event occurs when the connection has been updated in XrmToolBox
        /// </summary>
        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
        {
            if (Service == newService)
                return;

            base.UpdateConnection(newService, detail, actionName, parameter);

            if (actionName.Equals("AdditionalOrganization", StringComparison.OrdinalIgnoreCase))
                return;
        }

        private void parseButton_Click(object sender, EventArgs e)
        {
            var tomlContent = tomlRichTextBox.Text;

            if (string.IsNullOrWhiteSpace(tomlContent))
            {
                MessageBox.Show("No TOML operations to parse");
                return;
            }

            this.panelCards.Controls.Clear();
            this.TOMLOperationsExecutable = new Dictionary<Guid, TOMLOperationExecutable>();

            try
            {
                var TOMLParsingService = new TOMLParsingService();
                // da sistemare quando finisco l'implementazione toml
                var TOMLOperationsDeserialized = TOMLParsingService.ParseToTOMLExecutables(tomlContent, null);

                TOMLOperationsExecutable = new Dictionary<Guid, TOMLOperationExecutable>();

                foreach (var operation in TOMLOperationsDeserialized)
                {
                    TOMLOperationsExecutable[operation.OperationId] = operation;

                    var card = new TOMLCardControl();
                    card.OperationId = operation.OperationId;

                    card.AddField("Type", operation.Type, FieldType.PlainText);
                    card.AddField("Table", operation.Table, FieldType.PlainText);

                    if (operation.Type.Equals("create", StringComparison.OrdinalIgnoreCase))
                    {
                        AddFieldsAndValues(card, operation);
                        panelCards.Controls.Add(card);
                        continue;
                    }

                    card.AddField("Match On", string.Join(", ", operation.MatchOn), FieldType.PlainText);
                    card.AddField("Row", operation.Row != null ? string.Join(", ", operation.Row) : "", FieldType.PlainText);

                    if (operation.Type.Equals("upsert", StringComparison.OrdinalIgnoreCase))
                    {
                        card.AddField("Ignore Fields", operation.IgnoreFields != null ? string.Join(", ", operation.IgnoreFields) : "", FieldType.PlainText);
                    }

                    if (operation.Type.Equals("replace", StringComparison.OrdinalIgnoreCase))
                    {
                        AddFieldsAndValues(card, operation);
                    }

                    panelCards.Controls.Add(card);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error parsing TOML: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private TOMLCardControl CreateCardWithCommonFields(TOMLOperationExecutable operation)
        {
            var card = new TOMLCardControl();
            card.AddField("Type", operation.Type, FieldType.PlainText);
            card.AddField("Table", operation.Table, FieldType.PlainText);
            card.OperationId = operation.OperationId;
            return card;
        }

        private void AddFieldsAndValues(TOMLCardControl card, TOMLOperationExecutable operation)
        {
            card.AddField("Field", string.Join(",", operation.Fields), FieldType.PlainText);

            var sb = new System.Text.StringBuilder();
            for (int y = 0; y < operation.Fields.Count; y++)
            {
                sb.AppendLine($"[{operation.Fields[y]}]:");
                sb.AppendLine(operation.Values[y]);
            }
            card.AddField("Value", sb.ToString(), FieldType.Multiline);
        }

        private void executeOperationButton_Click(object sender, EventArgs e)
        {
            if (Service == null)
            {
                MessageBox.Show("Please connect to an organization first");
                return;
            }

            if (this.AdditionalConnectionDetails == null && this.AdditionalConnectionDetails.Count == 0)
            {
                MessageBox.Show("Please select a second environment first");
                return;
            }

            var configurationService = new TOMLConfigurationService(Service, this.AdditionalConnectionDetails[0].GetCrmServiceClient(), logger);

            var TOMLOperationsExecutableSelected = new List<TOMLOperationExecutable>();

            foreach (TOMLCardControl control in panelCards.Controls)
            {
                if (!control.IsSelected)
                    continue;

                var selectedTOMLOperationSelected = TOMLOperationsExecutable[control.OperationId];
                selectedTOMLOperationSelected.ErrorMessage = string.Empty;

                TOMLOperationsExecutableSelected.Add(selectedTOMLOperationSelected);
            }

            configurationService.PortConfiguration(TOMLOperationsExecutableSelected);

            foreach (TOMLCardControl control in panelCards.Controls)
            {
                var controlUniqueId = control.OperationId;

                var tomlOperationExecutableFound = TOMLOperationsExecutableSelected.FirstOrDefault(t => t.OperationId == controlUniqueId);

                if (tomlOperationExecutableFound == null)
                    continue;

                if (tomlOperationExecutableFound.Success)
                    control.SetOk();
                else
                    control.SetKo(tomlOperationExecutableFound.ErrorMessage);
            }
        }

        private void secondEnvToolStripButton_Click(object sender, EventArgs e)
        {
            AddAdditionalOrganization();

            if (this.AdditionalConnectionDetails.Count == 0)
            {
                this.secondEnvToolStripButton.Text = "Second Env: None";
                return;
            }

            if (this.AdditionalConnectionDetails != null && this.AdditionalConnectionDetails.Count > 1)
                this.RemoveAdditionalOrganization(this.AdditionalConnectionDetails[0]);

            this.secondEnvToolStripButton.Text = this.AdditionalConnectionDetails[0].OrganizationFriendlyName;
        }
    }
}