using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;

public class DalgonaController : MonoBehaviour, ISubGameController
{

    [Header("UI & Game Control")]
    [SerializeField] private GameObject _preGameBarrier;
    [SerializeField] private GameObject _shapeSelectionBarrier;
    [SerializeField] private GameObject _dalgonaGameplay;
    [SerializeField] private StrokePainter _strokePainter;
    [SerializeField] private GameObject _deadNotice;
    [SerializeField] private Animator _failureAnimator;

    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI _timerText;
    private TimeSpan _currentTime;
    private bool _isTimerRunning = false;

    private ResponsePacketData.DalgonaGameStarted _dalgonaGameData;
    private InGameController _inGameController;

    public void ManualStart(InGameController inGameController, ResponsePacketData.DalgonaGameStarted data)
    {
        _inGameController = inGameController;
        _isTimerRunning = false;

        _preGameBarrier.SetActive(true);

        _dalgonaGameData = data;

        _currentTime = TimeSpan.FromMilliseconds(data.timeLimitMs);
        Debug.Log("Dalgona Game: ManualStart");

        _dalgonaGameplay.SetActive(false);
        _shapeSelectionBarrier.SetActive(false);
        
        bool isPlaying = _dalgonaGameData.playerIndices.Contains(_inGameController.MyIndex);
        if (_deadNotice != null)
        {
            _deadNotice.SetActive(!isPlaying);
        }

        if (isPlaying)
        {
            StartCoroutine(ShowInstructionsAndReady());
        }
    }

    private IEnumerator ShowInstructionsAndReady()
    {
        Animator barrierAnimator = _preGameBarrier.GetComponent<Animator>();
        
        if (barrierAnimator != null)
        {
            barrierAnimator.SetTrigger("Show");
        }

        yield return new WaitForSeconds(5f);
        
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
//디버깅용
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnSuccess();
        }
#endif
    }
    
    private void UpdateTimerUI()
    {
        _timerText.text = $"{Mathf.Max(0, (float) _currentTime.TotalSeconds):F2}";
    }

    // Called by ShapeSelectionController
    public void OnShapeSelected(StrokePainter.DalgonaShapeType shape, int difficulty)
    {
        Debug.Log($"모양({shape}, 난이도:{difficulty}) 선택됨. 게임플레이를 시작합니다.");

        
        if(_strokePainter != null)
        {
            _strokePainter.shapeToGenerate = shape;
            _strokePainter.difficultyLevel = difficulty;
        }

        _shapeSelectionBarrier.SetActive(true);

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
        Utils.Log("Dalgona Game: OnSuccess");
        NetworkManager.Instance.SendMessageToServer(new RequestPacketData.DalgonaGameResult(true));
    }

    public void OnFail()
    {
        Utils.Log("Dalgona Game: OnFail");
        if (_failureAnimator != null)
        {
            _failureAnimator.gameObject.SetActive(true);
            _failureAnimator.SetTrigger("Fail");
        }
        NetworkManager.Instance.SendMessageToServer(new RequestPacketData.DalgonaGameResult(false));
        _inGameController.OnPlayerDeath();
    }

}
