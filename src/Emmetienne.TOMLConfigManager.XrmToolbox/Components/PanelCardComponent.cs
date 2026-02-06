using Emmetienne.TOMLConfigManager.Controls;
using Emmetienne.TOMLConfigManager.Eventbus;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Emmetienne.TOMLConfigManager.Components
{
    public class PanelCardComponent
    {
        private readonly FlowLayoutPanel cardPanel;

        public PanelCardComponent(Component cardPanel)
        {
            this.cardPanel = (FlowLayoutPanel)cardPanel;

            EventbusSingleton.Instance.addCard += AddCard;
            EventbusSingleton.Instance.clearCards += ClearCards;

            EventbusSingleton.Instance.getSelectedCards = GetSelectedCards;
            EventbusSingleton.Instance.disableUiElements += DisableControl;
        }

        public void AddCard(TOMLCardControl card)
        {
            if (cardPanel.InvokeRequired)
            {
                cardPanel.Invoke(new Action(() => AddCard(card)));
                return;
            }

            cardPanel.Controls.Add(card);
        }

        public void ClearCards()
        {
            if (cardPanel.InvokeRequired)
            {
                cardPanel.Invoke(new Action(() => ClearCards()));
                return;
            }

            cardPanel.Controls.Clear();
        }

        public List<TOMLCardControl> GetAllCards()
        {
            var cards = new List<TOMLCardControl>();
            foreach (TOMLCardControl card in cardPanel.Controls)
            {
                cards.Add(card);
            }
            return cards;
        }

        public List<TOMLCardControl> GetSelectedCards()
        {
            var selectedCards = new List<TOMLCardControl>();
            foreach (TOMLCardControl card in cardPanel.Controls)
            {
                if (card.IsSelected)
                {
                    selectedCards.Add(card);
                }
            }
            return selectedCards;
        }

        public void DisableControl(bool disable)
        {
            if (cardPanel.InvokeRequired)
            {
                cardPanel.Invoke(new Action(() => DisableControl(disable)));
                return;
            }

            cardPanel.Enabled = !disable;
        }
    }
}
