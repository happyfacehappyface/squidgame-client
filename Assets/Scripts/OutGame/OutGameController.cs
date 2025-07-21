using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OutGameController : MonoBehaviour
{
    private ResponsePacketData.StartGame _startGameData;

    [SerializeField] private OutGamePopupHandler _popupHandler;

    [SerializeField] private GameObject _inGameControllerPrefab;
    [SerializeField] private GameObject _waitForServer;
    [SerializeField] private TextMeshProUGUI _playerCountText;

    [SerializeField] private Animator _canvasAnimator;

    // Start is called before the first frame update
    void Start()
    {
        ManualStart();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClickEnterRoom()
    {
        SoundManager.Instance.PlaySfxButtonClick(0.0f);
        RequestEnterRoom();
    }

    public void OnClickLeaveRoom()
    {
        SoundManager.Instance.PlaySfxButtonClick(0.0f);
        RequestLeaveRoom();
    }

    public void OnClickStartGame()
    {
        SoundManager.Instance.PlaySfxButtonClick(0.0f);
        RequestStartGame();
    }

    private void ManualStart()
    {
        _popupHandler.ManualStart(this);
        
    }

    public void RequestEnterRoom()
    {
        _waitForServer.SetActive(true);
        NetworkManager.Instance.SendMessageToServer(new RequestPacketData.EnterRoom());
    }

    public void RequestLeaveRoom()
    {
        _waitForServer.SetActive(true);
        NetworkManager.Instance.SendMessageToServer(new RequestPacketData.LeaveRoom());
    }

    public void RequestStartGame()
    {
        _waitForServer.SetActive(true);
        NetworkManager.Instance.SendMessageToServer(new RequestPacketData.StartGame());
    }


    public void OnResponseEnterRoom(bool isSuccess, ResponsePacketData.EnterRoom data)
    {
        if (isSuccess)
        {
            _canvasAnimator.SetTrigger("InRoom");
        }
        else
        {
            _popupHandler.OpenErrorPopup("문제 발생!", "게임이 이미 시작되었습니다.\n 다음 게임을 기다려주세요!");
        }

        _waitForServer.SetActive(false);
    }

    public void OnResponseLeaveRoom(bool isSuccess, ResponsePacketData.LeaveRoom data)
    {
        if (isSuccess)
        {
            _canvasAnimator.SetTrigger("Title");
        }
        else
        {
            Debug.Log("방 퇴장 실패");
        }

        _waitForServer.SetActive(false);
    }

    public void OnResponsePlayerCountChanged(bool isSuccess, ResponsePacketData.PlayerCountChanged data)
    {
        if (isSuccess)
        {
            _playerCountText.text = $"참가자 수: {data.playerCount}";
        }
    }

    public void OnResponseStartGame(bool isSuccess, ResponsePacketData.StartGame data)
    {
        if (isSuccess)
        {
            _startGameData = data;
            
            StartCoroutine(CO_OpenInGameScene());
        }
    }

    private IEnumerator CO_OpenInGameScene()
    {
        _canvasAnimator.SetTrigger("InGame");
        yield return new WaitForSeconds(1.0f);

        SceneManager.sceneLoaded += OnInGameSceneLoaded;
        SceneManager.LoadScene("WaitingScene");
    }

    private void OnInGameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnInGameSceneLoaded;
        InGameController controller = Instantiate(_inGameControllerPrefab).GetComponent<InGameController>();
        controller.ManualStart(_startGameData);
    }
}
