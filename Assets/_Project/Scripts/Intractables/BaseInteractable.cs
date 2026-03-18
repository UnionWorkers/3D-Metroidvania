using System;
using Entities.Controller;
using NUnit.Framework;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Interactable
{
    public class BaseInteractable : MonoBehaviour, IInteractable
    {
        protected bool isHighlighted = false;

        public event Action OnActionCompleted;

        public bool IsHighlighted => isHighlighted;
        public Transform GetTransform => transform;
        protected ItemState itemState = ItemState.None; 
        public ItemState MyItemState
        {
            get => itemState;
            set => itemState = value;
        }

        private Color defaultColor;

        private void Start()
        {
            defaultColor = GetComponent<MeshRenderer>().material.GetColor("_BaseColor");
        }

        public virtual void Highlight()
        {
            GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.blue);
            itemState = ItemState.Highlighted;
            isHighlighted = true;
        }

        public virtual void DeHighlight()
        {
            GetComponent<MeshRenderer>().material.SetColor("_BaseColor", defaultColor);
            itemState = ItemState.None;
            isHighlighted = false;
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