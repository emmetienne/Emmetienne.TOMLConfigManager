using Emmetienne.TOMLConfigManager.Controls;
using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Services;
using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Tomlyn;
using XrmToolBox.Extensibility;

namespace Emmetienne.TOMLConfigManager
{
    public partial class TOMLConfigurationManagerControl : MultipleConnectionsPluginControlBase
    {
        private Settings mySettings;
        private Dictionary<Guid, TOMLOperationExecutable> TOMLOperationsExecutable = new Dictionary<Guid, TOMLOperationExecutable>();

        public TOMLConfigurationManagerControl()
        {
            InitializeComponent();
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
            // The ExecuteMethod method handles connecting to an
            // organization if XrmToolBox is not yet connected
            //ExecuteMethod(GetAccounts);
            // open a prompt to load a toml file from disk and display its content in the richtextbox
            var openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "TOML files (*.toml)|*.toml|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var tomlContent = System.IO.File.ReadAllText(openFileDialog.FileName);
                tomlRichTextBox.Text = tomlContent;
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

            try
            {
                var TOMLOperationsDeserialized = Toml.ToModel<TOMLParsed>(tomlContent);

                var tomlOperationList = new List<TOMLOperationRaw>();

                TOMLOperationsExecutable = new Dictionary<Guid, TOMLOperationExecutable>();


                foreach (var singleTOMLOperation in TOMLOperationsDeserialized.operation)
                {
                    if (singleTOMLOperation.Type.Equals("create", StringComparison.OrdinalIgnoreCase))
                    {
                        CreateTOML(singleTOMLOperation);
                    }
                    else
                    {
                        NonCreateTOML(singleTOMLOperation);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error parsing TOML: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateTOML(TOMLOperationRaw singleTOMLOperation)
        {
            var toast = ToTOMLOperationExecutable(singleTOMLOperation, null);

            var cardo = new TOMLCardControl();

            cardo.AddField("Type", toast.Type, FieldType.PlainText);
            cardo.AddField("Table", toast.Table, FieldType.PlainText);

            if (toast.Type.Equals("create", StringComparison.OrdinalIgnoreCase))
            {
                cardo.AddField("Field", string.Join(",", toast.Fields), FieldType.PlainText);


                var valueDisplayString = string.Empty;

                for (int y = 0; y < toast.Fields.Count; y++)
                {
                    valueDisplayString += $"[{toast.Fields[y]}]:{Environment.NewLine}{toast.Values[y]}{Environment.NewLine}";
                }

                cardo.AddField("Value", valueDisplayString, FieldType.Multiline);
            }

            cardo.OperationId = toast.OperationId;
            panelCards.Controls.Add(cardo);
        }

        private void NonCreateTOML(TOMLOperationRaw singleTOMLOperation)
        {
            for (int i = 0; i < singleTOMLOperation.Rows.Count; i++)
            {
                var row = singleTOMLOperation.Rows[i];
                int expected = singleTOMLOperation.MatchOn.Count;

                if (row.Count != expected)
                {
                    throw new Exception(
                       $"The numbers of fields in the row do not match the expected count for the followin TOML:{Environment.NewLine}{Toml.FromModel(singleTOMLOperation)}"
                    );
                }

                if (singleTOMLOperation.Fields?.Count != singleTOMLOperation.Values?.Count)
                {
                    throw new Exception(
                       $"The numbers of fields and values do not match for the following TOML:{Environment.NewLine}{Toml.FromModel(singleTOMLOperation)}"
                    );
                }

                var single = ToTOMLOperationExecutable(singleTOMLOperation, row);

                var card = new TOMLCardControl();

                card.AddField("Type", single.Type, FieldType.PlainText);
                card.AddField("Table", single.Table, FieldType.PlainText);

                //if (!single.Type.Equals("create", StringComparison.OrdinalIgnoreCase))
                //{
                card.AddField("Match On", string.Join(", ", single.MatchOn), FieldType.PlainText);
                card.AddField("Row", single.Row != null ? string.Join(", ", single.Row) : "", FieldType.PlainText);
                //}


                if (single.Type.Equals("upsert", StringComparison.OrdinalIgnoreCase))
                {
                    card.AddField("Ignore Fields", single.IgnoreFields != null ? string.Join(", ", single.IgnoreFields) : "", FieldType.PlainText);
                }

                if (single.Type.Equals("replace", StringComparison.OrdinalIgnoreCase))
                {
                    card.AddField("Field", string.Join(",", single.Fields), FieldType.PlainText);


                    var valueDisplayString = string.Empty;

                    for (int y = 0; y < single.Fields.Count; y++)
                    {
                        valueDisplayString += $"[{single.Fields[y]}]:{Environment.NewLine}{single.Values[y]}{Environment.NewLine}";
                    }

                    card.AddField("Value", valueDisplayString, FieldType.Multiline);
                }

                card.OperationId = single.OperationId;

                panelCards.Controls.Add(card);
                continue;
            }
        }

        private TOMLOperationExecutable ToTOMLOperationExecutable(TOMLOperationRaw singleTOMLOperation, List<string> row)
        {
            var single = new TOMLOperationExecutable();

            single.Type = singleTOMLOperation.Type;
            single.Table = singleTOMLOperation.Table;
            single.MatchOn = singleTOMLOperation.MatchOn;
            single.Row = row;
            single.Fields = singleTOMLOperation.Fields;
            single.Values = singleTOMLOperation.Values;
            single.IgnoreFields = singleTOMLOperation.IgnoreFields;


            TOMLOperationsExecutable[single.OperationId] = single;
            return single;
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

            var configurationService = new TOMLConfigurationService(Service, this.AdditionalConnectionDetails[0].GetCrmServiceClient());

            var TOMLOperationsExecutableSelected = new List<TOMLOperationExecutable>();

            foreach (TOMLCardControl control in panelCards.Controls)
            {
                if (!control.IsSelected)
                    continue;

                TOMLOperationsExecutableSelected.Add(TOMLOperationsExecutable[control.OperationId]);
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