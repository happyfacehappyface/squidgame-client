using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class RedLightGreenLightDrawer : MonoBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform _playerParent;

    [SerializeField] private Animator _lightAnimator;

    [SerializeField] private GameObject _ufoBulletPrefab;
    [SerializeField] private Transform _ufoBulletParent;

    [SerializeField] private Transform _ufoTransform;

    [SerializeField] private TextMeshProUGUI _timeCountText;


    private RedLightGreenLightController _controller;

    private RedLightGreenLightPlayerComponent[] _playerComponents;

    private List<RedLightGreenLightUFOBulletComponent> _ufoBulletComponents;

    private TimeSpan _ufoMoveTime;


    private const float _ufoAmplitude = 25f;
    private const float _ufoPeriodinSeconds = 2f;

    public void ManualStart(RedLightGreenLightController redLightGreenLightController)
    {
        _controller = redLightGreenLightController;

        _playerComponents = new RedLightGreenLightPlayerComponent[_controller.PlayerCount];
        _ufoBulletComponents = new List<RedLightGreenLightUFOBulletComponent>();
        
        CreatePlayerComponents();

        _ufoTransform.localPosition = new Vector3(0f, 0f, 0f);
        _ufoMoveTime = TimeSpan.Zero;
    }

    public void ManualUpdate()
    {

        UpdateLight();
        UpdatePlayerPosition();

        UpdateUFOPosition();
        UpdateUFOBulletComponents();

        int timeLeftInt = Mathf.Max(0, (int)(_controller.TimeLeft.TotalSeconds));

        _timeCountText.text = timeLeftInt.ToString();
        _timeCountText.color = timeLeftInt > 10 ? Color.black : Color.red;
    }

    private void UpdatePlayerPosition()
    {
        for (var i = 0; i < _controller.PlayerCount; i++)
        {
            if (_controller.PlayerIsPlaying[i])
            {
                _playerComponents[i].transform.localPosition = new Vector3(_controller.PlayerProgressMovingAverage[i] * 1500f, 0f, 0f);
            }
        }
    }

    private void UpdateLight()
    {
        switch (_controller.CurrentLightState)
        {
            case RedLightGreenLightController.LightState.Red:
                _lightAnimator.SetInteger("Color", 0);
                break;
            case RedLightGreenLightController.LightState.Yellow:
                _lightAnimator.SetInteger("Color", 1);
                break;
            case RedLightGreenLightController.LightState.Green:
                _lightAnimator.SetInteger("Color", 2);
                break;
            
        }
    }

    private void CreateNewUFOBullet(float progress)
    {
        RedLightGreenLightUFOBulletComponent ufoBulletComponent = Instantiate(_ufoBulletPrefab, _ufoBulletParent).GetComponent<RedLightGreenLightUFOBulletComponent>();
        ufoBulletComponent.ManualStart(new Vector2(1760f, 950f), new Vector2(150f + (progress * 1500f), 320f), TimeSpan.FromSeconds(0.5f));
        _ufoBulletComponents.Add(ufoBulletComponent);
    }

    private void UpdateUFOPosition()
    {
        _ufoMoveTime = _ufoMoveTime + TimeSpan.FromSeconds(Time.deltaTime);

        _ufoTransform.localPosition = new Vector3(0f, _ufoAmplitude * Mathf.Sin(2f * Mathf.PI * (float)_ufoMoveTime.TotalSeconds / _ufoPeriodinSeconds), 0f);
    }

    private void UpdateUFOBulletComponents()
    {
        for (var i = _ufoBulletComponents.Count - 1; i >= 0; i--)
        {
            if (_ufoBulletComponents[i].ShouldBeDeleted())
            {
                Destroy(_ufoBulletComponents[i].gameObject);
                _ufoBulletComponents.RemoveAt(i);
            }
            else
            {
                _ufoBulletComponents[i].ManualUpdate();
            }
        }
    }

    public void ClearPlayerComponents()
    {
        foreach (Transform child in _playerParent)
        {
            Destroy(child.gameObject);
        }
    }

    public void CreatePlayerComponents()
    {
        ClearPlayerComponents();

        for (var i = 0; i < _controller.PlayerCount; i++)
        {
            if (_controller.PlayerIsPlaying[i])
            {
                RedLightGreenLightPlayerComponent playerComponent = Instantiate(_playerPrefab, _playerParent).GetComponent<RedLightGreenLightPlayerComponent>();
                _playerComponents[i] = playerComponent;
                playerComponent.ManualStart(i, i == _controller.MyIndex);
            }
        }

        if (_controller.PlayerIsPlaying[_controller.MyIndex])
        {
            _playerComponents[_controller.MyIndex].transform.SetAsLastSibling();
        }
    }

    public void OnResponsePlayerResult(ResponsePacketData.RedLightGreenLightPlayerResult data)
    {
        if (data.isSuccess)
        {
            _playerComponents[data.playerIndex].OnPlayerSuccess();
        }
        else
        {
            _playerComponents[data.playerIndex].OnPlayerFail();
            CreateNewUFOBullet(_controller.PlayerProgress[data.playerIndex]);
        }
        
    }

    public void OnResponseGameResult(ResponsePacketData.RedLightGreenLightGameResult data)
    {
        for (var i = 0; i < _controller.PlayerCount; i++)
        {
            if (_controller.PlayerIsPlaying[i])
            {
                _playerComponents[i].OnPlayerFail();
                CreateNewUFOBullet(_controller.PlayerProgress[i]);
            }
        }

    }


}
