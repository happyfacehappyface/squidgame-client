using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TogOfWarPlayerComponent : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _numberText;
    [SerializeField] private GameObject _isMeIndicator;
    [SerializeField] private Animator _animator;

    [SerializeField] private Image _bodyColor;


    private void ManualStart(int playerIndex, string playerName, bool isMe)
    {
        _isMeIndicator.SetActive(isMe);
        _nameText.text = playerName;
        _bodyColor.color = AssetManager.Instance.GetBodyColorFromPlayerIndex(playerIndex);
        _numberText.text = (playerIndex + 1).ToString();
    }

    private void OnCountDownStart()
    {
        _animator.SetTrigger("Shake");
    }

    private void OnLose()
    {
        _animator.SetTrigger("Flying");
    }
}
