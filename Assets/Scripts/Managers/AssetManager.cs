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
                return new Color(1f, 0f, 0f); // Red
            case 1:
                return new Color(0f, 0f, 1f); // Blue
            case 2:
                return new Color(0f, 1f, 0f); // Green
            case 3:
                return new Color(1f, 1f, 0f); // Yellow
            case 4:
                return new Color(1f, 0f, 1f); // Magenta
            case 5:
                return new Color(0f, 1f, 1f); // Cyan
            case 6:
                return new Color(1f, 0.5f, 0f); // Orange
            case 7:
                return new Color(0.5f, 0f, 0.5f); // Purple
            case 8:
                return new Color(1f, 0.8f, 0.8f); // Pink
            case 9:
                return new Color(0.5f, 1f, 0f); // Lime
            case 10:
                return new Color(0f, 0.5f, 0.5f); // Teal
            case 11:
                return new Color(0.3f, 0f, 0.5f); // Indigo
            case 12:
                return new Color(1f, 0.5f, 0.3f); // Coral
            case 13:
                return new Color(0.3f, 0.8f, 0.8f); // Turquoise
            case 14:
                return new Color(0.5f, 0.5f, 0f); // Olive
            case 15:
                return new Color(0.5f, 0f, 0f); // Maroon
            case 16:
                return new Color(0f, 0f, 0.5f); // Navy
            case 17:
                return new Color(0f, 1f, 1f); // Aqua
            case 18:
                return new Color(1f, 0f, 1f); // Fuchsia
            case 19:
                return new Color(0.5f, 1f, 0f); // Chartreuse
            case 20:
                return new Color(0.86f, 0.08f, 0.24f); // Crimson
            case 21:
                return new Color(0.25f, 0.41f, 0.88f); // RoyalBlue
            case 22:
                return new Color(0f, 1f, 0.5f); // SpringGreen
            case 23:
                return new Color(1f, 0.84f, 0f); // Gold
            case 24:
                return new Color(0.85f, 0.44f, 0.84f); // Orchid
            case 25:
                return new Color(0.94f, 0.9f, 0.55f); // Khaki
            case 26:
                return new Color(0.42f, 0.35f, 0.8f); // SlateBlue
            case 27:
                return new Color(0.18f, 0.55f, 0.34f); // SeaGreen
            case 28:
                return new Color(1f, 0.39f, 0.28f); // Tomato
            case 29:
                return new Color(0.53f, 0.81f, 0.98f); // SkyBlue
            case 30:
                return new Color(0.6f, 0.8f, 0.2f); // YellowGreen
            case 31:
                return new Color(1f, 0.08f, 0.58f); // DeepPink
            case 32:
                return new Color(0.6f, 0.4f, 0.2f); // Brown
            
            default:
                return new Color(0.5f, 0.5f, 0.5f); // Gray
        }
    }
}
