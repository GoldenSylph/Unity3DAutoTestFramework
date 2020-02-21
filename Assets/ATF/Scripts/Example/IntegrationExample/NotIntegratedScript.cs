using UnityEngine;

namespace ATF.Scripts.Example.IntegrationExample
{
    public class NotIntegratedScript : MonoBehaviour
    {
        private const float SPEED = 100f;

        private void Update()
        {
            if (AtfInput.GetKey(KeyCode.Space))
            {
                transform.Rotate(new Vector3(0, 1, 1), Time.deltaTime * SPEED);
            }
        }
    }
}