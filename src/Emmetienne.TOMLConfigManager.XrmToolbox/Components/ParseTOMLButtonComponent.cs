using Emmetienne.TOMLConfigManager.Eventbus;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Emmetienne.TOMLConfigManager.Components
{
    internal class ParseTOMLButtonComponent
    {
        private readonly Button openTOMLButton;

        public ParseTOMLButtonComponent(Component openTOMLButton)
        {
            this.openTOMLButton = (Button)openTOMLButton;

            this.openTOMLButton.Click += ParseTOML;
            
            EventbusSingleton.Instance.disableUiElements += DisableControl;
        }

        private void ParseTOML(object sender, EventArgs e)
        {
            var tomlText = EventbusSingleton.Instance.getTOMLText?.Invoke();
            EventbusSingleton.Instance.parseTOML?.Invoke(tomlText);
        }

        public void DisableControl(bool disable)
        {
            if (openTOMLButton.InvokeRequired)
            {
                openTOMLButton.Invoke(new Action(() => DisableControl(disable)));
                return;
            }

            openTOMLButton.Enabled = !disable;
        }
    }
}
