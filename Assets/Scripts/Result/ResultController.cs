using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private PlayerComponent _winnerPlayer;
    [SerializeField] private TextMeshProUGUI _winnerName;
    [SerializeField] private Animator _animator;
    

    private InGameController _inGameController;
    private int _winnerIndex;

    public void ManualStart(InGameController inGameController, ResponsePacketData.GameEnded data)
    {
        _inGameController = inGameController;
        _winnerIndex = data.winnerPlayerIndex;

        UpdateResult();
    }

    public void UpdateResult()
    {

        if (_winnerIndex == _inGameController.MyIndex)
        {
            SoundManager.Instance.PlaySfxGameWin(0.0f);
            _title.text = "축하합니다!";
        }
        else
        {
            if (_winnerIndex == -1)
            {
                SoundManager.Instance.PlaySfxGameDraw(0.0f);
            }
            else
            {
                SoundManager.Instance.PlaySfxGameLose(0.0f);
            }

            _title.text = "아쉽네요!";
        }

        if (_winnerIndex == -1)
        {
            _winnerPlayer.gameObject.SetActive(false);
            _winnerName.text = "승자 없음";
        }
        else
        {
            _winnerPlayer.ManualStart(_winnerIndex, true);
            _winnerName.text = _inGameController.PlayerNames[_winnerIndex];
        }

        

        _animator.SetTrigger("Show");
    }

    public void OnClickReturnToOutGameButton()
    {
        SoundManager.Instance.PlaySfxButtonClick(0.0f);
        _inGameController.DestroyInGameController();
        SceneManager.LoadScene("OutGameScene");
    }
}
