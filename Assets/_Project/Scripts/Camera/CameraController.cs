using UnityEngine;

namespace Entities.CameraControl
{
    [ExecuteInEditMode]
    public class CameraController : BaseEntity
    {
        private Camera mainCamera;
        [SerializeField] private Transform targetTransform;
        [SerializeField] private Vector3 cameraOffset;
        [Range(0f, 1f)]
        [SerializeField] private float distanceFromTarget;

        public Camera MainCamera => mainCamera;
        public Quaternion Rotation => transform.rotation;
        public Vector3 Position => transform.position;
        public Transform CurrentTarget => targetTransform;

        void Awake()
        {
            mainCamera = Camera.main;
        }
        public void SetTarget(Transform inTarget)
        {
            targetTransform = inTarget;
        }

        public override void OnInitialize()
        {

        }

        public override void OnUpdate()
        {
            if (targetTransform == null)
            {
                return;
            }

            FollowTarget();

        }

#if UNITY_EDITOR
        void Update()
        {
            if (targetTransform == null)
            {
                return;
            }

            FollowTarget();
        }
#endif

        private void FollowTarget()
        {
            // Vector3 cameraDir = -targetTransform.forward;
            // Quaternion rotation = Quaternion.AngleAxis(0.5f * 60f, targetTransform.right);
            // cameraDir = rotation * cameraDir;
            // Vector3 cameraCalc = targetTransform.position + cameraDir * 10f;

            transform.position = targetTransform.position - transform.forward * 10f;

        }

        public void RotateCamera(Vector2 inDirection)
        {
            // smooth this, and remove magic number 
            transform.eulerAngles += new Vector3(0, inDirection.x * 2f, 0);
        }

    }

}