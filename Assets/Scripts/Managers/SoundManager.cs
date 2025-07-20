using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _voiceSource;

    [SerializeField] private AudioClip[] _sfxButtonClick;
    

    
    private bool _isReady = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        _isReady = true;
    }

    public bool IsReady()
    {
        return _isReady;
    }


    public void PlaySfxButtonClick(float delay) => PlayRandomSfx(_sfxButtonClick, delay);

    private void PlayRandomSfx(AudioClip[] clips, float delay)
    {
        if (clips != null && clips.Length > 0)
        {
            int randomIndex = Random.Range(0, clips.Length);
            if (delay <= 0f)
            {
                PlaySfx(clips[randomIndex]);
            }
            else
            {
                StartCoroutine(CO_PlaySfxWithDelay(clips[randomIndex], delay));
            }
        }
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip != null)
        {
            _sfxSource.PlayOneShot(clip);
        }
    }

    private IEnumerator CO_PlaySfxWithDelay(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlaySfx(clip);
    }
}
