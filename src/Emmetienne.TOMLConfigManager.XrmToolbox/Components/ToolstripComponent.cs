using Emmetienne.TOMLConfigManager.Eventbus;
using System;
using System.Windows.Forms;

namespace Emmetienne.TOMLConfigManager.Components
{
    public class ToolstripComponent
    {
        private readonly ToolStrip toolStrip;

        public ToolstripComponent(ToolStrip toolStrip)
        {
            this.toolStrip = toolStrip;

            EventbusSingleton.Instance.disableUiElements += DisableControl;
        }

        public void DisableControl(bool disable)
        {
            if (toolStrip.InvokeRequired)
            {
                toolStrip.Invoke(new Action(() => DisableControl(disable)));
                return;
            }

            toolStrip.Enabled = !disable;
        }
    }
}
