using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Managers;
using System;
using Unity.VisualScripting;

public class RotatingObject : MonoBehaviour
{

    public Vector3 Rotation;
    public float Speed;
    public Collider[] Colliders;
    
    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(Rotation * Speed * Time.deltaTime * GameManager.Instance.ObjectsGameSpeed);
    }

    private void OnCollisionEnter(Collision Colliders)
    {
        if (Colliders.gameObject.CompareTag("Player"))
        {
            Debug.Log("Hit the PLayer");
        }
    }
}
