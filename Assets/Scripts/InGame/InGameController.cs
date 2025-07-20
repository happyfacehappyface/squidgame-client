using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameController : MonoBehaviour
{
    private int _myIndex;
    public int MyIndex => _myIndex;
    private string[] _playerNames;
    public string[] PlayerNames => _playerNames;
    private string[] _playerIsAlive;
    public string[] PlayerIsAlive => _playerIsAlive;
    private int _playerCount;
    public int PlayerCount => _playerCount;
    private int _round;
    public int Round => _round;
    private ISubGameController _currentSubGame;

    private bool _isInititalized = false;


    private ResponsePacketData.SubGameEnded _subGameEndedData;

    private ResponsePacketData.DalgonaGameStarted _dalgonaGameData;
    private ResponsePacketData.TugOfWarGameStarted _tugOfWarGameData;



    public void ManualStart(ResponsePacketData.StartGame startGameData)
    {

        _myIndex = startGameData.myIndex;
        _playerNames = startGameData.names;
        _playerIsAlive = new string[_playerNames.Length];
        _playerCount = _playerNames.Length;
        _round = 0;
        DontDestroyOnLoad(gameObject);
        _currentSubGame = FindObjectOfType<WaitingController>();

        _isInititalized = true;

        NetworkManager.Instance.SendMessageToServer(new RequestPacketData.ReadyGame());
    }

    private void ManualUpdate()
    {
        if (!_isInititalized)
        {
            return;
        }

        _currentSubGame.ManualUpdate();
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
            _currentSubGame.OnSubGameStarted();
        }
    }

    public void OnResponseSubGameEnded(bool isSuccess, ResponsePacketData.SubGameEnded data)
    {
        if (isSuccess)
        {
            _subGameEndedData = data;
            SceneManager.LoadScene("WaitingScene");
            SceneManager.sceneLoaded += OnWaitingSceneLoaded;
        }
    }

    private void OnWaitingSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnWaitingSceneLoaded;
        WaitingController waitingController = FindObjectOfType<WaitingController>();
        _currentSubGame = waitingController;
        waitingController.ManualStart(this);
        waitingController.OnShowSubGameResult(_subGameEndedData);
    }

    public void OnResponseGameEnded(bool isSuccess, ResponsePacketData.GameEnded data)
    {
        if (isSuccess)
        {
            
        }
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
        dalgonaController.ManualStart(_dalgonaGameData);
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
    }


    protected void Update()
    {
        ManualUpdate();
    }

    
}


public interface ISubGameController
{

    public abstract void OnSubGameStarted();
    public abstract void ManualUpdate();
}








