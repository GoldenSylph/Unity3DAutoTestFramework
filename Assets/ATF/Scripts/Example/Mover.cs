using UnityEngine;

namespace ATF.Scripts.Example
{
    public class Mover : MonoBehaviour
    {
        private const float Speed = 100f;

        private void Update()
        {
            if (ATFInput.GetKey(KeyCode.Space))
            {
                transform.Rotate(new Vector3(0, 1, 1), Time.deltaTime * Speed);
            }
        }
    }
}
