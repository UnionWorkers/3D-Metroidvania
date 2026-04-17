using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundsPresetSO", menuName = "Scriptable Objects/SoundsPresetSO")]
public class SoundsPresetSO : ScriptableObject
{
    public SoundSO[] SoundPresets;

    public Sound GetSoundByName(string name)
    {
        if(SoundPresets.Length > 0)
        {
            return Array.Find(SoundPresets, sound => sound.name == name || sound.name.ToLower() == name.ToLower()).SoundPreset;
        }
        return null;
    }
}
