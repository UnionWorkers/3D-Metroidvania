using System;
using Managers;
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
        [SerializeField] private CinemachineInputAxisController cinemachineInput;

        public Camera MainCamera => mainCamera;
        public Quaternion Rotation => mainCamera.transform.rotation;
        public Vector3 Position => mainCamera.transform.position;
        public Transform CurrentTarget => targetTransform;


        void Awake()
        {
            mainCamera = Camera.main;

            cinemachineInput = GetComponent<CinemachineInputAxisController>();

            if (cinemachineCamera != null) { return; }
            cinemachineCamera = GetComponent<CinemachineCamera>();
        }

        public override void OnInitialize()
        {
            GameManager.Instance.OnGameStateChanged += GameStateChanged;
        }

        private void GameStateChanged(GameState state)
        {

            switch (state)
            {
                case GameState.Running:
                    cinemachineCamera.enabled = true;
                    cinemachineInput.enabled = true;
                    break;
                case GameState.Paused:
                    cinemachineCamera.enabled = false;
                    cinemachineInput.enabled = false;

                    break;
            }

        }

        public void SetTarget(Transform inTarget)
        {
            targetTransform = inTarget;
            cinemachineCamera.Follow = targetTransform;
        }
    }

}