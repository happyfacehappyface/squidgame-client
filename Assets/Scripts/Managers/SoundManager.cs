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

    [SerializeField] private AudioClip[] _sfxSplash;

    [SerializeField] private AudioClip[] _sfxLaser;
    [SerializeField] private AudioClip[] _sfxSpaceShip;
    [SerializeField] private AudioClip[] _sfxGameStart;

    [SerializeField] private AudioClip[] _sfxWind;
    [SerializeField] private AudioClip[] _sfxRope;
    [SerializeField] private AudioClip[] _sfxBurn;
    [SerializeField] private AudioClip[] _sfxSlash;

    [SerializeField] private AudioClip[] _sfxSubGameStart;
    [SerializeField] private AudioClip[] _sfxAlert;


    [SerializeField] private AudioClip[] _bgmOutGame;

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
    public void PlaySfxSplash(float delay) => PlayRandomSfx(_sfxSplash, delay);
    public void PlaySfxLaser(float delay) => PlayRandomSfx(_sfxLaser, delay);
    public void PlaySfxSpaceShip(float delay) => PlayRandomSfx(_sfxSpaceShip, delay);
    public void PlaySfxGameStart(float delay) => PlayRandomSfx(_sfxGameStart, delay);
    public void PlaySfxWind(float delay) => PlayRandomSfx(_sfxWind, delay);
    public void PlaySfxRope(float delay) => PlayRandomSfx(_sfxRope, delay);
    public void PlaySfxBurn(float delay) => PlayRandomSfx(_sfxBurn, delay);
    public void PlaySfxSlash(float delay) => PlayRandomSfx(_sfxSlash, delay);
    public void PlaySfxSubGameStart(float delay) => PlayRandomSfx(_sfxSubGameStart, delay);
    public void PlaySfxAlert(float delay) => PlayRandomSfx(_sfxAlert, delay);

    public void PlayBgmOutGame(bool loop = true) => PlayRandomBgm(_bgmOutGame, loop);


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


    public void PlayBgm(AudioClip clip, bool loop = true)
    {
        _bgmSource.clip = clip;
        _bgmSource.loop = loop;
        _bgmSource.Play();
    }

    public void PlayRandomBgm(AudioClip[] clips, bool loop = true)
    {
        if (clips != null && clips.Length > 0)
        {
            int randomIndex = Random.Range(0, clips.Length);
            PlayBgm(clips[randomIndex], loop);
        }
    }

    public void StopBgm()
    {
        _bgmSource.Stop();
    }
}
