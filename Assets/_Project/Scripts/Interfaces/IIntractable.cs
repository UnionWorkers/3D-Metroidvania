using System;
using UnityEngine;

namespace Interactable
{
    public interface IInteractable
    {
        public event Action OnActionCompleted;
        public Transform GetTransform => throw new NotImplementedException("GetTransform is not implemented"); 

        public delegate void SelectableAction();
        public void Highlight();
        public void DeHighlight();
        public SelectableAction SelectInteractable();

    }
}

