using Emmetienne.TOMLConfigManager.Eventbus;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Emmetienne.TOMLConfigManager.Components
{
    internal class ExecuteTOMLOperationsButtonComponent
    {
        private readonly Button executeTOMLOperationsButton;

        public ExecuteTOMLOperationsButtonComponent(Component executeTOMLOperationsButton)
        {
            this.executeTOMLOperationsButton = (Button)executeTOMLOperationsButton;

            this.executeTOMLOperationsButton.Click += ExecutionTOMLOperationsButton;
            EventbusSingleton.Instance.disableUiElements += DisableControl;
        }

        private void ExecutionTOMLOperationsButton(object sender, EventArgs e)
        {
            EventbusSingleton.Instance.executeTOMLOperations?.Invoke();
        }

        public void DisableControl(bool disable)
        {
            if (executeTOMLOperationsButton.InvokeRequired)
            {
                executeTOMLOperationsButton.Invoke(new Action(() => DisableControl(disable)));
                return;
            }

            executeTOMLOperationsButton.Enabled = !disable;
        }
    }
}
