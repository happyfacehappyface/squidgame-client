using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InGameDrawer : MonoBehaviour
{
    [SerializeField] private Animator _canvasAnimator;
    [SerializeField] private SurvivePlayerComponent _survivePlayerItemPrefab;
    [SerializeField] private Transform _survivePlayerItemParent;

    [SerializeField] private Animator _deathIndicatorAnimator;


    private InGameController _inGameController;

    public void ManualStart(InGameController inGameController)
    {
        _inGameController = inGameController;
    }

    public void OnSubGameEnded(ResponsePacketData.SubGameEnded data)
    {
        UpdateSurvivePlayers(data);
        _canvasAnimator.SetBool("Open", true);
    }

    public void OnNewSubGameSceneLoaded()
    {
        _canvasAnimator.SetBool("Open", false);
    }

    private void UpdateSurvivePlayers(ResponsePacketData.SubGameEnded data)
    {
        ClearSurvivePlayers();

        for (var i = 0; i < data.survivePlayerIndices.Length; i++)
        {
            SurvivePlayerComponent survivePlayerItem = Instantiate(_survivePlayerItemPrefab, _survivePlayerItemParent);
            survivePlayerItem.UpdateSurvivePlayer(data.survivePlayerIndices[i], _inGameController.PlayerNames[data.survivePlayerIndices[i]], data.survivePlayerIndices[i] == _inGameController.MyIndex);
        }

        
        _deathIndicatorAnimator.SetBool("Lose", !data.survivePlayerIndices.Contains(_inGameController.MyIndex));
    }

    private void ClearSurvivePlayers()
    {
        foreach (Transform child in _survivePlayerItemParent)
        {
            Destroy(child.gameObject);
        }
    }

    public void SetDeathIndicatorTrue()
    {
        _deathIndicatorAnimator.SetBool("Lose", true);
    }
}
