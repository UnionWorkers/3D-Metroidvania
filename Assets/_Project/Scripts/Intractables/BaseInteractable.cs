using System;
using Entities.Controller;
using UnityEngine;
using UnityEngine.VFX;

namespace Interactable
{
    public class BaseInteractable : MonoBehaviour, IInteractable
    {

        public Transform GetTransform => transform;
        protected ItemState itemState = ItemState.None;
        protected ParticleSystem activeVFX;
        [SerializeField] protected ParticleSystem interactVFX;
        [SerializeField] protected ParticleSystem interactableVFX;


        public ItemState MyItemState
        {
            get => itemState;
            set => itemState = value;
        }

        protected virtual void Start()
        {
            // add Resources.Load for VFX 
        }

        public virtual void Highlight()
        {
            if (interactableVFX != null && activeVFX == null)
            {
                activeVFX = Instantiate(interactableVFX, transform.position, Quaternion.identity);
                activeVFX.Play();
            }
            itemState = ItemState.Highlighted;
        }

        public virtual void DeHighlight()
        {
            if (activeVFX != null)
            {
                Destroy(activeVFX.gameObject);
            }

            itemState = ItemState.None;
        }

        public virtual IInteractable.SelectableAction SelectInteractable()
        {
            return InteractableAction;
        }

        protected virtual void InteractableAction(PlayerController inPlayerController)
        {
            itemState = ItemState.Destroyed;
            Destroy(gameObject);
        }

        protected virtual void OnDestroy()
        {
            if (activeVFX != null)
            {
                Destroy(activeVFX);
            }
            if (interactVFX != null)
            {
                activeVFX = Instantiate(interactVFX, transform.position, Quaternion.identity);
                activeVFX.Play();
                Destroy(activeVFX, 1f);
            }
        }
    }

}