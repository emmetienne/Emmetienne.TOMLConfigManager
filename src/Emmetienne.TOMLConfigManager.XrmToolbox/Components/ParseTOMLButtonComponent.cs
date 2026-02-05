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
        }

        private void ParseTOML(object sender, EventArgs e)
        {
            var tomlText = EventbusSingleton.Instance.getTOMLText?.Invoke();
            EventbusSingleton.Instance.parseTOML?.Invoke(tomlText);
        }
    }
}
