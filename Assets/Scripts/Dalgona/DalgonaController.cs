using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DalgonaController : MonoBehaviour, ISubGameController
{

    private ResponsePacketData.DalgonaGameStarted _dalgonaGameData;
    public void ManualStart(ResponsePacketData.DalgonaGameStarted data)
    {
        _dalgonaGameData = data;
        Debug.Log("Dalgona Game: ManualStart");
    }

    public void OnSubGameStarted(InGameController inGameController)
    {
        Debug.Log("Dalgona Game: OnSubGameStarted");
    }

    public void ManualUpdate()
    {
        Debug.Log("Dalgona Game: ManualUpdate");
    }
}
