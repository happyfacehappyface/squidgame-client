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


    private void CloseAllPopups()
    {
        _popUpBlocker.SetActive(false);
        _errorPopup.SetActive(false);
    }







}
