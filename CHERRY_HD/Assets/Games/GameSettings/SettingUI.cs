using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingUI : MonoBehaviour
{
    [Header("Common UI")]
    [SerializeField] private TMP_Text settingNameText;
    [SerializeField] private TMP_Text valueText;

    [Header("Slider UI")]
    [SerializeField] private Slider slider;

    [Header("Toggle UI")]
    [SerializeField] private Toggle toggle;


    private Setting currentSetting;

    public void Setup(Setting setting)
    {
        currentSetting = setting;

        settingNameText.text = setting.settingName;

        switch (setting.type)
        {
            case SettingType.Slider:
                SetupSlider(setting);
                break;

            case SettingType.Toggle:
                SetupToggle(setting);
                break;
        }
    }

    private void SetupSlider(Setting setting)
    {
        slider.gameObject.SetActive(true);

        slider.minValue = setting.minValue;
        slider.maxValue = setting.maxValue;

        float value = PlayerPrefs.GetFloat(
            setting.settingID,
            setting.defaultSliderValue
        );

        value = Mathf.Clamp(value, setting.minValue, setting.maxValue);

        slider.SetValueWithoutNotify(value);

        UpdateValueText(value);

        slider.onValueChanged.AddListener(OnSliderChanged);

        setting.onSliderChanged?.Invoke(value);
    }

    private void SetupToggle(Setting setting)
    {
        toggle.gameObject.SetActive(true);

        bool value = PlayerPrefs.GetInt(
            setting.settingID,
            setting.defaultToggleValue ? 1 : 0
        ) == 1;

        toggle.SetIsOnWithoutNotify(value);

        UpdateValueText(value);

        toggle.onValueChanged.AddListener(OnToggleChanged);

        setting.onToggleChanged?.Invoke(value);
    }

    private void OnSliderChanged(float value)
    {
        PlayerPrefs.SetFloat(currentSetting.settingID, value);

        UpdateValueText(value);

        currentSetting.onSliderChanged?.Invoke(value);
    }

    private void OnToggleChanged(bool value)
    {
        PlayerPrefs.SetInt(
            currentSetting.settingID,
            value ? 1 : 0
        );

        UpdateValueText(value);

        currentSetting.onToggleChanged?.Invoke(value);
    }

    private void UpdateValueText(float value)
    {
        if (valueText != null)
            valueText.text = value.ToString("0.0");
    }

    private void UpdateValueText(bool value)
    {
        if (valueText != null)
            valueText.text = value ? "On" : "Off";
    }


    private void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderChanged);

        if (toggle != null)
            toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }
}