using System;
using NUnit.Framework;
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
        private ItemState itemState = ItemState.None; 
        public ItemState MyItemState
        {
            get => itemState;
            set => itemState = value;
        }

        private Color defaultColor;

        void Start()
        {
            defaultColor = GetComponent<MeshRenderer>().material.GetColor("_BaseColor");
        }

        public void Highlight()
        {
            GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.blue);
            itemState = ItemState.Highlighted;
            isHighlighted = true;
        }

        public void DeHighlight()
        {
            GetComponent<MeshRenderer>().material.SetColor("_BaseColor", defaultColor);
            itemState = ItemState.None;
            isHighlighted = false;
        }

        public IInteractable.SelectableAction SelectInteractable()
        {
            return test;
        }

        private void test()
        {
            itemState = ItemState.Destroyed;
            OnActionCompleted?.Invoke();
            Destroy(gameObject);
        }

    }

}