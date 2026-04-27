using Managers;
using UnityEngine;

namespace Utils.Effect
{
    [System.Serializable]
    public class RotatingUtil
    {
        [SerializeField] private Vector3 Rotation;
        [SerializeField] private float rotationSpeed = 1f;

        public virtual void RotateObject(Transform objectToRotate, float deltaTime)
        {
            objectToRotate.Rotate(Rotation * rotationSpeed * deltaTime * GameManager.Instance.ObjectsGameSpeed);
        }

        public virtual void RotateRoundPivot(Transform objectToRotate, Vector3 pivot, float deltaTime)
        {
            objectToRotate.position = Quaternion.Euler(deltaTime * rotationSpeed * Rotation) * (objectToRotate.position - pivot) + pivot;
        }
    }

}