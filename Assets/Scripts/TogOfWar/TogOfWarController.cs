using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TogOfWarController : MonoBehaviour, ISubGameController
{
    
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _leftPlayerParent;
    [SerializeField] private GameObject _rightPlayerParent;

    private InGameController _inGameController;

    public void OnSubGameStarted(InGameController inGameController)
    {
        _inGameController = inGameController;
    }

    public void ManualUpdate()
    {

    }


}
