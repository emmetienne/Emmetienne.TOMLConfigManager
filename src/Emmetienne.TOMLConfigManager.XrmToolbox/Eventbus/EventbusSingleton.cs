using Emmetienne.TOMLConfigManager.Controls;
using Emmetienne.TOMLConfigManager.Logger;
using Emmetienne.TOMLConfigManager.Models;
using System;
using System.Collections.Generic;

namespace Emmetienne.TOMLConfigManager.Eventbus
{
    public class EventbusSingleton : Singleton<EventbusSingleton>
    {
        public Action<bool> disableUiElements;

        public Action<TOMLCardControl> addCard;
        public Action clearCards;
        public Func<List<TOMLCardControl>> getSelectedCards;

        public Action initializeOperationStore;
        public Action<Guid,TOMLOperationExecutable> addToOperationStore;
        public Func<Guid, TOMLOperationExecutable> getOperationFromStore;
        public Func<List<TOMLOperationExecutable>> getAllOperationsFromStore;

        public Action openTOMLDialog;
        public Action<string> setTOMLText;
        public Func<string> getTOMLText;
        public Action<string> parseTOML;
        public Action executeTOMLOperations;

        public Action<LogModel> writeLog;
    }
}
