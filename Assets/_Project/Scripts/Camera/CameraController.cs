
using Unity.Mathematics;
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
            Vector3 cameraCalc = cameraOffset + transform.position;
            transform.position = Vector3.Lerp(cameraCalc, targetTransform.position, distanceFromTarget);
        }

        public void RotateCamera(Vector2 direction)
        {
            Vector2 targetPos = targetTransform.position;
            Vector2 myPos = transform.position;
            Vector2 newPos = new Vector2(
                targetPos.x + (myPos.x - targetPos.x) * Mathf.Cos(direction.x) - (myPos.y - targetPos.y) * Mathf.Sin(direction.x),
                targetPos.y + (myPos.x - targetPos.x) * Mathf.Sin(direction.y) - (myPos.y - targetPos.y) * Mathf.Cos(direction.y)
            );

            transform.position = newPos;
        }

    }

}