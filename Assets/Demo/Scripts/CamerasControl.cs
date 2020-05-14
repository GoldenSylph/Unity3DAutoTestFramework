using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamerasControl : MonoBehaviour
{
    public List<GameObject> cameras;

    private GameObject _player;
    private Coroutine _updateCamerasCoroutine;
    
    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _updateCamerasCoroutine = StartCoroutine(UpdateCameras());
    }

    private void OnDestroy()
    {
        StopCoroutine(_updateCamerasCoroutine);
    }

    private IEnumerator UpdateCameras()
    {
        while (true)
        {
            cameras.Sort((cameraA, cameraB) =>
            {
                var position = _player.transform.position;
                var aDistance = Vector3.Distance(cameraA.transform.position, position);
                var bDistance = Vector3.Distance(cameraB.transform.position, position);
                if (aDistance > bDistance)
                {
                    return 1;
                }
                if (aDistance < bDistance)
                {
                    return -1;
                }
                return 0;
            });
            yield return new WaitForSeconds(0.1f);
            cameras[0].gameObject.SetActive(true);
            for (var i = 1; i < cameras.Count; i++)
            {
                cameras[i].gameObject.SetActive(false);
            }
        }
    }
}
