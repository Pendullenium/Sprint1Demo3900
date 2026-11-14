using System;
using UnityEngine;
using UnityEngine.Events;

public enum SettingType
{
    Slider,
    Toggle
}

[Serializable]
public class Setting
{
    [Header("Information")]
    public string settingID;
    public string settingName;
    public SettingType type;

    [Header("Slider")]
    public float defaultSliderValue = 1f;
    public float minValue = 0f;
    public float maxValue = 1f;

    [Header("Toggle")]
    public bool defaultToggleValue;

    [Header("Actions")]
    public UnityEvent<float> onSliderChanged = new UnityEvent<float>();
    public UnityEvent<bool> onToggleChanged = new UnityEvent<bool>();
}