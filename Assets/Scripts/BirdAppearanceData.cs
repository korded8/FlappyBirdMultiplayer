using UnityEngine;
using System;
using NUnit.Framework;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BirdAppearanceData", menuName = "Bird/BirdAppearanceData")]
public class BirdAppearanceData : ScriptableObject
{
    /*
    [Header("Sliders")]
    public Slider redSlider;
    public Slider greenSlider;
    public Slider blueSlider;*/

    [System.Serializable]
    public struct Preset
    {
        public Color bodyColor;
        public Color wingColor;
        public Color featherColor;
        public string presetName;
    }
    [Header("Current Colors")]
    public Color bodyColor = Color.white;
    public Color wingColor = Color.white;
    public Color featherColor = Color.white;

    [Header("Presets")]
    public List<Preset> presets = new List<Preset>();

    public Action OnAppearanceChanged;

    private void OnEnable()
    {
        bodyColor = Color.white;
        wingColor = Color.white;
        featherColor = Color.white;
    }

    public void ApplyPreset(int index)
    {

        if (index < 0 || index >= presets.Count) return;

        bodyColor = presets[index].bodyColor;
        wingColor = presets[index].wingColor;
        featherColor = presets[index].featherColor;

        OnAppearanceChanged?.Invoke();
    }

    public void AddPreset(Color body, Color wing, Color feather, string presetName)
    {
        Preset newPreset = new Preset
        {
            bodyColor = body,
            wingColor = wing,
            featherColor = feather,
            presetName = presetName
        };
        
        presets.Add(newPreset);
    }

    public void UpdatePreset(Color body, Color wing, Color feather, int index)
    {
        Preset p = presets[index];
        p.bodyColor = body;
        p.wingColor = wing;
        p.featherColor = featherColor;
        presets[index] = p;
    }

    public void SetColor(BirdAppearanceUI.BirdPart part, Color color, int presetIndex)
    {
        switch (part)
        {
            case BirdAppearanceUI.BirdPart.body: bodyColor = color; break;
            case BirdAppearanceUI.BirdPart.wings: wingColor = color; break;
            case BirdAppearanceUI.BirdPart.feather: featherColor = color; break;
        }

        if (presetIndex >= 0 && presetIndex < presets.Count)
        {
            Preset p = presets[presetIndex];
            switch (part)
            {
                case BirdAppearanceUI.BirdPart.body: p.bodyColor = color; break;
                case BirdAppearanceUI.BirdPart.wings: p.wingColor = color; break;
                case BirdAppearanceUI.BirdPart.feather: p.featherColor = color; break;
            }
        }

        OnAppearanceChanged?.Invoke();
    }
    public Color GetColor(BirdAppearanceUI.BirdPart part)
    {
        switch (part)
        {
            case BirdAppearanceUI.BirdPart.body: return bodyColor;
            case BirdAppearanceUI.BirdPart.wings: return wingColor;
            case BirdAppearanceUI.BirdPart.feather: return featherColor;
            default: return Color.white;
        }
    }

    public void Randomize()
    {
        bodyColor = new Color(UnityEngine.Random.value, 0, 0, 1);
        wingColor = new Color(0, UnityEngine.Random.value, 0, 1);
        featherColor = new Color(0, 0, UnityEngine.Random.value, 1);

        OnAppearanceChanged?.Invoke();
    }
}