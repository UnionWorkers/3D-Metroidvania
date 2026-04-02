using System;
using Managers;
using UnityEngine;

namespace Utils.Effect
{
    public class RotatingObject : MonoBehaviour
    {
        [SerializeField] private RotatingUtil rotatingUtil;

        void Start()
        {
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Running:
                    enabled = true;
                    break;
                case GameState.Paused:
                    enabled = false;
                    break;
                case GameState.GameOver:
                    break;
            }
        }

        void Update()
        {
            rotatingUtil.RotateObject(transform);
        }

    }

}