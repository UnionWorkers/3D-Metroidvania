using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Managers;
using System;
using Unity.VisualScripting;
public class FallingPlatform : MonoBehaviour
{
    private Vector3 StartingPosition;
    private float Timer;
    public float FallCountdown;
    public float ResetTimer;
    public Rigidbody Platform;
    private bool HasPlayerEntered;
    private bool HasFallen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartingPosition = this.transform.position;
        Platform.isKinematic = true;
        Platform.useGravity = false;

    }

    // Update is called once per frame
    void Update()
    {
        if (HasPlayerEntered == true)
        {
            Timer += Time.deltaTime * GameManager.Instance.ObjectsGameSpeed;
            if (Timer >= FallCountdown)
            {
                Platform.isKinematic = false;
                Platform.useGravity = true;
                HasFallen = true;
                Timer = 0;
                HasPlayerEntered = false;

            }
        }

        if (HasFallen == true) 
        {
            Timer += Time.deltaTime * GameManager.Instance.ObjectsGameSpeed;

            if ( Timer >= ResetTimer)
            {
                Platform.isKinematic = true;
                Platform.useGravity = false;
                this.transform.position = StartingPosition;
                Timer = 0;
                HasFallen = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            HasPlayerEntered = true;
        }
    }
}
