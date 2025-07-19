using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class OutGameController : MonoBehaviour
{

    [SerializeField] private GameObject _tabTitle;
    [SerializeField] private GameObject _tabInRoom;
    [SerializeField] private GameObject _waitForServer;
    [SerializeField] private TextMeshProUGUI _playerCountText;

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
        RequestEnterRoom();
    }

    public void OnClickLeaveRoom()
    {
        RequestLeaveRoom();
    }

    private void ManualStart()
    {
        OpenTabTitle();
    }

    private void CloseAllTab()
    {
        _tabTitle.SetActive(false);
        _tabInRoom.SetActive(false);
    }

    private void OpenTabTitle()
    {
        CloseAllTab();
        _tabTitle.SetActive(true);
    }

    private void OpenTabInRoom()
    {
        CloseAllTab();
        _tabInRoom.SetActive(true);
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

    public void OnResponseEnterRoom(bool isSuccess, ResponsePacketData.EnterRoom data)
    {
        if (isSuccess)
        {
            OpenTabInRoom();
        }
        else
        {
            Debug.Log("방 입장 실패");
        }

        _waitForServer.SetActive(false);
    }

    public void OnResponseLeaveRoom(bool isSuccess, ResponsePacketData.LeaveRoom data)
    {
        if (isSuccess)
        {
            OpenTabTitle();
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
            _playerCountText.text = data.playerCount.ToString();
        }
    }
}
