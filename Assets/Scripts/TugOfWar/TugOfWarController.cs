using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TugOfWarController : MonoBehaviour, ISubGameController
{
    
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform _leftPlayerParent;
    [SerializeField] private Transform _rightPlayerParent;
    [SerializeField] private Transform _ropeAndPlayer;
    [SerializeField] private Transform _leftRopeTransform;
    [SerializeField] private Transform _rightRopeTransform;
    [SerializeField] private Image _leftTeamSlider;
    [SerializeField] private Image _rightTeamSlider;

    [SerializeField] private GameObject _handPrefab;
    [SerializeField] private Transform _leftHandParent;
    [SerializeField] private Transform _rightHandParent;

    [SerializeField] private TextMeshProUGUI _noticeText;
    [SerializeField] private Animator _spaceBarAnimator;

    [SerializeField] private GameObject _unearnedWinNotice;
    [SerializeField] private GameObject _deadNotice;

    [SerializeField] private GameObject _preGameBarrier;

    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI _timerText;
    private TimeSpan _currentTime;
    private bool _isTimerRunning = false;

    private InGameController _inGameController;

    private ResponsePacketData.TugOfWarGameStarted _tugOfWarGameData;
    

    private int _pressCount;
    private int _deltaPressCount;

    private float _deltaPressCountMovingAverage;

    private Coroutine _keepSendingPressCountToServerCoroutine = null;

    List<TugOfWarPlayerComponent> _leftTeamPlayerComponents = new List<TugOfWarPlayerComponent>();
    List<TugOfWarPlayerComponent> _rightTeamPlayerComponents = new List<TugOfWarPlayerComponent>();

    List<GameObject> _leftHands = new List<GameObject>();
    List<GameObject> _rightHands = new List<GameObject>();

    private TugOfWarGameState _gameState;

    private bool _isPlaying = false;
    private bool _isLeftTeam = false;
    

    public void ManualStart(InGameController inGameController, ResponsePacketData.TugOfWarGameStarted data)
    {
        
        _inGameController = inGameController;
        _gameState = TugOfWarGameState.Waiting;
        _tugOfWarGameData = data;

        _preGameBarrier.SetActive(true);

        bool isPlaying = _tugOfWarGameData.leftTeamPlayerIndex.Contains(_inGameController.MyIndex) || _tugOfWarGameData.rightTeamPlayerIndex.Contains(_inGameController.MyIndex);
        bool isUnearnedWin = _tugOfWarGameData.unearnedWinPlayerIndex != null && _tugOfWarGameData.unearnedWinPlayerIndex.Contains(_inGameController.MyIndex);

        _isPlaying = isPlaying;
        _isLeftTeam = _tugOfWarGameData.leftTeamPlayerIndex.Contains(_inGameController.MyIndex);


        if (_unearnedWinNotice != null)
        {
            _unearnedWinNotice.SetActive(isUnearnedWin);
        }

        if (_deadNotice != null)
        {
            _deadNotice.SetActive(!isUnearnedWin && !isPlaying);
        }

        _isTimerRunning = false;
        _currentTime = TimeSpan.FromMilliseconds(data.timeLimitMs);
        UpdateTimerUI();
        SoundManager.Instance.PlaySfxSubGameStart(0.0f);
        CreatePlayerCharacters();

        _noticeText.text = "";

        

        StartCoroutine(ShowInstructionsAndReady());
        

    }

    private IEnumerator ShowInstructionsAndReady()
    {
        if (_preGameBarrier != null)
        {
            _preGameBarrier.SetActive(true);
            Animator barrierAnimator = _preGameBarrier.GetComponent<Animator>();
            if (barrierAnimator != null)
            {
                barrierAnimator.SetTrigger("Show");
            }
        }

        yield return new WaitForSeconds(5f);

        NetworkManager.Instance.SendMessageToServer(new RequestPacketData.ReadySubGame());
    }

    public void OnSubGameStarted()
    {
        _preGameBarrier.SetActive(false);
        Utils.Log("TugOfWarController.OnSubGameStarted");
        _gameState = TugOfWarGameState.Playing;
        _isTimerRunning = true;
        _pressCount = 0;
        _deltaPressCountMovingAverage = 0.0f;


        bool isPlaying = _tugOfWarGameData.leftTeamPlayerIndex.Contains(_inGameController.MyIndex) || _tugOfWarGameData.rightTeamPlayerIndex.Contains(_inGameController.MyIndex);
        
        if (isPlaying)
        {
            _keepSendingPressCountToServerCoroutine = StartCoroutine(CO_KeepSendingPressCountToServer());
        }

        _noticeText.text = "스페이스 바를 연타하세요!";

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

    private void ClearHands()
    {
        foreach (Transform child in _leftHandParent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in _rightHandParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreatePlayerCharacters()
    {
        ClearPlayerCharacters();
        ClearHands();

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
            player.ManualStart(reorderedLeftTeamPlayerIndex[i], _inGameController.PlayerNames[reorderedLeftTeamPlayerIndex[i]], reorderedLeftTeamPlayerIndex[i] == _inGameController.MyIndex, true);
            player.transform.localPosition = new Vector3(i * (- 200.0f), 0.0f, 0.0f);

            var hand = Instantiate(_handPrefab, _leftHandParent);
            _leftHands.Add(hand);
            hand.transform.localPosition = new Vector3(i * (- 200.0f), 0.0f, 0.0f);
            hand.transform.localScale = new Vector3(1, 1, 1);
        }

        for (int i = 0; i < rightTeamPlayerCharacterCount; i++)
        {
            var player = Instantiate(_playerPrefab, _rightPlayerParent).GetComponent<TugOfWarPlayerComponent>();
            _rightTeamPlayerComponents.Add(player);
            player.ManualStart(reorderedRightTeamPlayerIndex[i], _inGameController.PlayerNames[reorderedRightTeamPlayerIndex[i]], reorderedRightTeamPlayerIndex[i] == _inGameController.MyIndex, false);
            player.transform.localPosition = new Vector3(i * (+ 200.0f), 0.0f, 0.0f);

            var hand = Instantiate(_handPrefab, _rightHandParent);
            _rightHands.Add(hand);
            hand.transform.localPosition = new Vector3(i * (+ 200.0f), 0.0f, 0.0f);
            hand.transform.localScale = new Vector3(-1, 1, 1);
        }



    }

    public void ManualUpdate()
    {
        if (_isTimerRunning)
        {
            _currentTime -= TimeSpan.FromSeconds(Time.deltaTime);
            UpdateTimerUI();
        }

        if (_gameState != TugOfWarGameState.Playing)
        {
            return;
        }

        HandleInput();
        float smoothingSpeed = 1.0f;
        float deltaTimeFactor = Mathf.Clamp01(smoothingSpeed * Time.deltaTime);
        _deltaPressCountMovingAverage = Mathf.Lerp(_deltaPressCountMovingAverage, _deltaPressCount, deltaTimeFactor);

        UpdateViewUsingDeltaPressCount();

    }

    private void UpdateTimerUI()
    {
        _timerText.text = $"{Mathf.Max(0, (float) _currentTime.TotalSeconds):F2}";
    }

    private void UpdateViewUsingDeltaPressCount()
    {
        float sigmoidDeltaMovingAverage = 2.0f / (1.0f + Mathf.Exp(-0.02f * _deltaPressCountMovingAverage)) - 1.0f;
        float clampedDeltaMovingAverage = Mathf.Clamp(sigmoidDeltaMovingAverage, -1.0f, 1.0f);

        _ropeAndPlayer.transform.localPosition = new Vector3(clampedDeltaMovingAverage * (- 300.0f), 0.0f, 0.0f);

        _leftTeamSlider.fillAmount = Mathf.Clamp01(0.5f + (clampedDeltaMovingAverage / 2.0f));
        _rightTeamSlider.fillAmount = Mathf.Clamp01(0.5f - (clampedDeltaMovingAverage / 2.0f));
    }

    public void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _spaceBarAnimator.SetTrigger("Press");
            

            int score = 5;

            int winningCount = _deltaPressCount * (_isLeftTeam ? +1 : -1);

            if (winningCount < 0)
            {
                score = 6;
            }
            else if (winningCount < -50)
            {
                score = 8;
            }
            else if (winningCount < -100)
            {
                score = 10;
            }

            _pressCount += score;

        }
    }

    IEnumerator CO_KeepSendingPressCountToServer()
    {
        float interval;
        while (true)
        {
            interval = UnityEngine.Random.Range(0.5f, 1.0f);
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
            SoundManager.Instance.PlaySfxRope(0.0f);
            _deltaPressCount = data.deltaPressCount;
        }
    }

    public void OnResponseTugOfWarGameResult(bool isSuccess, ResponsePacketData.TugOfWarGameResult data)
    {
        _spaceBarAnimator.SetTrigger("Hide");

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

            SoundManager.Instance.PlaySfxSlash(0.0f);
            SoundManager.Instance.PlaySfxWind(0.0f);

            StartCoroutine(CO_ShowTugOfWarResultAnimation(data.isLeftWin));

            foreach (var hand in _leftHands)
            {
                hand.SetActive(false);
            }

            foreach (var hand in _rightHands)
            {
                hand.SetActive(false);
            }


            if (!data.isLeftWin && _tugOfWarGameData.leftTeamPlayerIndex.Contains(_inGameController.MyIndex))
            {
                _inGameController.OnPlayerDeath();
            }
            else if (data.isLeftWin && _tugOfWarGameData.rightTeamPlayerIndex.Contains(_inGameController.MyIndex))
            {
                _inGameController.OnPlayerDeath();
            }
        }
    }

    IEnumerator CO_ShowTugOfWarResultAnimation(bool isLeftWin)
    {
        if (isLeftWin)
        {
            _noticeText.text = "왼쪽 팀 승리!";
        }
        else
        {
            _noticeText.text = "오른쪽 팀 승리!";
        }

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
            playerPositionDestination[i] = new Vector3(UnityEngine.Random.Range(400f, 1400f), UnityEngine.Random.Range(150f, 500f), 0.0f);
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


            float ropeHeight = 10f;
            if (isLeftWin)
            {
                _leftRopeTransform.transform.localPosition = new Vector3(-600f * progress, -ropeHeight * progress, 0.0f);
                _rightRopeTransform.transform.localPosition = new Vector3(100f * progress, -ropeHeight * progress, 0.0f);
            }
            else
            {
                _leftRopeTransform.transform.localPosition = new Vector3(-100f * progress, -ropeHeight * progress, 0.0f);
                _rightRopeTransform.transform.localPosition = new Vector3(600f * progress, -ropeHeight * progress, 0.0f);
            }


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


