using Unity.Cinemachine;
using UnityEngine;

namespace Entities.CameraControl
{
    [ExecuteInEditMode]
    public class CameraController : BaseEntity
    {
        private Camera mainCamera;
        [SerializeField] private Transform targetTransform;
        [SerializeField] private CinemachineCamera cinemachineCamera;

        public Camera MainCamera => mainCamera;
        public Quaternion Rotation => mainCamera.transform.rotation;
        public Vector3 Position => mainCamera.transform.position;
        public Transform CurrentTarget => targetTransform;



        void Awake()
        {
            mainCamera = Camera.main;

            if (cinemachineCamera != null) { return; }
            cinemachineCamera = GetComponent<CinemachineCamera>();

        }
        public void SetTarget(Transform inTarget)
        {
            targetTransform = inTarget;
            cinemachineCamera.Follow = targetTransform;
        }
    }

}