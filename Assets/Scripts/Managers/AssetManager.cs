using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetManager : MonoBehaviour
{
    public static AssetManager Instance;
    private bool _isReady = false;

    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _isReady = true;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    public bool IsReady()
    {
        return _isReady;
    }

    public Color GetBodyColorFromPlayerIndex(int playerIndex)
    {
        switch (playerIndex)
        {
            case 0:
                return Color.red;
            case 1:
                return Color.blue;
            case 2:
                return Color.green;
            case 3:
                return Color.yellow;
            default:
                return Color.white;
        }
    }
}
