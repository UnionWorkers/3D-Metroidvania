using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundsPresetSO", menuName = "Scriptable Objects/SoundsPresetSO")]
public class SoundsPresetSO : ScriptableObject
{
    public SoundSO[] SoundPresets;

    public Sound GetSoundByName(string name)
    {
        return Array.Find(SoundPresets, sound => sound.name == name).SoundPreset;
    }
}
