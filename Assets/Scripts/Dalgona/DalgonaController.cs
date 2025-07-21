using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DalgonaController : MonoBehaviour, ISubGameController
{

    private bool _isStarted = false;

    [Header("UI & Game Control")]
    [SerializeField] private GameObject _preGameBarrier;
    [SerializeField] private GameObject _shapeSelectionUI;
    [SerializeField] private GameObject _dalgonaGameplay;
    [SerializeField] private StrokePainter _strokePainter;

    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private float _gameDuration = 35.0f;
    private float _currentTime;
    private bool _isTimerRunning = false;

    private ResponsePacketData.DalgonaGameStarted _dalgonaGameData;

    private void Awake()
    {
        // Ensure a clean state when the scene loads, before any game logic starts.
        // This prevents objects from being visible if they were left active in the editor.
        if (_shapeSelectionUI != null) _shapeSelectionUI.SetActive(false);
        if (_dalgonaGameplay != null) _dalgonaGameplay.SetActive(false);
        if (_timerText != null) _timerText.gameObject.SetActive(false);
    }

    void Update()
    {
        // =================================================================
        //  TESTING 목적으로 추가된 코드 (나중에 반드시 제거해야 함)
        // =================================================================
        // 1. 게임 시작 트리거
        if (!_isStarted && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.LogWarning("!!! 테스트: 스페이스바를 눌러 강제로 게임을 시작합니다. !!!");
            OnSubGameStarted();
        }

        // 2. InGameController의 ManualUpdate 역할 대행
        if (_isStarted)
        {
            ManualUpdate();
        }
        // =================================================================
    }


    public void ManualStart(ResponsePacketData.DalgonaGameStarted data)
    {
        _isStarted = false;
        _isTimerRunning = false;

        if(_preGameBarrier != null) _preGameBarrier.SetActive(true);
        if(_shapeSelectionUI != null) _shapeSelectionUI.SetActive(false);
        if(_dalgonaGameplay != null) _dalgonaGameplay.SetActive(false);
        if(_timerText != null) _timerText.gameObject.SetActive(false);

        _dalgonaGameData = data;
        Debug.Log("Dalgona Game: ManualStart");

        NetworkManager.Instance.SendMessageToServer(new RequestPacketData.ReadySubGame());
    }

    public void OnSubGameStarted()
    {
        Debug.Log("Dalgona Game: OnSubGameStarted - Shape Selection Starts!");
        _isStarted = true;
        if(_preGameBarrier != null) _preGameBarrier.SetActive(false);

        // Show Shape Selection UI
        if(_shapeSelectionUI != null) _shapeSelectionUI.SetActive(true);
        if(_dalgonaGameplay != null) _dalgonaGameplay.SetActive(false);

        // Ensure timer is hidden during shape selection
        if(_timerText != null) 
        {
            _timerText.gameObject.SetActive(false);
        }
        _isTimerRunning = false;
    }

    public void ManualUpdate()
    {
        if (_isTimerRunning)
        {
            _currentTime -= Time.deltaTime;
            UpdateTimerUI();

            if (_currentTime <= 0)
            {
                _currentTime = 0;
                _isTimerRunning = false;
                Debug.Log("시간 초과! 실패 처리합니다.");
                OnClickDalgonaResultFalse();
            }
        }
    }
    
    private void UpdateTimerUI()
    {
        if (_timerText != null)
        {
            _timerText.text = $"{_currentTime:F2}";
        }
    }

    // Called by ShapeSelectionController
    public void OnShapeSelected(StrokePainter.DalgonaShapeType shape, int difficulty)
    {
        Debug.Log($"모양({shape}, 난이도:{difficulty}) 선택됨. 게임플레이를 시작합니다.");

        if(_shapeSelectionUI != null) _shapeSelectionUI.SetActive(false);
        
        if(_strokePainter != null)
        {
            _strokePainter.shapeToGenerate = shape;
            _strokePainter.difficultyLevel = difficulty;
        }

        if(_dalgonaGameplay != null) _dalgonaGameplay.SetActive(true);

        // Timer starts HERE, when the gameplay begins.
        _currentTime = _gameDuration;
        _isTimerRunning = true;
        if(_timerText != null) 
        {
            _timerText.gameObject.SetActive(true);
            UpdateTimerUI();
        }
    }


    public void OnResponseDalgonaGameResult(bool isSuccess, ResponsePacketData.DalgonaGameResult data)
    {
        if (isSuccess)
        {
            Debug.Log("Dalgona Game: OnResponseDalgonaGameResult");
            // This is just a confirmation from server, actual result is handled by OnClick... methods
        }
    }

    public void OnClickDalgonaResultTrue()
    {
        if (!_isStarted) return;
        _isStarted = false; // Prevent double sending

        Debug.Log("성공! 서버에 결과를 전송합니다.");
        _isTimerRunning = false;
        NetworkManager.Instance.SendMessageToServer(new RequestPacketData.DalgonaGameResult(true));
        if(_dalgonaGameplay != null) _dalgonaGameplay.SetActive(false);
    }

    public void OnClickDalgonaResultFalse()
    {
        if (!_isStarted) return;
        _isStarted = false; // Prevent double sending

        Debug.Log("실패! 서버에 결과를 전송합니다.");
        _isTimerRunning = false;
        NetworkManager.Instance.SendMessageToServer(new RequestPacketData.DalgonaGameResult(false));
        if(_dalgonaGameplay != null) _dalgonaGameplay.SetActive(false);
    }
}
