using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaitingController : MonoBehaviour
{

    private InGameController _inGameController;

    [SerializeField] private PlayerComponent _playerComponent;
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private TextMeshProUGUI _alivePlayerCountText;


    public void ManualStart(InGameController inGameController, ResponsePacketData.StartGame data)
    {
        _inGameController = inGameController;

        UpdateWaitingScene(data);

        StartCoroutine(CO_SendReadyGame());
        
    }

    private IEnumerator CO_SendReadyGame()
    {
        SoundManager.Instance.PlaySfxGameStart(0.0f);
        yield return new WaitForSeconds(2.0f);
        NetworkManager.Instance.SendMessageToServer(new RequestPacketData.ReadyGame());
    }


    private void UpdateWaitingScene(ResponsePacketData.StartGame data)
    {
        _playerComponent.ManualStart(data.myIndex, true);
        _playerNameText.text = data.names[data.myIndex];
        _alivePlayerCountText.text = data.names.Length.ToString();
    }





}
