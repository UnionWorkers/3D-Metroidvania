using System;
using Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Entities
{
    public enum EntityState : byte
    {
        Active,
        Disabled,
        ToDestroy
    }

    public class BaseEntity : MonoBehaviour
    {
        private EntityState currentEntityState = EntityState.Active;
        private EntityState oldEntityState = EntityState.Active;
        public Action<BaseEntity> OnEntityStateChanged;
        public Action<BaseEntity> OnEntityDestroy;

        public EntityState EntityState
        {
            get => currentEntityState;
            set
            {
                if (value == currentEntityState)
                {
                    return;
                }

                oldEntityState = currentEntityState;
                currentEntityState = value;

                OnEntityStateChanged?.Invoke(this);
            }
        }
        public EntityState OldEntityState => oldEntityState;

        public virtual void OnInitialize()
        {

        }

        public virtual void OnBeforeDestroy()
        {

            OnEntityStateChanged?.Invoke(this);
        }

        public virtual void OnUpdate()
        {

        }

        // for testing remove later 
        void Update()
        {
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                GameManager.Instance.ChangeGameState(GameState.Paused);
            }

            if (Keyboard.current.oKey.wasPressedThisFrame)
            {
                GameManager.Instance.ChangeGameState(GameState.Running);
            }
        }
    }

}