using Emmetienne.TOMLConfigManager.Eventbus;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Emmetienne.TOMLConfigManager.Components
{
    public class OpenTOMLButtonComponent
    {
        private readonly ToolStripButton openTOMLButton;

        public OpenTOMLButtonComponent(Component openTOMLToolStripButton)
        {
            this.openTOMLButton = (ToolStripButton)openTOMLToolStripButton;

            this.openTOMLButton.Click += InitializeOpenTOMLDialog;
        }

        private void InitializeOpenTOMLDialog(object sender, EventArgs e)
        {
            EventbusSingleton.Instance.openTOMLDialog?.Invoke();
        }
    }
}
