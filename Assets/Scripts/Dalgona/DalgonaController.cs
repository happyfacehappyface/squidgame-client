using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class DalgonaController : MonoBehaviour, ISubGameController
{

    [Header("UI & Game Control")]
    [SerializeField] private GameObject _preGameBarrier;
    [SerializeField] private GameObject _shapeSelectionUI;
    [SerializeField] private GameObject _dalgonaGameplay;
    [SerializeField] private StrokePainter _strokePainter;

    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI _timerText;
    private TimeSpan _currentTime;
    private bool _isTimerRunning = false;

    private ResponsePacketData.DalgonaGameStarted _dalgonaGameData;




    public void ManualStart(ResponsePacketData.DalgonaGameStarted data)
    {
        _isTimerRunning = false;

        _preGameBarrier.SetActive(true);

        _dalgonaGameData = data;

        _currentTime = TimeSpan.FromMilliseconds(data.timeLimitMs);
        Debug.Log("Dalgona Game: ManualStart");

        _dalgonaGameplay.SetActive(false);

        NetworkManager.Instance.SendMessageToServer(new RequestPacketData.ReadySubGame());
    }

    public void OnSubGameStarted()
    {
        Debug.Log("Dalgona Game: OnSubGameStarted - Shape Selection Starts!");
        _preGameBarrier.SetActive(false);
        
        _isTimerRunning = true;
    }

    public void ManualUpdate()
    {
        if (_isTimerRunning)
        {
            _currentTime -= TimeSpan.FromSeconds(Time.deltaTime);
            UpdateTimerUI();
        }
    }
    
    private void UpdateTimerUI()
    {
        _timerText.text = $"{Mathf.Max(0, (float) _currentTime.TotalSeconds):F2}";
    }

    // Called by ShapeSelectionController
    public void OnShapeSelected(StrokePainter.DalgonaShapeType shape, int difficulty)
    {
        Debug.Log($"모양({shape}, 난이도:{difficulty}) 선택됨. 게임플레이를 시작합니다.");

        _shapeSelectionUI.SetActive(false);
        
        if(_strokePainter != null)
        {
            _strokePainter.shapeToGenerate = shape;
            _strokePainter.difficultyLevel = difficulty;
        }

        _dalgonaGameplay.SetActive(true);

    }


    public void OnResponseDalgonaGameResult(bool isSuccess, ResponsePacketData.DalgonaGameResult data)
    {
        if (isSuccess)
        {
            Debug.Log("Dalgona Game: OnResponseDalgonaGameResult");
            // This is just a confirmation from server, actual result is handled by OnClick... methods
        }
    }

    public void OnSuccess()
    {
        Debug.Log("Dalgona Game: OnSuccess");
        NetworkManager.Instance.SendMessageToServer(new RequestPacketData.DalgonaGameResult(true));
    }

    public void OnFail()
    {
        Debug.Log("Dalgona Game: OnFail");
        NetworkManager.Instance.SendMessageToServer(new RequestPacketData.DalgonaGameResult(false));
    }

}
