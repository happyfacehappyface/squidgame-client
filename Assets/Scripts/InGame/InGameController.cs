using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class InGameController : MonoBehaviour
{

    [SerializeField] private InGameDrawer _inGameDrawer;
    private int _myIndex;
    public int MyIndex => _myIndex;
    private string[] _playerNames;
    public string[] PlayerNames => _playerNames;
    private bool[] _playerIsAlive;
    public bool[] PlayerIsAlive => _playerIsAlive;
    private int _playerCount;
    public int PlayerCount => _playerCount;
    private int _round;
    public int Round => _round;
    private ISubGameController _currentSubGame;

    private bool _isInititalized = false;
    private bool _isGameEnded = false;


    private ResponsePacketData.SubGameEnded _subGameEndedData;

    private ResponsePacketData.DalgonaGameStarted _dalgonaGameData;
    private ResponsePacketData.TugOfWarGameStarted _tugOfWarGameData;
    private ResponsePacketData.GameEnded _gameEndedData;
    private ResponsePacketData.RedLightGreenLightGameStarted _redLightGreenLightGameData;



    public void ManualStart(ResponsePacketData.StartGame startGameData)
    {
        _isGameEnded = false;
        _myIndex = startGameData.myIndex;
        _playerNames = startGameData.names;

        _playerIsAlive = Utils.CreateFill1D(_playerNames.Length, true);

        _playerCount = _playerNames.Length;
        _round = 0;
        DontDestroyOnLoad(gameObject);

        WaitingController waitingController = FindObjectOfType<WaitingController>();
        waitingController.ManualStart(this, startGameData);

        _currentSubGame = null;

        _inGameDrawer.ManualStart(this);

        _isInititalized = true;

    }

    private void ManualUpdate()
    {
        if (!_isInititalized || _isGameEnded)
        {
            return;
        }

        _currentSubGame?.ManualUpdate();
    }

    

    public void OnResponseReadyGame(bool isSuccess, ResponsePacketData.ReadyGame data)
    {
        if (isSuccess)
        {
            Debug.Log("Game Started");
        }
    }

    public void OnResponseReadySubGame(bool isSuccess, ResponsePacketData.ReadySubGame data)
    {
        if (isSuccess)
        {
            _currentSubGame?.OnSubGameStarted();
        }
    }

    public void OnResponseSubGameEnded(bool isSuccess, ResponsePacketData.SubGameEnded data)
    {
        if (isSuccess)
        {
            for (int i = 0; i < _playerIsAlive.Length; i++)
            {
                _playerIsAlive[i] = data.survivePlayerIndices.Contains(i);
            }

            var finalData = data;
            if (_currentSubGame is TugOfWarController && _tugOfWarGameData.unearnedWinPlayerIndex != null)
            {
                var newSurvivePlayerIndices = data.survivePlayerIndices.Union(_tugOfWarGameData.unearnedWinPlayerIndex).ToArray();
                finalData = new ResponsePacketData.SubGameEnded(newSurvivePlayerIndices);
            }
//코드 별로임 : 부전승자를 수동으로 추가하는 식
            _inGameDrawer.OnSubGameEnded(finalData);
        }
    }


/*
    private void OnWaitingSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnWaitingSceneLoaded;
        WaitingController waitingController = FindObjectOfType<WaitingController>();
        _currentSubGame = waitingController;
        waitingController.ManualStart(this);
        //waitingController.OnShowSubGameResult(_subGameEndedData);
    }
    */

    public void OnResponseGameEnded(bool isSuccess, ResponsePacketData.GameEnded data)
    {
        if (isSuccess)
        {
            _gameEndedData = data;
            SceneManager.sceneLoaded += OnResultSceneLoaded;
            SceneManager.LoadScene("ResultScene");
        }
    }

    private void OnResultSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnResultSceneLoaded;
        ResultController resultController = FindObjectOfType<ResultController>();
        _currentSubGame = null;
        resultController.ManualStart(this, _gameEndedData);
        _inGameDrawer.OnNewSubGameSceneLoaded();
    }

    public void OnResponseDalgonaGameStarted(bool isSuccess, ResponsePacketData.DalgonaGameStarted data)
    {
        if (isSuccess)
        {
            _dalgonaGameData = data;

            SceneManager.sceneLoaded += OnDalgonaSceneLoaded;
            SceneManager.LoadScene("DalgonaScene");
        }
    }


    private void OnDalgonaSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnDalgonaSceneLoaded;
        DalgonaController dalgonaController = FindObjectOfType<DalgonaController>();
        _currentSubGame = dalgonaController;
        dalgonaController.ManualStart(this, _dalgonaGameData);
        _inGameDrawer.OnNewSubGameSceneLoaded();
    }

    public void OnResponseRedLightGreenLightGameStarted(bool isSuccess, ResponsePacketData.RedLightGreenLightGameStarted data)
    {
        if (isSuccess)
        {
            _redLightGreenLightGameData = data;

            SceneManager.sceneLoaded += OnRedLightGreenLightSceneLoaded;
            SceneManager.LoadScene("RedLightGreenLightScene");
        }
    }

    private void OnRedLightGreenLightSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnRedLightGreenLightSceneLoaded;
        RedLightGreenLightController redLightGreenLightController = FindObjectOfType<RedLightGreenLightController>();
        _currentSubGame = redLightGreenLightController;
        redLightGreenLightController.ManualStart(this, _redLightGreenLightGameData);
        _inGameDrawer.OnNewSubGameSceneLoaded();
    }
    public void OnResponseTugOfWarGameStarted(bool isSuccess, ResponsePacketData.TugOfWarGameStarted data)
    {
        if (isSuccess)
        {
            _tugOfWarGameData = data;

            SceneManager.sceneLoaded += OnTugOfWarSceneLoaded;
            SceneManager.LoadScene("TugOfWarScene");
        }
    }

    private void OnTugOfWarSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnTugOfWarSceneLoaded;
        TugOfWarController tugOfWarController = FindObjectOfType<TugOfWarController>();
        _currentSubGame = tugOfWarController;
        tugOfWarController.ManualStart(this, _tugOfWarGameData);
        _inGameDrawer.OnNewSubGameSceneLoaded();
    }


    protected void Update()
    {
        ManualUpdate();
    }

    public void DestroyInGameController()
    {
        Destroy(gameObject);
    }

    public void OnPlayerDeath()
    {
        _inGameDrawer.SetDeathIndicatorTrue();
    }

    

    
}


public interface ISubGameController
{

    public abstract void OnSubGameStarted();
    public abstract void ManualUpdate();
}








