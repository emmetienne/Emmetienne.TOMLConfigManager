using Emmetienne.TOMLConfigManager.Eventbus;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Emmetienne.TOMLConfigManager.Components
{
    public class TOMLRichTextBoxComponent
    {
        private readonly RichTextBox TOMLRichTextBox;

        public TOMLRichTextBoxComponent(Component TOMLRichTextBox)
        {
            this.TOMLRichTextBox = (RichTextBox)TOMLRichTextBox;

            EventbusSingleton.Instance.setTOMLText += SetTOMLText;
            EventbusSingleton.Instance.getTOMLText = GetTOMLText;
        }

        private void SetTOMLText(string tomlText)
        {
            if (TOMLRichTextBox.InvokeRequired)
            {
                TOMLRichTextBox.Invoke(new Action(() => TOMLRichTextBox.Text = tomlText));
                return;
            }
        
            TOMLRichTextBox.Text = tomlText;
        }

        private string GetTOMLText()
        {
            if (TOMLRichTextBox.InvokeRequired)
            {
                return (string)TOMLRichTextBox.Invoke(new Func<string>(() => TOMLRichTextBox.Text));
            }
            return TOMLRichTextBox.Text;
        }
    }
}
