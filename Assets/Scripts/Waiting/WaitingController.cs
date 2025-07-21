using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingController : MonoBehaviour, ISubGameController
{

    private InGameController _inGameController;


    [SerializeField] private GameObject _playerResultItemPrefab;
    [SerializeField] private Transform _playerResultItemParent;


    public void ManualStart(InGameController inGameController)
    {
        _inGameController = inGameController;
    }

    public void OnSubGameStarted()
    {
        
    }

    public void ManualUpdate()
    {

    }

    public void OnShowSubGameResult(ResponsePacketData.SubGameEnded data)
    {
        Utils.Log("Waiting Controller: OnShowSubGameResult");
        UpdatePlayerResultItems(data.survivePlayerIndices);
    }

    private void UpdatePlayerResultItems(int[] alivePlayerIndices)
    {
        ClearPlayerResultItems();
        
        for (var i = 0; i < alivePlayerIndices.Length; i++)
        {
            GameObject playerResultItem = Instantiate(_playerResultItemPrefab, _playerResultItemParent);
            playerResultItem.GetComponent<AlivePlayerItemComponent>().ManualStart(_inGameController.PlayerNames[alivePlayerIndices[i]]);
        }
    }

    private void ClearPlayerResultItems()
    {
        foreach (Transform child in _playerResultItemParent)
        {
            Destroy(child.gameObject);
        }
    }


}
