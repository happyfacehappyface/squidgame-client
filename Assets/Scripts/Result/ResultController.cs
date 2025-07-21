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
            _title.text = "축하합니다!";
        }
        else
        {
            _title.text = "아쉽네요!";
        }

        _winnerPlayer.ManualStart(_winnerIndex);
        _winnerName.text = _inGameController.PlayerNames[_winnerIndex];

        _animator.SetTrigger("Show");
    }

    public void OnClickReturnToOutGameButton()
    {
        SceneManager.LoadScene("OutGameScene");
    }
}
