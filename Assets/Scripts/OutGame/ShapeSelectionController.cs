using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class ShapeSelectionController : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button circleButton;
    public Button triangleButton;
    public Button starButton;
    
    [SerializeField] private DalgonaController dalgonaController;

    private Dictionary<StrokePainter.DalgonaShapeType, int> shapeDifficulties = new Dictionary<StrokePainter.DalgonaShapeType, int>();

    void Start()
    {
        AssignRandomDifficulties();
        
        circleButton.onClick.AddListener(() => OnShapeSelected(StrokePainter.DalgonaShapeType.Circle));
        triangleButton.onClick.AddListener(() => OnShapeSelected(StrokePainter.DalgonaShapeType.SierpinskiTriangle));
        starButton.onClick.AddListener(() => OnShapeSelected(StrokePainter.DalgonaShapeType.Star));
        
        UpdateButtonLabels();

        if (dalgonaController == null)
        {
            // Find the DalgonaController in the scene if not assigned
            dalgonaController = FindObjectOfType<DalgonaController>();
        }
    }

    private void AssignRandomDifficulties()
    {
        shapeDifficulties = new Dictionary<StrokePainter.DalgonaShapeType, int>();
        List<int> levels = new List<int> { 1, 2, 3 }.OrderBy(x => System.Guid.NewGuid()).ToList();

        shapeDifficulties.Add(StrokePainter.DalgonaShapeType.Circle, levels[0]);
        shapeDifficulties.Add(StrokePainter.DalgonaShapeType.SierpinskiTriangle, levels[1]);
        shapeDifficulties.Add(StrokePainter.DalgonaShapeType.Star, levels[2]);

        Debug.Log($"난이도 할당됨: 동그라미 (Level {levels[0]}), 세모 (Level {levels[1]}), 별 (Level {levels[2]})");
    }

    private void OnShapeSelected(StrokePainter.DalgonaShapeType shape)
    {
        if (dalgonaController != null)
        {
            dalgonaController.OnShapeSelected(shape, shapeDifficulties[shape]);
        }
        else
        {
            Debug.LogError("DalgonaController가 연결되지 않았습니다!");
        }
    }

    private void UpdateButtonLabels()
    {
        SetButtonLabel(circleButton, "동그라미", shapeDifficulties[StrokePainter.DalgonaShapeType.Circle]);
        SetButtonLabel(triangleButton, "세모", shapeDifficulties[StrokePainter.DalgonaShapeType.SierpinskiTriangle]);
        SetButtonLabel(starButton, "별", shapeDifficulties[StrokePainter.DalgonaShapeType.Star]);
    }

    private void SetButtonLabel(Button button, string shapeName, int level)
    {
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = $"{shapeName}\n(난이도 {level})";
        }
    }
} 