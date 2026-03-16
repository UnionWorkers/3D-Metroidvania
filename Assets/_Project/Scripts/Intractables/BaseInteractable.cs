using System;
using NUnit.Framework;
using UnityEngine;

namespace Interactable
{

    public class BaseInteractable : MonoBehaviour, IInteractable
    {
        protected bool isHighlighted = false;
        public event Action OnActionCompleted;

        public bool IsHighlighted => isHighlighted;
        public Transform GetTransform => transform;

        public void Highlight()
        {
            Debug.Log("Hi");
            isHighlighted = true;
        }

        public void DeHighlight()
        {
            Debug.Log("De Hi");

            isHighlighted = false;
        }

        public IInteractable.SelectableAction SelectInteractable()
        {
            Debug.Log("interactable");
            return test;
        }

        private void test()
        {
            Debug.Log("I work");
            OnActionCompleted?.Invoke();
            Destroy(gameObject);
        }

    }

}