using System;
using Entities.Controller;
using UnityEngine;

namespace Interactable
{

    public enum ItemState
    {
        None,
        Highlighted,
        BeingUsed,
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


        public delegate void SelectableAction(PlayerController inPlayerController);
        public void Highlight();
        public void DeHighlight();
        public SelectableAction SelectInteractable();

    }
}

