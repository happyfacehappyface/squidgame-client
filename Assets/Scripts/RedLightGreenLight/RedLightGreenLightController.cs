using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
public class RedLightGreenLightController : MonoBehaviour, ISubGameController
{

    [SerializeField] private RedLightGreenLightDrawer _drawer;

    private InGameController _inGameController;
    private ResponsePacketData.RedLightGreenLightGameStarted _redLightGreenLightGameData;

    [HideInInspector] public bool[] PlayerIsPlaying;
    [HideInInspector] public float[] PlayerProgress;
    [HideInInspector] public float[] PlayerProgressMovingAverage;

    private bool _isGameStarted;

    private TimeSpan _timeLimit;
    private TimeSpan _currentTime;
    [HideInInspector] public TimeSpan TimeLeft => _timeLimit - _currentTime;

    [HideInInspector] public LightState CurrentLightState;

    public int MyIndex => _inGameController.MyIndex;


    private float _myProgressVelocity;
    private const float _velocityLimit = 0.1f;

    private bool _isMePlaying => PlayerIsPlaying[MyIndex];

    public int PlayerCount => _inGameController.PlayerCount;

    private Coroutine _keepSendMyPositionCoroutine;

    public void ManualStart(InGameController inGameController, ResponsePacketData.RedLightGreenLightGameStarted data)
    {
        Utils.Log("RedLightGreenLightController.ManualStart");
        _inGameController = inGameController;
        _redLightGreenLightGameData = data;

        PlayerIsPlaying = Utils.DeepCopy1D(_inGameController.PlayerIsAlive);
        PlayerProgress = Utils.CreateFill1D(_inGameController.PlayerCount, 0f);
        PlayerProgressMovingAverage = Utils.CreateFill1D(_inGameController.PlayerCount, 0f);

        _isGameStarted = false;

        _timeLimit = TimeSpan.FromMilliseconds(_redLightGreenLightGameData.timeLimitMs);
        _currentTime = TimeSpan.Zero;

        CurrentLightState = new LightState.Green();

        _myProgressVelocity = 0f;

        _drawer.ManualStart(this);

        NetworkManager.Instance.SendMessageToServer(new RequestPacketData.ReadySubGame());
    }

    public void OnSubGameStarted()
    {
        Utils.Log("RedLightGreenLightController.OnSubGameStarted");
        _isGameStarted = true;
        _keepSendMyPositionCoroutine = StartCoroutine(CO_KeepSendMyPosition());
    }

    private IEnumerator CO_KeepSendMyPosition()
    {
        while (true)
        {
            NetworkManager.Instance.SendMessageToServer(new RequestPacketData.RedLightGreenLightPlayerPosition((int)(PlayerProgress[MyIndex] * 1000)));
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void HandleInput()
    {
        if (!_isGameStarted) return;
        if (!_isMePlaying) return;

        if (Input.GetKey(KeyCode.Space))
        {
            _myProgressVelocity = Mathf.Clamp(_myProgressVelocity + (Time.deltaTime * 0.1f), 0f, _velocityLimit);
        }
        else
        {
            _myProgressVelocity = Mathf.Clamp(_myProgressVelocity - (Time.deltaTime * 0.1f), 0f, _velocityLimit);
        }

    }

    private void UpdateMyProgress()
    {
        if (!_isGameStarted) return;
        if (!_isMePlaying) return;

        float myNewProgress = Mathf.Clamp01(PlayerProgress[MyIndex] + (_myProgressVelocity * Time.deltaTime));

        PlayerProgress[MyIndex] = myNewProgress;
        PlayerProgressMovingAverage[MyIndex] = myNewProgress;
    }

    private void CheckEndCondition()
    {
        if (!_isGameStarted) return;
        if (!_isMePlaying) return;

        if ((_myProgressVelocity > (_velocityLimit * 0.2f)) && (CurrentLightState is LightState.Red))
        {
            NetworkManager.Instance.SendMessageToServer(new RequestPacketData.RedLightGreenLightPlayerResult(false));
            PlayerIsPlaying[MyIndex] = false;
        }
        else if (PlayerProgress[MyIndex] >= 1.0f)
        {
            NetworkManager.Instance.SendMessageToServer(new RequestPacketData.RedLightGreenLightPlayerResult(true));
            PlayerIsPlaying[MyIndex] = false;
        }
    }

    public void ManualUpdate()
    {
        _currentTime = _currentTime + TimeSpan.FromSeconds(Time.deltaTime);

        HandleInput();
        UpdateMyProgress();
        UpdateMovingAverage();
        CheckEndCondition();

        if (CurrentLightState is LightState.Yellow yellow)
        {
            if (_currentTime - yellow.StartTime > TimeSpan.FromSeconds(0.8f))
            {
                CurrentLightState = new LightState.Red();
            }
        }

        _drawer.ManualUpdate();
    }

    private void UpdateMovingAverage()
    {
        for (int i = 0; i < PlayerProgress.Length; i++)
        {
            if (PlayerIsPlaying[i])
            {
                PlayerProgressMovingAverage[i] = Mathf.Lerp(PlayerProgressMovingAverage[i], PlayerProgress[i], Time.deltaTime);
            }
        }
    }



    public void OnResponseRedLightGreenLightLightChanged(bool isSuccess, ResponsePacketData.RedLightGreenLightLightChanged data)
    {
        if (isSuccess)
        {
            if (data.redLightOn)
            {
                CurrentLightState = new LightState.Yellow(_currentTime);
            }
            else
            {
                CurrentLightState = new LightState.Green();
            }
        }
    }

    public void OnResponseRedLightGreenLightPlayerResult(bool isSuccess, ResponsePacketData.RedLightGreenLightPlayerResult data)
    {
        if (isSuccess)
        {
            PlayerIsPlaying[data.playerIndex] = false;
            _drawer.OnResponsePlayerResult(data);
        }
    }

    public void OnResponseRedLightGreenLightPlayerPosition(bool isSuccess, ResponsePacketData.RedLightGreenLightPlayerPosition data)
    {
        if (isSuccess)
        {
            for (int i = 0; i < PlayerProgress.Length; i++)
            {
                if ((i != MyIndex) && (PlayerIsPlaying[i]) && (data.progress[i] >= 0f))
                {
                    PlayerProgress[i] = data.progress[i] / 1000f;
                }
            }
        }
    }

    public void OnResponseRedLightGreenLightGameResult(bool isSuccess, ResponsePacketData.RedLightGreenLightGameResult data)
    {
        if (isSuccess)
        {
            Utils.Log("RedLightGreenLightController.OnResponseRedLightGreenLightGameResult");
            PlayerIsPlaying = Utils.CreateFill1D(PlayerCount, false);
            _drawer.OnResponseGameResult(data);
            _isGameStarted = false;
            StopCoroutine(_keepSendMyPositionCoroutine);
        }
    }



    public abstract record LightState
    {
        public sealed record Red() : LightState;
        public sealed record Yellow(TimeSpan StartTime) : LightState;
        public sealed record Green() : LightState;
    }
}






