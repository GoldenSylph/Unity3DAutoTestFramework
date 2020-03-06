using ATF.Scripts.DI;
using UnityEngine.EventSystems;

namespace ATF.Scripts
{
    public class AtfStandaloneInputManager : StandaloneInputModule
    {
        protected override void Start()
        {
            base.Start();
            m_InputOverride = gameObject.AddComponent<AtfInput>();
            DependencyInjector.InjectType(m_InputOverride.GetType());
        }
    }
}
