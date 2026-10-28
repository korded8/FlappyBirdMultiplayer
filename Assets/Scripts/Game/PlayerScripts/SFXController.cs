using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(AudioSource))]
public class SFXController : MonoBehaviour
{
    [SerializeField] private List<AudioClip> clipList = new();
    private AudioSource audioSource;
    private float defaultVolume = 0.5f;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = defaultVolume;
        audioSource.loop = false;
    }

    [PunRPC]
    public void PlaySFX_RPC(int index, float volume = 0f)
    {
        StartCoroutine(PlaySFX(index, volume));
    }
    
    public IEnumerator PlaySFX(int index, float volume = 0f)
    {
        Debug.Log(index);
        if (index < clipList.Count)
        {
            audioSource.clip = clipList[index];
            if (volume == 0f)
                audioSource.volume = defaultVolume;
            else
                audioSource.volume = volume;
            audioSource.Play();
        }

        yield return null;
    }
}
