using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MusicController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private bool isClipLoop = false;
    [SerializeField] private GameController gc;
    [SerializeField] private List<AudioClip> clipList = new();
    [Tooltip("The numbers that correspond to the indexes of AudioClips in the Clip List that should be looped.")]
    [SerializeField] private List<int> loopedClipIndexList = new();
    private int index = 0;
    private float transitionTime = 2f;
    private float defaultVolume = 0.8f;
    private float currentVolume = 0f;
    private bool musicIsChanged = false;

    private void Awake()
    {
        audioSource.loop = false;
        audioSource.volume = 0f;
        currentVolume = audioSource.volume;
    }

    void Update()
    {
        if (gc == null)
        {
            if (!audioSource.isPlaying)
            {
                StartCoroutine(PlayMusic());
                StartCoroutine(StartMusic());
            }
            return;
        }

        if (gc.IsGameStarted())
        {
            if (!audioSource.isPlaying)
            {
                StartCoroutine(PlayMusic());
                StartCoroutine(StartMusic());
            }
        }

        if (gc.currDiffLvl == GameController.DifficultyLevel.Medium && !musicIsChanged)
        {
            musicIsChanged = true;
            StartCoroutine(EndMusic());
            index++;
        }
    }

    IEnumerator PlayMusic()
    {
        if (index > clipList.Count - 1)
            index = 0;
        audioSource.clip = clipList[index];
        if (isClipLoop)
        {
            foreach (int c in loopedClipIndexList)
            {
                if (index == c)
                {
                    audioSource.loop = true;
                    break;
                }
                else
                    audioSource.loop = false;
            }
        }
        audioSource.Play();
        index++;

        yield return null;
    }

    IEnumerator EndMusic()
    {
        float elapsed = 0f;

        while (elapsed < transitionTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionTime;
            audioSource.volume = Mathf.Lerp(currentVolume, 0, t);
            yield return null;
        }

        audioSource.volume = 0f;
        currentVolume = audioSource.volume;
        audioSource.Stop();
    }

    IEnumerator StartMusic()
    {
        float elapsed = 0f;

        while (elapsed < transitionTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionTime;
            audioSource.volume = Mathf.Lerp(currentVolume, defaultVolume, t);
            yield return null;
        }

        audioSource.volume = defaultVolume;
        currentVolume = audioSource.volume;
    }
}