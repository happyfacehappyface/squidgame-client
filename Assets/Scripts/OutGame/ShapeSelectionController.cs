using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System;
using System.Collections;

public class ShapeSelectionController : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button circleButton;
    public Button triangleButton;
    public Button starButton;

    [SerializeField] private Animator _circleButtonAnimator;
    [SerializeField] private Animator _triangleButtonAnimator;
    [SerializeField] private Animator _starButtonAnimator;

    [SerializeField] private Transform _circleButtonTransform;
    [SerializeField] private Transform _triangleButtonTransform;
    [SerializeField] private Transform _starButtonTransform;
    
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
            switch (shape)
            {
                case StrokePainter.DalgonaShapeType.Circle:
                    _circleButtonAnimator.SetTrigger("Spin");
                    _triangleButtonAnimator.SetTrigger("Shrink");
                    _starButtonAnimator.SetTrigger("Shrink");
                    StartCoroutine(CO_AdjustButton(_circleButtonTransform));
                    break;
                case StrokePainter.DalgonaShapeType.SierpinskiTriangle:
                    _triangleButtonAnimator.SetTrigger("Spin");
                    _circleButtonAnimator.SetTrigger("Shrink");
                    _starButtonAnimator.SetTrigger("Shrink");
                    StartCoroutine(CO_AdjustButton(_triangleButtonTransform));
                    break;
                case StrokePainter.DalgonaShapeType.Star:
                    _starButtonAnimator.SetTrigger("Spin");
                    _circleButtonAnimator.SetTrigger("Shrink");
                    _triangleButtonAnimator.SetTrigger("Shrink");
                    StartCoroutine(CO_AdjustButton(_starButtonTransform));
                    break;
            }

            dalgonaController.OnShapeSelected(shape, shapeDifficulties[shape]);
            SoundManager.Instance.PlaySfxRock(0.0f);
        }
        else
        {
            Debug.LogError("DalgonaController가 연결되지 않았습니다!");
        }
    }

    private IEnumerator CO_AdjustButton(Transform button)
    {

        float progress = 0.0f;
        float duration = 0.8f;

        Vector3 originTransform = button.localPosition;
        Vector3 destTransform = new Vector3(0f, 0f, 0f);
        while (progress < 1.0f)
        {
            progress += Time.deltaTime / duration;
            button.localPosition = Vector3.Lerp(originTransform, destTransform, progress);
            yield return null;
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