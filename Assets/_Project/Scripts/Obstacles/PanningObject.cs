using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Managers;
using System;
using Unity.VisualScripting;

public class PanningObject : MonoBehaviour
{

    [SerializeField] private List<Transform> panningPoints = new List<Transform>();
    [SerializeField] private float speed = 10;
    [SerializeField] private float minDist = 0.5f;


    private int index = 0;
    private int indexDir = 1;
    private Rigidbody body;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (panningPoints.Count == 0)
        {
            GameObject go = new GameObject();
            go.transform.position = transform.position;
            panningPoints.Add(go.transform);
        }

        body = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector3 dist = new Vector3(panningPoints[index].position.x, 0, panningPoints[index].position.z) - new Vector3(transform.position.x, 0, transform.position.z);

        if (dist.magnitude < minDist && panningPoints.Count <= 1)

            return;

        transform.rotation = Quaternion.LookRotation(dist, Vector3.up);
        body.linearVelocity = dist.normalized * speed * GameManager.Instance.ObjectsGameSpeed;

        if (dist.magnitude > minDist)
            return;

        if ((index += indexDir) >= panningPoints.Count)
        {
            index = panningPoints.Count - 1;
            indexDir *= -1;
        }
        else if (index < 0)
        {
            index = 0;
            indexDir *= -1;
        }
    }
}
