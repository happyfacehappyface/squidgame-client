using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TugOfWarPlayerComponent : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private GameObject _isMeIndicator;
    [SerializeField] private Animator _animator;

    [SerializeField] private PlayerComponent _playerComponent;

    public void ManualStart(int playerIndex, string playerName, bool isMe, bool isLeft)
    {
        _isMeIndicator.SetActive(isMe);
        _nameText.text = playerName;
        _playerComponent.ManualStart(playerIndex, isLeft);
    }

    public void OnCountDownStart()
    {
        _animator.SetTrigger("Shake");
    }

    public void OnLose()
    {
        _animator.SetTrigger("Flying");
    }

    public void OnWin()
    {
        _animator.SetTrigger("Idle");
    }
}
