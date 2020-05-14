using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BombControl : MonoBehaviour
{
    public GameObject explosive;
    public GameObject energy;

    private Coroutine _selfDistractCoroutine;
    
    private void Start()
    {
        _selfDistractCoroutine = StartCoroutine(SelfDestruct());
    }

    private void OnDestroy()
    {
        StopCoroutine(_selfDistractCoroutine);
    }

    private IEnumerator SelfDestruct()
    {
        yield return new WaitForSeconds(Random.Range(3, 5));
        energy.SetActive(false);
        explosive.SetActive(true);
        yield return new WaitForSeconds(Random.Range(1, 2));
        GameObject o;
        (o = gameObject).SetActive(false);
        Destroy(o);
    }
}
