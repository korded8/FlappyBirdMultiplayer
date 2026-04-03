using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MusicController : MonoBehaviour
{
    public AudioSource audioSource;
    public bool isClipLoop = false;
    [Tooltip("The number that corresponds to the AudioClip in the Clip List that should be looped.")]
    public int loopedClipIndex = -1;

    public List<AudioClip> clipList = new();
    private int index = 0;

    private void Awake()
    {
        audioSource.loop = false;
    }

    void Update()
    {
        StartCoroutine(PlayMusic());
    }

    IEnumerator PlayMusic()
    {
        if (!audioSource.isPlaying)
        {
            index++;
            if (index > clipList.Count - 1)
                index = 0;
            audioSource.clip = clipList[index];
            if (isClipLoop && index == loopedClipIndex)
                audioSource.loop = true;
            audioSource.Play();
        }

        yield return null;
    }
}