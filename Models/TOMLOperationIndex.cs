namespace Emmetienne.TOMLConfigManager.Models
{
    internal class TOMLOperationIndex : Singleton<TOMLOperationIndex>
    {
        private int currentIndex = 0;

        public int NextIndex { get { return ++currentIndex; } }
        public void ResetIndex()
        {
            currentIndex = 0;
        }
    }
}
