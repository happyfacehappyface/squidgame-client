using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OutGameController : MonoBehaviour
{
    private ResponsePacketData.StartGame _startGameData;

    private static string _playerName = "";

    [SerializeField] private OutGamePopupHandler _popupHandler;

    [SerializeField] private GameObject _inGameControllerPrefab;
    [SerializeField] private GameObject _waitForServer;
    [SerializeField] private TextMeshProUGUI _playerCountText;

    [SerializeField] private TMP_InputField _playerNameInputField;
    [SerializeField] private Animator _canvasAnimator;


    [SerializeField] private Button _startGameButton;

    // Start is called before the first frame update
    void Start()
    {
        ManualStart();
    }

    // Update is called once per frame
    void Update()
    {
        #if RELEASE
        if ((Input.GetKeyDown(KeyCode.Z)) && (Input.GetKeyDown(KeyCode.A)) && (Input.GetKeyDown(KeyCode.Q)) && (Input.GetKeyDown(KeyCode.M)))
        {
            _startGameButton.interactable = true;
        }
        #endif
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
        #if RELEASE
        _startGameButton.interactable = false;
        #endif


        _popupHandler.ManualStart(this);
        SoundManager.Instance.PlayBgmOutGame();

        if (_playerName == "")
        {
            _playerName = GenerateRandomPlayerName();
            _playerNameInputField.text = _playerName;
        }
        else
        {
            _playerNameInputField.text = _playerName;
        }
    }

    public void RequestEnterRoom()
    {
        if (_playerNameInputField.text.Length == 0)
        {
            _popupHandler.OpenErrorPopup("문제 발생!", "플레이어 이름을 입력해주세요!");
            return;
        }
        else
        {
            _waitForServer.SetActive(true);
            NetworkManager.Instance.SendMessageToServer(new RequestPacketData.EnterRoom(_playerNameInputField.text));
            _playerName = _playerNameInputField.text;
        }
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
            if (_playerNameInputField.text == "서우게임즈")
            {
                SoundManager.Instance.PlayVoiceIntro(0f);
            }

            _canvasAnimator.SetTrigger("InRoom");
            SoundManager.Instance.PlaySfxSpaceShip(0.0f);
            SoundManager.Instance.PlaySfxAir(1.0f);
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
            SoundManager.Instance.PlaySfxDoor(0f);
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
            SoundManager.Instance.PlaySfxDoor(0f);
        }
    }

    public void OnResponseStartGame(bool isSuccess, ResponsePacketData.StartGame data)
    {
        if (isSuccess)
        {
            _startGameData = data;
            
            StartCoroutine(CO_OpenInGameScene());
        }

        _waitForServer.SetActive(false);
    }

    private IEnumerator CO_OpenInGameScene()
    {
        _canvasAnimator.SetTrigger("StartGame");
        SoundManager.Instance.PlaySfxSpaceShip(0.0f);
        SoundManager.Instance.PlaySfxGameStart(0.0f);
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
    
    private string GenerateRandomPlayerName()
    {

        string[] candidate = new string[] {"동현", "승준", "재헌", "기람", "재현", "지민", "서경", "신이", "다인", "서진", "연재", "재현", "창민", "하민", "서우", "현우", "예영", "한준", "광호", "서우", "서우", "서우", "서우", "서우"};

        return candidate[Random.Range(0, candidate.Length)];
    }

    public void OnClickShuffleName()
    {
        _playerName = GenerateRandomPlayerName();
        _playerNameInputField.text = _playerName;
        SoundManager.Instance.PlaySfxDice(0.0f);
    }
}
