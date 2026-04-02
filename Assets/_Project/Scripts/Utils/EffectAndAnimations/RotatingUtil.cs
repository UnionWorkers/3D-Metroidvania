using Managers;
using UnityEngine;

namespace Utils.Effect
{
    [System.Serializable]
    public class RotatingUtil
    {
        [SerializeField] private Vector3 Rotation;
        [SerializeField] private float rotationSpeed = 1f;

        public virtual void RotateObject(Transform objectToRotate)
        {
            objectToRotate.Rotate(Rotation * rotationSpeed * Time.deltaTime * GameManager.Instance.ObjectsGameSpeed);
        }
    }

}