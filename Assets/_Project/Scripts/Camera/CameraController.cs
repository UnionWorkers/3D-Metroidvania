using System.Threading;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Entities.CameraControl
{
    [ExecuteInEditMode]
    public class CameraController : BaseEntity
    {
        private Camera mainCamera;

        [Header("Follow Variables")]
        [SerializeField] private Transform targetTransform;
        [Range(0f, 40f)]
        [SerializeField] private float distanceFromTarget;
        [SerializeField] private float distanceSmoothing;
        [SerializeField] private float minDistFromTarget, maxDistFromTarget;
        [SerializeField] private float sphereCollisionRadius = 0.5f;
        private float currentDistFromTarget;
        private LayerMask layerMask = Physics.AllLayers;


        [Space(15)]
        [Header("Rotation Variables")]
        [Range(0, 90f)][SerializeField] private float upperVerticalLimit = 35f;
        [Range(0, 90f)][SerializeField] private float lowerVerticalLimit = 35f;
        [SerializeField] private float cameraSpeed = 50f;
        [Range(1, 50f)][SerializeField] private float cameraSmoothing = 25f;
        [Range(0.01f, 2f)][SerializeField] private float ySensitivity = 0.3f;
        [Range(0.01f, 2f)][SerializeField] private float xSensitivity = 1f;
        private Vector2 currentAngels;


        public Camera MainCamera => mainCamera;
        public Quaternion Rotation => transform.rotation;
        public Vector3 Position => transform.position;
        public Transform CurrentTarget => targetTransform;



        void Awake()
        {
            mainCamera = Camera.main;
            currentAngels = transform.localRotation.eulerAngles;

            layerMask &= ~(1 << LayerMask.NameToLayer("Ignore Raycast"));
            currentDistFromTarget = (targetTransform.position - transform.position).magnitude;
        }
        public void SetTarget(Transform inTarget)
        {
            targetTransform = inTarget;
        }

        public override void OnInitialize()
        {
            layerMask &= ~(1 << LayerMask.NameToLayer("Ignore Raycast"));
            currentDistFromTarget = (targetTransform.position - transform.position).magnitude;
        }

        public override void OnUpdate()
        {
            if (targetTransform == null)
            {
                return;
            }

            //FollowTarget(); dose not work :[
            transform.position = targetTransform.position - transform.forward * distanceFromTarget;

        }

#if UNITY_EDITOR
        void Update()
        {
            if (targetTransform == null)
            {
                return;
            }

            transform.position = targetTransform.position - transform.forward * distanceFromTarget;
        }
#endif

        private void FollowTarget()
        {
            Vector3 castingDirection = transform.position - targetTransform.position;

            float dist = GetCameraDistance(castingDirection);

            currentDistFromTarget = Mathf.Lerp(currentDistFromTarget, dist, Time.deltaTime * distanceSmoothing);
            transform.position = targetTransform.position + castingDirection.normalized * currentDistFromTarget;
        }

        private float GetCameraDistance(Vector3 castingDirection)
        {
            float dist = castingDirection.magnitude + minDistFromTarget;

            if (Physics.SphereCast(new Ray(targetTransform.position, castingDirection), sphereCollisionRadius, out RaycastHit hit, dist, layerMask, QueryTriggerInteraction.Ignore))
            {
                return Mathf.Max(0f, hit.distance, - minDistFromTarget);
            }
            return castingDirection.magnitude;
        }

        public void RotateCamera(Vector2 inDirection)
        {
            inDirection.y *= ySensitivity;
            inDirection.x *= xSensitivity;

            inDirection = Vector2.Lerp(Vector2.zero, inDirection, Time.deltaTime * cameraSmoothing);

            currentAngels += inDirection * cameraSpeed * Time.deltaTime;
            currentAngels.y = Mathf.Clamp(currentAngels.y, -lowerVerticalLimit, upperVerticalLimit);

            transform.localRotation = Quaternion.Euler(currentAngels.y, currentAngels.x, 0);

        }

    }

}