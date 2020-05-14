using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using ATF;
using ATF.Scripts;
using Random = UnityEngine.Random;

public class CharacterControl : MonoBehaviour
{
    public float speed;
    public float torque;
    public float throwStrength;
    public float coolDown;
    public List<GameObject> bombTypes;
    
    private Rigidbody _rb;
    private Animator _ani;
    private Vector3 _forceVec;
    private bool _readyToFire;
    private Coroutine _bombCoroutine;

    private float _verticalThrust;
    private float _horizontalThrust;

    private static readonly int Running = Animator.StringToHash("Running");
    private static readonly int Fire = Animator.StringToHash("Fire");
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _ani = GetComponent<Animator>();
    }

    private void Update()
    {
        _verticalThrust = AtfInput.GetAxis("Vertical") * speed;
        _horizontalThrust = AtfInput.GetAxis("Horizontal") * torque;
        _ani.SetBool(Running, _forceVec.magnitude > 0);
        if (_bombCoroutine != null || !AtfInput.GetKey(KeyCode.Space)) return;
        _bombCoroutine = StartCoroutine(BombCoroutine());
    }

    private IEnumerator BombCoroutine()
    {
        _ani.SetTrigger(Fire);
        var myTransform = transform;
        var bullet = Instantiate(bombTypes[Random.Range(0, bombTypes.Count)], myTransform.position + (myTransform.forward * 2), myTransform.rotation);
        bullet.GetComponent<Rigidbody>().AddForce(transform.forward * throwStrength, ForceMode.Impulse);
        yield return new WaitForSeconds(coolDown);
        _bombCoroutine = null;
    }
    
    private void FixedUpdate()
    {
        _forceVec = transform.forward * _verticalThrust;
        _rb.AddForce(_forceVec, ForceMode.VelocityChange);
        _rb.AddTorque(transform.up * _horizontalThrust);
    }
}
