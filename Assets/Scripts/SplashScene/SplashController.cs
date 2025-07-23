using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashController : MonoBehaviour
{

    void Start()
    {
        StartCoroutine(CO_OpenInGameScene());

        if (SoundManager.Instance.IsReady())
        {
            SoundManager.Instance.PlaySfxSplash(0f);
            SoundManager.Instance.PlayVoiceIntro(0.8f);
        }

    }

    private IEnumerator CO_OpenInGameScene()
    {

        yield return new WaitForSeconds(2.5f);

        while (true)
        {
            bool isNetworkManagerReady = NetworkManager.Instance.IsReady();
            bool isAssetManagerReady = AssetManager.Instance.IsReady();
            bool isSoundManagerReady = SoundManager.Instance.IsReady();

            if (isNetworkManagerReady && isAssetManagerReady && isSoundManagerReady)
            {
                SceneManager.LoadScene("OutGameScene");
                break;
            }

            yield return null;
        }

    }
}
