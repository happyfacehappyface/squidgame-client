using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedLightGreenLightPlayerComponent : MonoBehaviour
{
    [SerializeField] private PlayerComponent _playerComponent;
    [SerializeField] private GameObject _meIndicator;

    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Animator _playerAnimator;


    public void ManualStart(int playerIndex, bool isMe)
    {
        _playerComponent.ManualStart(playerIndex, true);
        _meIndicator.SetActive(isMe);
    }

    public void ManualUpdate(float progress)
    {
        AdjustPlayerPosition(progress);
    }

    private void AdjustPlayerPosition(float progress)
    {
        _playerTransform.localPosition = new Vector3(progress * 1500f, 0f, 0f);
    }

    public void OnPlayerSuccess()
    {
        AdjustPlayerPosition(1.0f);
        _playerAnimator.SetTrigger("Success");
    }

    public void OnPlayerFail()
    {
        _playerAnimator.SetTrigger("Fail");
    }



}
