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

    }

}