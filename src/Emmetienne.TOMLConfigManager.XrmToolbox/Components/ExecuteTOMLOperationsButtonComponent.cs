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
        }

        private void ExecutionTOMLOperationsButton(object sender, EventArgs e)
        {
            EventbusSingleton.Instance.executeTOMLOperations?.Invoke();
        }
    }
}
