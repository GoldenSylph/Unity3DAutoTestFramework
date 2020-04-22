using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SlimeRabbitControl : MonoBehaviour
{
    private Animator _ani;
    private Coroutine _changeCoroutine;
    private static readonly int Change = Animator.StringToHash("Change");

    private void Start()
    {
        _ani = GetComponent<Animator>();
        _changeCoroutine = StartCoroutine(ChangeCoroutine());
    }

    private void OnDestroy()
    {
        StopCoroutine(_changeCoroutine);
    }

    private IEnumerator ChangeCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2, 10));
            _ani.SetTrigger(Change);
        }
    }

}
