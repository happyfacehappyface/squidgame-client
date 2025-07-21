using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SurvivePlayerComponent : MonoBehaviour
{
    [SerializeField] private PlayerComponent _playerComponent;
    [SerializeField] private TextMeshProUGUI _playerName;
    [SerializeField] private GameObject _isMeIndicator;

    public void UpdateSurvivePlayer(int playerIndex, string playerName, bool isMe)
    {
        _playerComponent.ManualStart(playerIndex, true);
        _playerName.text = playerName;
        _isMeIndicator.SetActive(isMe);
    }



}
