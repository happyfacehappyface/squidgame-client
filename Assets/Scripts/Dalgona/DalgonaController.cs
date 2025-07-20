using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DalgonaController : MonoBehaviour, ISubGameController
{

    private ResponsePacketData.DalgonaGameStarted _dalgonaGameData;

    private bool _isStarted = false;

    [SerializeField] private GameObject _preGameBarrier;
    [SerializeField] private TextMeshProUGUI _recentResultText;

    public void ManualStart(ResponsePacketData.DalgonaGameStarted data)
    {
        _isStarted = false;
        _preGameBarrier.SetActive(true);
        _dalgonaGameData = data;
        Debug.Log("Dalgona Game: ManualStart");

        NetworkManager.Instance.SendMessageToServer(new RequestPacketData.ReadySubGame());
    }

    public void OnSubGameStarted(InGameController inGameController)
    {
        Debug.Log("Dalgona Game: OnSubGameStarted");
        _isStarted = true;
        _preGameBarrier.SetActive(false);
    }

    public void ManualUpdate()
    {

    }

    public void OnResponseDalgonaGameResult(bool isSuccess, ResponsePacketData.DalgonaGameResult data)
    {
        if (isSuccess)
        {
            Debug.Log("Dalgona Game: OnResponseDalgonaGameResult");
            _recentResultText.text = data.isSuccess ? "Success" : "Fail";
        }
    }

    public void OnClickDalgonaResultTrue()
    {
        if (!_isStarted)
        {
            return;
        }
        NetworkManager.Instance.SendMessageToServer(new RequestPacketData.DalgonaGameResult(true));
    }

    public void OnClickDalgonaResultFalse()
    {
        if (!_isStarted)
        {
            return;
        }
        NetworkManager.Instance.SendMessageToServer(new RequestPacketData.DalgonaGameResult(false));
    }


}
