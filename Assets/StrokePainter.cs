using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A Dalgona game controller that can generate Sierpinski triangle shapes using a recursive method.
/// This version is known to produce visual artifacts (diagonal lines) as part of its generation.
/// </summary>
public class StrokePainter : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Prefab for the line a player draws. Must have a LineRenderer.")]
    public GameObject strokePrefab;
    [Tooltip("The single LineRenderer that will display the generated shape.")]
    public LineRenderer dalgonaShape;

    [Header("Generation")]
    [Tooltip("The overall size of the generated triangle.")]
    public float sierpinskiSize = 10f;
    [Tooltip("The recursion depth for the Sierpinski triangle.")]
    [Range(0, 5)]
    public int sierpinskiDepth = 2;
    [Tooltip("How many points to add along each edge for collision precision.")]
    [Range(0, 20)]
    public int edgeSubdivisions = 5;

    [Header("Game Rules")]
    [Tooltip("Allowed distance from the shape's path.")]
    public float tolerance = 0.1f;
    [Tooltip("The percentage of the shape that must be traced for success.")]
    [Range(0.1f, 1f)]
    public float completionPercentage = 0.9f;

    [Header("Visuals")]
    [Tooltip("Material to apply to the line when the game is over.")]
    public Material failedMaterial;
    [Tooltip("The LineRenderer used to visualize the tolerance zone.")]
    public LineRenderer toleranceVisualizer;

    // --- Private members ---
    private Camera mainCamera;
    private bool isGameOver, isGameWon, isDrawing;
    private LineRenderer currentStrokeRenderer;
    private bool[] shapePointsCovered;
    private int coveredPointsCount;

    void Start()
    {
        mainCamera = Camera.main;
        if (dalgonaShape == null || strokePrefab == null)
        {
            Debug.LogError("Dalgona Shape or Stroke Prefab is not assigned!");
            this.enabled = false;
        }
        else
        {
            ResetCompletionTracking();
            SetupToleranceVisualizer();
        }
    }

    void Update()
    {
        if (isGameOver || isGameWon) return;
        if (Input.GetMouseButtonDown(0)) StartDrawing();
        if (Input.GetMouseButton(0) && isDrawing) ContinueDrawing();
        if (Input.GetMouseButtonUp(0) && isDrawing) StopDrawing();
    }
    
    [ContextMenu("Generate Sierpinski Triangle")]
    private void GenerateSierpinskiTriangle()
    {
        if (dalgonaShape == null) { Debug.LogError("Dalgona Shape is not assigned!", this); return; }

        List<Vector3> points = new List<Vector3>();
        float halfSize = sierpinskiSize / 2f;
        float height = halfSize * Mathf.Sqrt(3);

        Vector3 p1 = new Vector3(0, height / 2f, 0);
        Vector3 p2 = new Vector3(-halfSize, -height / 2f, 0);
        Vector3 p3 = new Vector3(halfSize, -height / 2f, 0);

        SierpinskiRecursive(points, p1, p2, p3, sierpinskiDepth);

        dalgonaShape.positionCount = points.Count;
        dalgonaShape.SetPositions(points.ToArray());

        ResetCompletionTracking();
        SetupToleranceVisualizer();
        Debug.Log($"Generated Sierpinski triangle with depth {sierpinskiDepth}, containing {points.Count} points.");
    }

    private void SierpinskiRecursive(List<Vector3> points, Vector3 p1, Vector3 p2, Vector3 p3, int depth)
    {
        if (depth <= 0)
        {
            AddLine(points, p1, p2);
            AddLine(points, p2, p3);
            AddLine(points, p3, p1);
            points.Add(p1);
            return;
        }

        Vector3 m12 = (p1 + p2) / 2;
        Vector3 m23 = (p2 + p3) / 2;
        Vector3 m31 = (p3 + p1) / 2;

        SierpinskiRecursive(points, p1, m12, m31, depth - 1);
        SierpinskiRecursive(points, m31, m23, p3, depth - 1);
        SierpinskiRecursive(points, m12, p2, m23, depth - 1);
    }

    private void AddLine(List<Vector3> points, Vector3 start, Vector3 end)
    {
        points.Add(start);
        for (int i = 1; i <= edgeSubdivisions; i++)
        {
            float t = (float)i / (edgeSubdivisions + 1);
            points.Add(Vector3.Lerp(start, end, t));
        }
    }

    [ContextMenu("Clear Shape")]
    private void ClearShape()
    {
        if (dalgonaShape == null) return;
        dalgonaShape.positionCount = 0;
        ResetCompletionTracking();
        SetupToleranceVisualizer();
    }
    
    // --- Core Gameplay & Helper Logic ---

    private void ResetCompletionTracking()
    {
        coveredPointsCount = 0;
        if (dalgonaShape != null && dalgonaShape.positionCount > 0)
            shapePointsCovered = new bool[dalgonaShape.positionCount];
        else
            shapePointsCovered = new bool[0];
    }
    private void StartDrawing()
    {
        Vector3 mousePos = GetMouseWorldPosition();
        GameObject strokeGO = Instantiate(strokePrefab, Vector3.zero, Quaternion.identity, this.transform);
        currentStrokeRenderer = strokeGO.GetComponent<LineRenderer>();
        currentStrokeRenderer.positionCount = 0;
        if (dalgonaShape != null)
            currentStrokeRenderer.sortingOrder = dalgonaShape.sortingOrder + 1;
        AddPointToLine(mousePos);
        if (!IsPointOnPath(mousePos)) { TriggerGameOver(); return; }
        else { CheckShapeCoverage(mousePos); }
        isDrawing = true;
    }
    private void ContinueDrawing()
    {
        Vector3 mousePos = GetMouseWorldPosition();
        AddPointToLine(mousePos);
        if (!IsPointOnPath(mousePos)) { TriggerGameOver(); }
        else { CheckShapeCoverage(mousePos); }
    }
    private void StopDrawing()
    {
        isDrawing = false;
        currentStrokeRenderer = null;
        float currentCoverage = 0f;
        if (dalgonaShape.positionCount > 0)
            currentCoverage = (float)coveredPointsCount / dalgonaShape.positionCount;
        Debug.Log($"현재 총 완성도 (Total Coverage): {currentCoverage * 100:F1}%");
        if (currentCoverage >= completionPercentage)
            TriggerGameWon();
    }
    private void TriggerGameOver() { Debug.LogError("실패! 경로를 벗어났습니다. (Game Over! You strayed from the path.)"); isGameOver = true; isDrawing = false; if (currentStrokeRenderer != null && failedMaterial != null) currentStrokeRenderer.material = failedMaterial; }
    private void TriggerGameWon() { Debug.Log("성공! 모양을 완성했습니다! (Success! You completed the shape!)"); isGameWon = true; this.enabled = false; }
    private void AddPointToLine(Vector3 position) { if (currentStrokeRenderer == null) return; if (currentStrokeRenderer.positionCount > 0 && Vector3.Distance(currentStrokeRenderer.GetPosition(currentStrokeRenderer.positionCount - 1), position) < 0.01f) return; currentStrokeRenderer.positionCount++; currentStrokeRenderer.SetPosition(currentStrokeRenderer.positionCount - 1, position); }
    private void CheckShapeCoverage(Vector3 drawnPoint) { Vector2 drawnPoint2D = new Vector2(drawnPoint.x, drawnPoint.y); for (int i = 0; i < dalgonaShape.positionCount; i++) { if (!shapePointsCovered[i]) { Vector3 shapePoint3D = dalgonaShape.useWorldSpace ? dalgonaShape.GetPosition(i) : dalgonaShape.transform.TransformPoint(dalgonaShape.GetPosition(i)); Vector2 shapePoint2D = new Vector2(shapePoint3D.x, shapePoint3D.y); if (Vector2.Distance(drawnPoint2D, shapePoint2D) <= tolerance) { shapePointsCovered[i] = true; coveredPointsCount++; } } } }
    private Vector3 GetMouseWorldPosition() { Vector3 mousePos = Input.mousePosition; mousePos.z = mainCamera.nearClipPlane + 10; return mainCamera.ScreenToWorldPoint(mousePos); }
    private bool IsPointOnPath(Vector3 point) { float minDistance = float.MaxValue; Vector2 point2D = new Vector2(point.x, point.y); for (int i = 0; i < dalgonaShape.positionCount - 1; i++) { Vector3 p1_3D = dalgonaShape.useWorldSpace ? dalgonaShape.GetPosition(i) : dalgonaShape.transform.TransformPoint(dalgonaShape.GetPosition(i)); Vector3 p2_3D = dalgonaShape.useWorldSpace ? dalgonaShape.GetPosition(i + 1) : dalgonaShape.transform.TransformPoint(dalgonaShape.GetPosition(i + 1)); Vector2 p1 = new Vector2(p1_3D.x, p1_3D.y); Vector2 p2 = new Vector2(p2_3D.x, p2_3D.y); float distance = DistancePointToLineSegment(point2D, p1, p2); if (distance < minDistance) minDistance = distance; } if (dalgonaShape.loop && dalgonaShape.positionCount > 1) { Vector3 p_last_3D = dalgonaShape.useWorldSpace ? dalgonaShape.GetPosition(dalgonaShape.positionCount - 1) : dalgonaShape.transform.TransformPoint(dalgonaShape.GetPosition(dalgonaShape.positionCount - 1)); Vector3 p_first_3D = dalgonaShape.useWorldSpace ? dalgonaShape.GetPosition(0) : dalgonaShape.transform.TransformPoint(dalgonaShape.GetPosition(0)); Vector2 p_last = new Vector2(p_last_3D.x, p_last_3D.y); Vector2 p_first = new Vector2(p_first_3D.x, p_first_3D.y); float distance = DistancePointToLineSegment(point2D, p_last, p_first); if (distance < minDistance) minDistance = distance; } return minDistance <= tolerance; }
    public static float DistancePointToLineSegment(Vector2 point, Vector2 p1, Vector2 p2) { if (p1 == p2) return Vector2.Distance(point, p1); Vector2 lineDirection = p2 - p1; float lineLengthSqr = lineDirection.sqrMagnitude; Vector2 pointVector = point - p1; float t = Mathf.Clamp01(Vector2.Dot(pointVector, lineDirection) / lineLengthSqr); Vector2 projection = p1 + t * lineDirection; return Vector2.Distance(point, projection); }
    private void SetupToleranceVisualizer() { if (toleranceVisualizer == null) return; toleranceVisualizer.useWorldSpace = dalgonaShape.useWorldSpace; if (!toleranceVisualizer.useWorldSpace) { toleranceVisualizer.transform.position = dalgonaShape.transform.position; toleranceVisualizer.transform.rotation = dalgonaShape.transform.rotation; toleranceVisualizer.transform.localScale = dalgonaShape.transform.localScale; } Vector3[] points = new Vector3[dalgonaShape.positionCount]; dalgonaShape.GetPositions(points); toleranceVisualizer.positionCount = dalgonaShape.positionCount; toleranceVisualizer.SetPositions(points); toleranceVisualizer.startWidth = tolerance * 2f; toleranceVisualizer.endWidth = tolerance * 2f; toleranceVisualizer.loop = dalgonaShape.loop; toleranceVisualizer.sortingOrder = dalgonaShape.sortingOrder - 1; }
}
