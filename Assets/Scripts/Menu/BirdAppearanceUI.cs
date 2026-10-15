using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ColorPickerController;

public class BirdAppearanceUI : MonoBehaviour
{
    [Header("Appearance Scripts")]
    [SerializeField] private BirdAppearanceData data;
    [SerializeField] private BirdAppearance birdAppearance;
     
    [Header("Presets")]
    [SerializeField] private TMP_Text deletingClassicPresetWarning;
    [SerializeField] private TMP_Text classicEditWarning;
    [SerializeField] private Button randomizeButton;
    [SerializeField] private float transitionDuration = 1.5f;

    [Header("Groups")]
    [SerializeField] private GameObject mainGroup;
    [SerializeField] private GameObject playGroup;
    [SerializeField] private GameObject customizeGroup;
    [SerializeField] private GameObject additionalInfoGroup;

    public int currentPreset = 0;
    public Action OnCurrentPresetChanged;

    [Header("Highscore text")]
    [SerializeField] private TMP_Text highscoreText;

    private void Start()
    {
        ShowWarning(null);

        playGroup.SetActive(false);

        int highscore = PlayerPrefs.GetInt("Highscore", 0);
        highscoreText.text = "Highscore: " + highscore.ToString();

        StartCoroutine(nameof(TransitionFirstColor));

        data.LoadPresets();
        currentPreset = data.currentPresetIndex;
        birdAppearance.ApplySkin(data.currentSkinIndex);

        OnCurrentPresetChanged?.Invoke();
        data.OnAppearanceChanged?.Invoke();
    }

    private IEnumerator TransitionFirstColor()
    {
        randomizeButton.GetComponent<Image>().color = RandomizeColor();
        yield return new WaitForSeconds(transitionDuration);
        StartCoroutine(nameof(CycleColors));
    }

    private IEnumerator CycleColors()
    {
        while (true)
        {
            Color startColor = randomizeButton.GetComponent<Image>().color;
            Color targetColor = RandomizeColor();

            float elapsed = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / transitionDuration);
                t = Mathf.SmoothStep(0f, 1f, t);

                randomizeButton.GetComponent<Image>().color = Color.Lerp(startColor, targetColor, t);

                yield return null;
            }
        }
    }

    public void RandomizeColors()
    {
        Randomize(BirdPart.body);
        Randomize(BirdPart.wings);
        Randomize(BirdPart.feather);
        OnCurrentPresetChanged?.Invoke();
    }

    private Color RandomizeColor()
    {
        float r = UnityEngine.Random.Range(0, 1f);
        float g = UnityEngine.Random.Range(0, 1f);
        float b = UnityEngine.Random.Range(0, 1f);

        Color newColor = new Color(r, g, b);
        return newColor;
    }

    public void Randomize(BirdPart birdPart)
    {
        Color newColor = RandomizeColor();
        data.SetColor(birdPart, newColor);
    }

    public void SelectSkin(int index)
    {
        data.currentSkinIndex = index;
        birdAppearance.ApplySkin(index);
    }

    public void AddPreset()
    {
        Color bodyColor = data.bodyColor;
        Color wingColor = data.wingColor;
        Color featherColor = data.featherColor;

        data.AddPreset(bodyColor, wingColor, featherColor);
        currentPreset = data.presets.Count - 1;
        data.ApplyPreset(currentPreset);

        OnCurrentPresetChanged?.Invoke();
        data.OnAppearanceChanged?.Invoke();
    }

    public void UpdatePreset()
    {
        if (currentPreset == 0)
        {
            //ShowWarning(classicEditWarning);
            return;
        }

        Color bodyColor = data.bodyColor;
        Color wingColor = data.wingColor;
        Color featherColor = data.featherColor;

        data.UpdatePreset(bodyColor, wingColor, featherColor, currentPreset);
        data.ApplyPreset(currentPreset);

        data.OnAppearanceChanged?.Invoke();
    }

    public void DeletePreset()
    {
        if (currentPreset == 0)
        {
            //ShowWarning(deletingClassicPresetWarning);
            return;
        }
        else
        {
            ShowWarning(null);
        }

        data.DeletePreset(currentPreset);
        currentPreset = 0;

        OnCurrentPresetChanged?.Invoke();
        data.OnAppearanceChanged?.Invoke();
    }

    public void SelectPreset(int index)
    {
        if (index < 0) return;

        ShowWarning(null);

        if (index > data.presets.Count - 1)
        {
            AddPreset();
            return;
        }

        currentPreset = index;
        data.ApplyPreset(currentPreset);
        birdAppearance.ApplyColors();

        OnCurrentPresetChanged?.Invoke();
        data.OnAppearanceChanged?.Invoke();
    }

    private void ShowWarning(TMP_Text warning)
    {
        //deletingClassicPresetWarning.enabled = false;
        //classicEditWarning.enabled = false;

        if (warning != null) warning.enabled = true;
    }

    public void TogglePlayGroup(bool show)
    {
        mainGroup.SetActive(!show);
        playGroup.SetActive(show);
    }

    public void ToggleCustomizeGroup(bool show)
    {
        mainGroup.SetActive(!show);
        customizeGroup.SetActive(show);
        additionalInfoGroup.SetActive(!show);
    }
}
