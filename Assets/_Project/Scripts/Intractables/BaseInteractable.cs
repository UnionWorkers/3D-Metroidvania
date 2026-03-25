using System;
using Entities.Controller;
using UnityEngine;

namespace Interactable
{
    public class BaseInteractable : MonoBehaviour, IInteractable
    {

        public event Action OnActionCompleted;

        public Transform GetTransform => transform;
        protected ItemState itemState = ItemState.None; 
        public ItemState MyItemState
        {
            get => itemState;
            set => itemState = value;
        }

        protected Color defaultColor;

        protected virtual void Start()
        {
            defaultColor = GetComponent<MeshRenderer>().material.GetColor("_BaseColor");
        }

        public virtual void Highlight()
        {
            GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.blue);
            itemState = ItemState.Highlighted;
        }

        public virtual void DeHighlight()
        {
            GetComponent<MeshRenderer>().material.SetColor("_BaseColor", defaultColor);
            itemState = ItemState.None;
        }

        public virtual IInteractable.SelectableAction SelectInteractable()
        {
            return InteractableAction;
        }

        protected virtual void InteractableAction(PlayerController inPlayerController)
        {
            itemState = ItemState.Destroyed;
            OnActionCompleted?.Invoke();
            Destroy(gameObject);
        }

    }

}