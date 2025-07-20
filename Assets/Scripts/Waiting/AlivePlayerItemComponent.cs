using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AlivePlayerItemComponent : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _playerNameText;

    public void ManualStart(string playerName)
    {
        _playerNameText.text = playerName;
    }
}
