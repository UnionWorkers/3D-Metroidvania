using System;
using UnityEngine;

namespace Interactable
{

    public enum ItemState
    {
        None,
        Highlighted,
        Destroyed
    }

    public interface IInteractable
    {
        public event Action OnActionCompleted;
        public Transform GetTransform => throw new NotImplementedException("GetTransform is not implemented");
        public ItemState MyItemState
        {
            get => throw new NotImplementedException("Get ItemState is not implemented");
            set => throw new NotImplementedException("Set ItemState is not implemented");
        }


        public delegate void SelectableAction();
        public void Highlight();
        public void DeHighlight();
        public SelectableAction SelectInteractable();

    }
}

