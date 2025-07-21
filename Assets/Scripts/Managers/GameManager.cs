using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        ShapeSelection,
        DalgonaPlay
    }

    [Header("State Control")]
    public GameObject shapeSelectionUI;
    public GameObject dalgonaGame;

    // Data to pass between states
    public StrokePainter.DalgonaShapeType selectedShape;
    public int selectedDifficulty;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        // Always start the game in the shape selection state
        ChangeState(GameState.ShapeSelection);
    }

    public void ChangeState(GameState newState)
    {
        switch (newState)
        {
            case GameState.ShapeSelection:
                shapeSelectionUI.SetActive(true);
                dalgonaGame.SetActive(false);
                break;
            case GameState.DalgonaPlay:
                shapeSelectionUI.SetActive(false);
                dalgonaGame.SetActive(true);
                break;
        }
    }
} 