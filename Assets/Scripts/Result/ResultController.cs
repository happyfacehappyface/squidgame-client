using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultController : MonoBehaviour
{
    [SerializeField] private PlayerComponent _winnerPlayer;
    [SerializeField] private TextMeshProUGUI _winnerName;
    

    private InGameController _inGameController;
    private int _winnerIndex;

    public void ManualStart(InGameController inGameController, int winnerIndex)
    {
        _inGameController = inGameController;
        _winnerIndex = winnerIndex;
    }

    public void UpdateResult()
    {

    }
}
