using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SettingsData", menuName = "Game/Settings Data")]
public class SettingsData : ScriptableObject
{
    public List<Setting> settings = new List<Setting>();
}