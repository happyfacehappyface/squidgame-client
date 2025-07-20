using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OutGamePopupHandler : MonoBehaviour
{
    [SerializeField] private GameObject _popUpBlocker;
    [SerializeField] private GameObject _errorPopup;
    [SerializeField] private TextMeshProUGUI _errorTitle;
    [SerializeField] private TextMeshProUGUI _errorDescription;

    private OutGameController _controller;


    public void ManualStart(OutGameController controller)
    {
        _controller = controller;
    }

    public void OnClickClosePopupButton()
    {
        SoundManager.Instance.PlaySfxButtonClick(0.0f);

        CloseAllPopups();
    }

    private void CloseAllPopups()
    {
        _popUpBlocker.SetActive(false);
        _errorPopup.SetActive(false);
    }

    public void OpenErrorPopup(string title, string description)
    {
        CloseAllPopups();
        _errorPopup.SetActive(true);

        _errorTitle.text = title;
        _errorDescription.text = description;
    }







}
