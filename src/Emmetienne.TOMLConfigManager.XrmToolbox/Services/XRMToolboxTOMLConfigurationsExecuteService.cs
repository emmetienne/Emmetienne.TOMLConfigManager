using Emmetienne.TOMLConfigManager.Eventbus;
using Emmetienne.TOMLConfigManager.Logger;
using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Stores;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using XrmToolBox.Extensibility;

namespace Emmetienne.TOMLConfigManager.Services
{
    public class XRMToolboxTOMLConfigurationsExecuteService
    {
        private readonly MultipleConnectionsPluginControlBase multipleConnectionsPluginControl;
        private readonly ILogger logger;

        public XRMToolboxTOMLConfigurationsExecuteService(MultipleConnectionsPluginControlBase multipleConnectionsPluginControl, ILogger logger)
        {
            this.multipleConnectionsPluginControl = multipleConnectionsPluginControl;
            this.logger = logger;

            EventbusSingleton.Instance.executeTOMLOperations += PortCongifurations;
        }

        public void PortCongifurations()
        {
            if (multipleConnectionsPluginControl.Service == null)
            {
                MessageBox.Show("Please connect to an organization first");
                return;
            }

            if (multipleConnectionsPluginControl.AdditionalConnectionDetails == null || multipleConnectionsPluginControl.AdditionalConnectionDetails.Count == 0)
            {
                MessageBox.Show("Please select a target environment first");
                return;
            }

            var selectedCards = EventbusSingleton.Instance.getSelectedCards?.Invoke();

            multipleConnectionsPluginControl.WorkAsync(new WorkAsyncInfo
            {
                Message = "Executing TOML Operations",
                Work = (worker, args) =>
                {
                    EventbusSingleton.Instance.disableUiElements?.Invoke(true);

                    var configurationService = new TOMLConfigurationService(multipleConnectionsPluginControl.Service, multipleConnectionsPluginControl.AdditionalConnectionDetails[0].GetCrmServiceClient(), logger);

                    var TOMLOperationsExecutableSelected = new List<TOMLOperationExecutable>();

                    foreach (var control in selectedCards)
                    {
                        if (!control.IsSelected)
                            continue;

                        var selectedTOMLOperationSelected = OperationsStoreSingleton.Instance.GetOperationExecutable(control.OperationId);
                        selectedTOMLOperationSelected.ErrorMessage = string.Empty;

                        TOMLOperationsExecutableSelected.Add(selectedTOMLOperationSelected);
                    }

                    if (TOMLOperationsExecutableSelected.Count == 0)
                    {
                        var errorMessage = "No TOML Operations to execute";
                        logger.LogError(errorMessage);
                        MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        return;
                    }

                    configurationService.PortConfigurations(TOMLOperationsExecutableSelected);

                    args.Result = TOMLOperationsExecutableSelected;
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(args.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        logger.LogError(args.Error.StackTrace);
                        EventbusSingleton.Instance.disableUiElements?.Invoke(false);
                        return;
                    }

                    var TOMLOperationsExecutableSelected = args.Result as List<TOMLOperationExecutable>;

                    foreach (var control in selectedCards)
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

                    EventbusSingleton.Instance.disableUiElements?.Invoke(false);
                }
            });
        }
    }
}
