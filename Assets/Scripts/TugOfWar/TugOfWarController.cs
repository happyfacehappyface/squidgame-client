using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TugOfWarController : MonoBehaviour, ISubGameController
{
    
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform _leftPlayerParent;
    [SerializeField] private Transform _rightPlayerParent;
    [SerializeField] private Transform _ropeAndPlayer;
    [SerializeField] private Transform _ropeTransform;
    [SerializeField] private Image _redTeamSlider;
    [SerializeField] private Image _blueTeamSlider;

    private InGameController _inGameController;

    private ResponsePacketData.TugOfWarGameStarted _tugOfWarGameData;
    

    private int _pressCount;
    private int _deltaPressCount;

    private float _deltaPressCountMovingAverage;

    private Coroutine _keepSendingPressCountToServerCoroutine = null;

    List<TugOfWarPlayerComponent> _leftTeamPlayerComponents = new List<TugOfWarPlayerComponent>();
    List<TugOfWarPlayerComponent> _rightTeamPlayerComponents = new List<TugOfWarPlayerComponent>();

    private TugOfWarGameState _gameState;
    

    public void ManualStart(InGameController inGameController, ResponsePacketData.TugOfWarGameStarted data)
    {
        
        _inGameController = inGameController;
        _gameState = TugOfWarGameState.Waiting;
        _tugOfWarGameData = data;
        
        CreatePlayerCharacters();

        NetworkManager.Instance.SendMessageToServer(new RequestPacketData.ReadySubGame());
    }

    public void OnSubGameStarted()
    {
        Utils.Log("TugOfWarController.OnSubGameStarted");
        _gameState = TugOfWarGameState.Playing;
        _pressCount = 0;
        _deltaPressCountMovingAverage = 0.0f;
        _keepSendingPressCountToServerCoroutine = StartCoroutine(CO_KeepSendingPressCountToServer());

        foreach (var player in _leftTeamPlayerComponents)
        {
            player.OnCountDownStart();
        }

        foreach (var player in _rightTeamPlayerComponents)
        {
            player.OnCountDownStart();
        }

        

    }

    private void ClearPlayerCharacters()
    {
        foreach (Transform child in _leftPlayerParent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in _rightPlayerParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreatePlayerCharacters()
    {
        ClearPlayerCharacters();

        var leftTeamPlayerCharacterCount = Mathf.Min(_tugOfWarGameData.leftTeamPlayerIndex.Length, 10);
        var rightTeamPlayerCharacterCount = Mathf.Min(_tugOfWarGameData.rightTeamPlayerIndex.Length, 10);

        var isMeInLeftTeam = _tugOfWarGameData.leftTeamPlayerIndex.Contains(_inGameController.MyIndex);
        var isMeInRightTeam = _tugOfWarGameData.rightTeamPlayerIndex.Contains(_inGameController.MyIndex);

        int[] reorderedLeftTeamPlayerIndex;
        if (isMeInLeftTeam)
        {
            var otherPlayers = _tugOfWarGameData.leftTeamPlayerIndex.Where(x => x != _inGameController.MyIndex).ToArray();
            reorderedLeftTeamPlayerIndex = new int[] { _inGameController.MyIndex }.Concat(otherPlayers).ToArray();
        }
        else
        {
            reorderedLeftTeamPlayerIndex = _tugOfWarGameData.leftTeamPlayerIndex;
        }

        int[] reorderedRightTeamPlayerIndex;
        if (isMeInRightTeam)
        {
            var otherPlayers = _tugOfWarGameData.rightTeamPlayerIndex.Where(x => x != _inGameController.MyIndex).ToArray();
            reorderedRightTeamPlayerIndex = new int[] { _inGameController.MyIndex }.Concat(otherPlayers).ToArray();
        }
        else
        {
            reorderedRightTeamPlayerIndex = _tugOfWarGameData.rightTeamPlayerIndex;
        }



        for (int i = 0; i < leftTeamPlayerCharacterCount; i++)
        {
            var player = Instantiate(_playerPrefab, _leftPlayerParent).GetComponent<TugOfWarPlayerComponent>();
            _leftTeamPlayerComponents.Add(player);
            player.ManualStart(reorderedLeftTeamPlayerIndex[i], _inGameController.PlayerNames[reorderedLeftTeamPlayerIndex[i]], reorderedLeftTeamPlayerIndex[i] == _inGameController.MyIndex);
            player.transform.localPosition = new Vector3(i * (- 200.0f), 0.0f, 0.0f);
        }

        for (int i = 0; i < rightTeamPlayerCharacterCount; i++)
        {
            var player = Instantiate(_playerPrefab, _rightPlayerParent).GetComponent<TugOfWarPlayerComponent>();
            _rightTeamPlayerComponents.Add(player);
            player.ManualStart(reorderedRightTeamPlayerIndex[i], _inGameController.PlayerNames[reorderedRightTeamPlayerIndex[i]], reorderedRightTeamPlayerIndex[i] == _inGameController.MyIndex);
            player.transform.localPosition = new Vector3(i * (- 200.0f), 0.0f, 0.0f);
        }





    }

    public void ManualUpdate()
    {

        if (_gameState != TugOfWarGameState.Playing)
        {
            return;
        }

        HandleInput();
        float smoothingSpeed = 1.0f; // 초당 변화 속도 (값이 클수록 빠르게 변화)
        float deltaTimeFactor = Mathf.Clamp01(smoothingSpeed * Time.deltaTime);
        _deltaPressCountMovingAverage = Mathf.Lerp(_deltaPressCountMovingAverage, _deltaPressCount, deltaTimeFactor);

        UpdateViewUsingDeltaPressCount();

    }

    private void UpdateViewUsingDeltaPressCount()
    {
        float clampedDeltaMovingAverage = Mathf.Clamp(_deltaPressCountMovingAverage / 50.0f, -1.0f, 1.0f);

        _ropeAndPlayer.transform.localPosition = new Vector3(clampedDeltaMovingAverage * (- 100.0f), 0.0f, 0.0f);

        _redTeamSlider.fillAmount = Mathf.Clamp01(0.5f + (clampedDeltaMovingAverage / 2.0f));
        _blueTeamSlider.fillAmount = Mathf.Clamp01(0.5f - (clampedDeltaMovingAverage / 2.0f));
    }

    public void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _pressCount++;
        }
    }

    IEnumerator CO_KeepSendingPressCountToServer()
    {
        float interval;
        while (true)
        {
            interval = Random.Range(0.5f, 1.0f);
            yield return new WaitForSeconds(interval);
            SendPressCountToServer();
        }
    }

    private void SendPressCountToServer()
    {
        NetworkManager.Instance.SendMessageToServer(new RequestPacketData.TugOfWarGamePressCount(_pressCount));
        _pressCount = 0;
    }

    public void OnResponseTugOfWarGamePressCount(bool isSuccess, ResponsePacketData.TugOfWarGamePressCount data)
    {
        if (isSuccess)
        {
            _deltaPressCount = data.deltaPressCount;
        }
    }

    public void OnResponseTugOfWarGameResult(bool isSuccess, ResponsePacketData.TugOfWarGameResult data)
    {
        if (isSuccess)
        {
            if (_keepSendingPressCountToServerCoroutine != null)
            {
                StopCoroutine(_keepSendingPressCountToServerCoroutine);
                _keepSendingPressCountToServerCoroutine = null;
            }

            _deltaPressCount = data.deltaPressCount;
            _deltaPressCountMovingAverage = _deltaPressCount;

            UpdateViewUsingDeltaPressCount();

            _gameState = TugOfWarGameState.Ended;

            StartCoroutine(CO_ShowTugOfWarResultAnimation(data.isLeftWin));
        }
    }

    IEnumerator CO_ShowTugOfWarResultAnimation(bool isLeftWin)
    {

        foreach (var player in _leftTeamPlayerComponents)
        {

            if (isLeftWin)
            {
                player.OnWin();
            }
            else
            {
                player.OnLose();
            }
        }

        foreach (var player in _rightTeamPlayerComponents)
        {
            if (isLeftWin)
            {
                player.OnLose();
            }
            else
            {
                player.OnWin();
            }
        }

        float progress = 0.0f;
        float duration = 1.3f;

        int playerCount = isLeftWin ? _rightTeamPlayerComponents.Count : _leftTeamPlayerComponents.Count;

        Vector3[] playerPositionOrigin = new Vector3[playerCount];
        Vector3[] playerPositionDestination = new Vector3[playerCount];

        for (int i = 0; i < playerCount; i++)
        {
            playerPositionOrigin[i] = isLeftWin ? _rightTeamPlayerComponents[i].transform.localPosition : _leftTeamPlayerComponents[i].transform.localPosition;
            playerPositionDestination[i] = new Vector3(Random.Range(400f, 1400f), Random.Range(150f, 500f), 0.0f);
        }

        
        while (progress < 1.0f)
        {
            progress += Time.deltaTime / duration;

            for (int i = 0; i < playerCount; i++)
            {

                if (isLeftWin)
                {
                    _rightTeamPlayerComponents[i].transform.localPosition = Vector3.Lerp(playerPositionOrigin[i], playerPositionDestination[i], progress);
                }
                else
                {
                    _leftTeamPlayerComponents[i].transform.localPosition = Vector3.Lerp(playerPositionOrigin[i], playerPositionDestination[i], progress);
                }

                
            }

            _ropeTransform.transform.localPosition = new Vector3((isLeftWin ? -1.0f : 1.0f) * progress * 300.0f, 0.0f, 0.0f);

            yield return null;
        }
    }




    private enum TugOfWarGameState
    {
        Waiting,
        Playing,
        Ended,
    }

}


