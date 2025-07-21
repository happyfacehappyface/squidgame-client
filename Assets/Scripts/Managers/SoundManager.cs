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
    [SerializeField] private AudioClip[] _sfxDeath;
    [SerializeField] private AudioClip[] _sfxGameWin;
    [SerializeField] private AudioClip[] _sfxGameLose;
    [SerializeField] private AudioClip[] _sfxGameDraw;
    [SerializeField] private AudioClip[] _sfxGunShot;
    [SerializeField] private AudioClip[] _sfxDoor;
    

    
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
    public void PlaySfxDeath(float delay) => PlayRandomSfx(_sfxDeath, delay);
    public void PlaySfxGameWin(float delay) => PlayRandomSfx(_sfxGameWin, delay);
    public void PlaySfxGameLose(float delay) => PlayRandomSfx(_sfxGameLose, delay);
    public void PlaySfxGameDraw(float delay) => PlayRandomSfx(_sfxGameDraw, delay);
    public void PlaySfxGunShot(float delay) => PlayRandomSfx(_sfxGunShot, delay);
    public void PlaySfxDoor(float delay) => PlayRandomSfx(_sfxDoor, delay);


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
