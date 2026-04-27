using System;
using UnityEngine;

namespace Interactable.Key
{
    public enum ActionWhenUsed
    {
        Reusable,
        DeleteSelf,
    }


    [System.Serializable]
    public class Key
    {
        [SerializeField] private ActionWhenUsed actionWhenUsed;
        public ActionWhenUsed ActionWhenUsed => actionWhenUsed;

        public bool CompareKey(Key inKey)
        {
            if(inKey == this)
            {
                return true;
            }            

            return false;
        }
    }

    [CreateAssetMenu(fileName = "KeySO", menuName = "Scriptable Objects/KeySO")]
    public class KeySO : ScriptableObject
    {
        [SerializeField] private Key keyInfo;
        public Key Key => keyInfo;
        public bool HasKey => keyInfo != null;

    }

}