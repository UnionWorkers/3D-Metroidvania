using System;
using UnityEngine;

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
        protected EntityState currentEntityState = EntityState.Active;
        protected EntityState oldEntityState = EntityState.Active;
        public Action<BaseEntity> OnEntityStateChanged;
        public Action<BaseEntity> OnEntityDestroy;
        [SerializeField] protected bool debugState = false;

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
            return;
        }

        public virtual void OnBeforeDestroy()
        {

            OnEntityStateChanged?.Invoke(this);
        }

        public virtual void OnUpdate()
        {
            return;
        }

        public virtual void OnFixedUpdate()
        {
            return;
        }

    }

}