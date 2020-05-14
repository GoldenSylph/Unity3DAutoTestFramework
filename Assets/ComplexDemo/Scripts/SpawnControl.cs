using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class SpawnControl : MonoBehaviour
{

    public GameObject mob;
    public float delay;

    private Coroutine _spawnCoroutine;
    
    private void Start()
    {
        _spawnCoroutine = StartCoroutine(Spawn());
    }

    private void OnDestroy()
    {
        StopCoroutine(_spawnCoroutine);
    }

    private IEnumerator Spawn()
    {
        while (true)
        {
            var myTransform = transform;
            var newMob = Instantiate(mob, myTransform.position, myTransform.rotation);
            newMob.SetActive(true);
            var newMobsAgent = newMob.GetComponent<NavMeshAgent>();
            newMob.GetComponent<Collider>().isTrigger = false;
            newMobsAgent.enabled = true;
            yield return new WaitForSeconds(delay + Random.Range(-5, 5));
        }
    }
    
}
