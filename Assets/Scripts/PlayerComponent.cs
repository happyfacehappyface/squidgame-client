using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerComponent : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _numberText;
    [SerializeField] private Image _playerBodyImage;

    [SerializeField] private Transform _rootTransform;
    [SerializeField] private Transform _numberTransform;

    

    public void ManualStart(int playerIndex, bool isLeft)
    {
        _playerBodyImage.color = AssetManager.Instance.GetBodyColorFromPlayerIndex(playerIndex);
        _numberText.text = (playerIndex + 1).ToString();

        if (isLeft)
        {
            _rootTransform.localScale = new Vector3(1, 1, 1);
            _numberTransform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            _rootTransform.localScale = new Vector3(-1, 1, 1);
            _numberTransform.localScale = new Vector3(-1, 1, 1);
        }
    }
}
