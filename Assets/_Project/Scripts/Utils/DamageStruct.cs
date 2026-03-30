using UnityEngine;

[System.Serializable]
public struct DamageStruct
{
    [SerializeField] private int damageAmount;

    public int DamageAmount => damageAmount;

}
