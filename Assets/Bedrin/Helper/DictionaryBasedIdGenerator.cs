using System.Collections.Generic;

namespace Bedrin.Helper
{
    public static class DictionaryBasedIdGenerator
    {
        private static int _idCounter;
        private static Dictionary<string, int> _ids;        
        
        public static int GetNewId(string displayName)
        {
            if (_ids == null)
            {
                _ids = new Dictionary<string, int>();
            }

            if (displayName == null)
            {
                return ++_idCounter;
            }
            
            if (_ids.ContainsKey(displayName))
            {
                return _ids[displayName];
            }

            _ids[displayName] = ++_idCounter;
            return _ids[displayName];
        }
    }
}
