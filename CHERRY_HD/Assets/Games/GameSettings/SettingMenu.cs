using System.Collections.Generic;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField]
    private SettingsData settingsData;

    [Header("UI References")]
    [SerializeField]
    private Transform settingsContainer;

    [Header("Setting Prefabs")]
    [SerializeField]
    private GameObject sliderPrefab;

    [SerializeField]
    private GameObject togglePrefab;

    private void Start()
    {
        GenerateSettings();
    }

    private void GenerateSettings()
    {

        // Generate each setting from the settings data
        foreach (Setting setting in settingsData.settings)
        {
            GameObject prefab = GetPrefab(setting.type);

            if (prefab == null)
            {
                Debug.LogWarning("Missing prefab for: " + setting.settingName);
                continue;
            }
            
            //Create an instance of the prefab with and add it to the settings container
            GameObject instance = Instantiate(prefab, settingsContainer);
            
            // Get the SettingUI component from the instantiated prefab and set it up with the current setting
            SettingUI settingUI = instance.GetComponent<SettingUI>();
            if (settingUI != null)
            {
                settingUI.Setup(setting);
            }
            else
            {
                Debug.LogWarning("SettingUI component missing on: " + prefab.name);
            }
        }
    }

    private GameObject GetPrefab(SettingType type)
    {
        switch (type)
        {
            case SettingType.Slider:
                return sliderPrefab;

            case SettingType.Toggle:
                return togglePrefab;

            default:
                return null;
        }
    }
}