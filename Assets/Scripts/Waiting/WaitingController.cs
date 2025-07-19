using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingController : MonoBehaviour
{
    [SerializeField] private GameObject _gameStarted;

    protected void Start()
    {
        ManualStart();
    }

    private void ManualStart()
    {
        _gameStarted.SetActive(false);
        NetworkManager.Instance.SendMessageToServer(new RequestPacketData.ReadyGame());
    }

    public void OnResponseReadyGame(bool isSuccess, ResponsePacketData.ReadyGame data)
    {
        if (isSuccess)
        {
            _gameStarted.SetActive(true);
        }
    }


}
