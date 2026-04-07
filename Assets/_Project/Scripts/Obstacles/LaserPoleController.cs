using System;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Managers;
using UnityEngine;
using Utils.Math;
using Utils.Triggers;

public class LaserPoleController : BaseEntity
{
    [SerializeField] private bool canAnimate = true;
    [SerializeField] private Transform animationObject;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float moveSpeed;
    [SerializeField] private Vector3 rotateDirection;
    [Space(15)]
    [SerializeField] private Transform laserHolder;
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private int lasersToSpawn;
    [SerializeField] private Vector3 laserScale;
    [SerializeField]private List<LaserTrigger> laserTriggers = new();

    [Space(15)]
    [SerializeField] private Vector3 startPoint;
    [SerializeField] private Vector3 endPoint;

    public Vector3 StartPoint
    {
        get => startPoint + transform.position;
        set => startPoint = (value - transform.position);
    }
    public Vector3 EndPoint
    {
        get => endPoint + transform.position;
        set => endPoint = (value - transform.position);
    }

    private float dist;
    private float remainingDist;
    private bool isMovingToEnd = false;
    private Vector3 movePoint;
    private Vector3 currentStartPoint;

    public void Validate()
    {
        CleanUp();

        for (int i = 0; i < lasersToSpawn; i++)
        {
            LaserTrigger laserTrigger = Instantiate(laserPrefab, laserHolder.position, Quaternion.identity, laserHolder).GetComponent<LaserTrigger>();
            laserTriggers.Add(laserTrigger);
        }

        SetLaserSettings();
    }

    private void OnValidate()
    {
        if (laserTriggers != null && laserTriggers.Count > 0)
        {
            SetLaserSettings();
        }
    }

    private void CleanUp()
    {
        if (laserHolder != null)
        {
            if (Application.isPlaying)
            {
                foreach (Transform child in laserHolder)
                {
                    Destroy(child.gameObject);
                }
            }
            else
            {
                while (laserHolder.childCount != 0)
                {
                    DestroyImmediate(laserHolder.GetChild(0).gameObject);
                }

            }
        }

        laserTriggers.Clear();
    }

    private void SetLaserSettings()
    {
        float laserRotation = 360 / laserHolder.childCount;
        for (int i = 0; i < laserHolder.childCount; i++)
        {
            Transform child = laserHolder.GetChild(i);
            child.localScale = laserScale;
            child.eulerAngles = new Vector3(0, laserRotation * i, 0);
        }

    }

    private void Awake()
    {
        rotateDirection = MyMath.ClampVector(rotateDirection, MyMath.ClampMode.MinusOneToOne);
        currentStartPoint = animationObject.position;
        movePoint = isMovingToEnd ? EndPoint : StartPoint;
        isMovingToEnd = !isMovingToEnd;

        foreach (var trigger in laserTriggers)
        {
            trigger.OnLaserTriggerCollision += OnLaserTriggers;
        }
    }

    private void OnLaserTriggers(Collider collider, CollisionTriggerType type, Transform senderTransform)
    {
        switch (type)
        {
            case CollisionTriggerType.Enter:
                if (collider.CompareTag("Player"))
                {
                    IHealth health = collider.GetComponent<IHealth>();
                    if (health != null)
                    {
                        health.TakeDamage(new(1, senderTransform));
                    }
                }
                break;
        }
    }

    public override void OnUpdate()
    {
        if (!canAnimate)
        {
            return;
        }

        if (remainingDist > 0)
        {
            animationObject.position = Vector3.Lerp(currentStartPoint, movePoint, 1 - (remainingDist / dist));
            remainingDist -= moveSpeed * GameManager.Instance.ObjectsGameSpeed * Time.deltaTime;
        }
        else
        {
            movePoint = isMovingToEnd ? EndPoint : StartPoint;
            isMovingToEnd = !isMovingToEnd;
            currentStartPoint = animationObject.position;

            dist = Vector3.Distance(currentStartPoint, movePoint);
            remainingDist = dist;
        }

        animationObject.Rotate(rotateDirection * rotationSpeed * Time.deltaTime * GameManager.Instance.ObjectsGameSpeed);
    }
}
