using UnityEngine;

namespace Bedrin.Helper
{
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {

        private static T _sInstance;
        private static bool _sIsDestroyed;

        public static T Instance
        {
            get
            {
                if (_sIsDestroyed)
                    return null;

                if (_sInstance == null)
                {
                    _sInstance = FindObjectOfType(typeof(T)) as T;

                    if (_sInstance == null)
                    {
                        var gameObject = new GameObject(typeof(T).Name);
                        DontDestroyOnLoad(gameObject);

                        _sInstance = gameObject.AddComponent(typeof(T)) as T;
                    }
                }

                return _sInstance;
            }
        }

        protected virtual void OnDestroy()
        {
            if (_sInstance)
                Destroy(_sInstance);

            _sInstance = null;
            _sIsDestroyed = true;
        }

        public bool IsLive()
        {
            return _sIsDestroyed;
        }
    }
}
