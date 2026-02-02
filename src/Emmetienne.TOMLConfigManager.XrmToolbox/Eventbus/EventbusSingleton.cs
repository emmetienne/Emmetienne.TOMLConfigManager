using Emmetienne.TOMLConfigManager.Logger;
using Emmetienne.TOMLConfigManager.Models;
using System;

namespace Emmetienne.TOMLConfigManager.Eventbus
{
    public class EventbusSingleton : Singleton<EventbusSingleton>
    {
        public Action<bool> disableUiElements;

        public Action<LogModel> writeLog;
    }
}
