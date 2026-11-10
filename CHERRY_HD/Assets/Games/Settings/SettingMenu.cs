using System.Collections.Generic;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField]
    private List<Setting> settings = new List<Setting>();

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

        // Generate each setting
        foreach (Setting setting in settings)
        {
            GameObject prefab = GetPrefab(setting.type);

            if (prefab == null)
            {
                Debug.LogWarning("Missing prefab for: " + setting.settingName);
                continue;
            }

            GameObject instance = Instantiate(prefab, settingsContainer);

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