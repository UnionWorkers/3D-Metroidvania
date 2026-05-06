using System;
using System.Collections;
using Entities.Controller;
using Interactable;
using Interactable.Key;
using Managers;
using NUnit.Framework;
using UnityEngine;

public enum DoorState : byte
{
    Open,
    Closed,
    Locked,
}

public enum DoorAction : byte
{
    OpenOnce,
    Reusable,
    Inactive
}

public class DoorInteractable : BaseInteractable
{
    [SerializeField] private DoorState doorState = DoorState.Locked;
    [SerializeField] private DoorAction doorAction = DoorAction.OpenOnce;
    [SerializeField] private KeySO keySO;

    [Header("Rotation variables")]
    [SerializeField] private Transform rotationObject;
    [SerializeField] private float timeFromStartToFinished = 3f;
    [SerializeField] private float rotateAmount = 90f;
    [SerializeField] private Vector3 rotateDirection;
    private bool isDoorAnimating = false;
    private float currentToFinishTimer = 0;
    private float previousRotation;

    private Key key => keySO.Key;

    protected override void Start()
    {
        Validate();
        if (rotationObject == null) { return; }

        currentToFinishTimer = timeFromStartToFinished;
        previousRotation = rotationObject.localEulerAngles.y;
        rotateDirection = Utils.Math.MyMath.ClampVector(rotateDirection, Utils.Math.MyMath.ClampMode.ZeroToOne);
    }

    private bool Validate()
    {
        if (keySO == null) { Debug.LogWarning("Has no KeySO, this items will not work"); return false; }

        if (!keySO.HasKey) { Debug.LogWarning("KeySO Has no key, item will not work"); return false; }

        return true;
    }

    public override void Highlight()
    {
        base.Highlight();
    }

    public override void DeHighlight()
    {
        base.DeHighlight();
    }

    protected override void InteractableAction(PlayerController inPlayerController)
    {
        if (!Validate()) { return; }

        if (doorState == DoorState.Locked && !inPlayerController.Inventory.HasKey(key) || isDoorAnimating)
        {
            return;
        }

        switch (doorAction)
        {
            case DoorAction.OpenOnce:
                CommitAction(inPlayerController);
                doorAction = DoorAction.Inactive;
                break;
            case DoorAction.Reusable:
                CommitAction(inPlayerController);
                break;
            case DoorAction.Inactive:
                break;
        }
    }

    protected virtual void CommitAction(PlayerController inPlayerController)
    {
        if (isDoorAnimating) { return; }

        switch (doorState)
        {
            case DoorState.Open:
                StartCoroutine(DoorAnimation());
                doorState = DoorState.Closed;
                break;
            case DoorState.Closed:
                StartCoroutine(DoorAnimation());
                doorState = DoorState.Open;
                break;
            case DoorState.Locked:
                inPlayerController.Inventory.UseKey(key);
                doorState = DoorState.Closed;
                CommitAction(inPlayerController);
                break;
        }
    }

    private IEnumerator DoorAnimation()
    {
        isDoorAnimating = true;

        while (currentToFinishTimer >= 0f)
        {

            rotationObject.localEulerAngles += rotateDirection * (rotateAmount / timeFromStartToFinished * Time.deltaTime * GameManager.Instance.GameSpeed);
            currentToFinishTimer -= Time.deltaTime * GameManager.Instance.GameSpeed;

            yield return new WaitForEndOfFrame();
        }

        Vector3 eulerAngles = rotationObject.localEulerAngles;
        eulerAngles.y =  (previousRotation - rotateAmount) * -rotateDirection.y;
        rotationObject.localEulerAngles = eulerAngles;
        previousRotation = rotationObject.localEulerAngles.y;
        currentToFinishTimer = timeFromStartToFinished;

        rotateDirection.y *= -1;
        isDoorAnimating = false;

    }

}
