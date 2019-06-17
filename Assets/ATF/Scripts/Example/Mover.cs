using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ATF.Example
{
    public class Mover : MonoBehaviour
    {

        private readonly float Speed = 100f;

        void Update()
        {
            if (ATFInput.GetKey(KeyCode.Space))
            {
                transform.Rotate(new Vector3(0, 1, 1), Time.deltaTime * Speed);
            }
        }
    }
}
