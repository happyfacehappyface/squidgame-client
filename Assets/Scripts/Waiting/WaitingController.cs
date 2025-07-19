using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingController : MonoBehaviour, ISubGameController
{


    private InGameController _inGameController;


    public void OnSubGameStarted(InGameController inGameController)
    {
        _inGameController = inGameController;
    }

    public void ManualUpdate()
    {

    }

    


}
