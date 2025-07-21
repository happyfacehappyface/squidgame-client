using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerComponent : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _numberText;
    [SerializeField] private Image _playerBodyImage;

    

    public void ManualStart(int playerIndex)
    {
        _playerBodyImage.color = AssetManager.Instance.GetBodyColorFromPlayerIndex(playerIndex);
        _numberText.text = (playerIndex + 1).ToString();
    }
}
