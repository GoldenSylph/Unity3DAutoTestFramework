using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MobControl : MonoBehaviour
{
    public GameObject explosion;
    
    private GameObject _player;
    private NavMeshAgent _agent;
    private Animator _ani;
    private Collider _collider;
    
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Die = Animator.StringToHash("Die");
    private static readonly int Walking = Animator.StringToHash("Walking");

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _agent = GetComponent<NavMeshAgent>();
        _ani = GetComponent<Animator>();
        _collider = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!other.gameObject.CompareTag("Bullet")) return;
        _agent.enabled = false;
        _ani.SetTrigger(Die);
        StartCoroutine(SelfDestructDeadBody());
    }

    private IEnumerator SelfDestructDeadBody()
    {
        explosion.SetActive(true);
        _collider.isTrigger = true;
        yield return new WaitForSeconds(3);
        explosion.SetActive(false);
        yield return new WaitForSeconds(8);
        gameObject.SetActive(false);
    }
    
    private void Update()
    {
        if (!_agent.enabled) return;
        _agent.destination = _player.transform.position;
        if (_agent.remainingDistance < 2f)
        {
            _ani.SetBool(Walking, false);
            _ani.SetTrigger(Attack);
        }
        else
        {
            _ani.SetBool(Walking, true);
        }
    }
}
