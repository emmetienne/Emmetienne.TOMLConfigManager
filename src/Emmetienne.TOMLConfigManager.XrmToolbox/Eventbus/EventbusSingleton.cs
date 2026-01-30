using Emmetienne.TOMLConfigManager.Models;
using System;

namespace Emmetienne.TOMLConfigManager.Eventbus
{
    public class EventbusSingleton : Singleton<EventbusSingleton>
    {
        public Action<bool> disableUiElements;
    }
}
