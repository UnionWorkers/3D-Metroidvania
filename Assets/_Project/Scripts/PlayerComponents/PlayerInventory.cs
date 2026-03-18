using System.Collections.Generic;
using Interactable.Key;
using UnityEngine;

public class PlayerInventory
{

    private List<Key> keyHolder = new();
    public PlayerInventory()
    {

    }

    public void AddKey(Key inKey)
    {
        keyHolder.Add(inKey);
    }

    public bool HasKey(Key inKey)
    {
        foreach (var key in keyHolder)
        {
            if (key.CompareKey(inKey))
            {
                return true;
            }
        }

        return false;
    }

    public bool HasKey(Key inKey, out int keyIndex)
    {
        keyIndex = -1;

        for (int i = 0; i < keyHolder.Count; i++)
        {
            if (keyHolder[i].CompareKey(inKey))
            {
                keyIndex = i;
                return true;
            }

        }
        return false;
    }

    public bool UseKey(Key inKey)
    {
        if (HasKey(inKey, out int keyIndex) && keyIndex != -1)
        {
            switch (keyHolder[keyIndex].ActionWhenUsed)
            {
                case ActionWhenUsed.Reusable:
                    break;
                case ActionWhenUsed.DeleteSelf:
                    keyHolder.Remove(keyHolder[keyIndex]);
                    break;
            }
            return true;
        }

        return false;
    }



}
