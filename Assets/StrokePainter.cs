using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class StrokePainter : MonoBehaviour
{
    // --- Public Enums ---
    public enum DalgonaShapeType { SierpinskiTriangle, Circle, Star, CustomFromProfile }

    // --- Serialized Fields (Visible in Inspector) ---
    [Header("Game Control")]
    [SerializeField] private DalgonaController dalgonaController;

    [Header("Shape & Difficulty")]
    public DalgonaShapeType shapeToGenerate;
    public float shapeSize = 10f;
    [Range(1, 3)]
    public int difficultyLevel = 1;
    public DalgonaShapeProfile customShapeProfile;

    [Header("Game Rules")]
    public float tolerance = 0.3f;
    [Range(0.1f, 1.0f)]
    public float completionPercentage = 0.97f;

    [Header("Setup (Prefabs & Containers)")]
    public GameObject strokePrefab;
    public Transform dalgonaShapeContainer;
    public GameObject dalgonaSegmentPrefab;
    public Transform toleranceVisualizerContainer;
    
    [Header("Visuals")]
    public Material failedMaterial;
    public Material toleranceMaterial;

    // --- Private State ---
    private List<LineRenderer> dalgonaLines = new List<LineRenderer>();
    private List<LineRenderer> toleranceLines = new List<LineRenderer>();
    private Camera mainCamera;
    private bool isGameOver, isGameWon, isDrawing;
    private LineRenderer currentStrokeRenderer;
    private List<bool[]> completionPointTrackers;
    private int totalPointsInShape = 0;
    private int coveredPointsCount = 0;
    private float _runtimeTolerance;
    private float _runtimeCompletionPercentage;
    
    // 사운드 제한을 위한 변수
    private float lastBurnSoundTime = 0f;
    private const float BURN_SOUND_INTERVAL = 0.1f; // 0.1초마다 한 번씩만 재생

    // --- Unity Methods ---
    void OnEnable()
    {
        isGameOver = false;
        isGameWon = false;
        isDrawing = false;
        currentStrokeRenderer = null;
        mainCamera = Camera.main;

        if (dalgonaController == null) dalgonaController = FindObjectOfType<DalgonaController>();

        if (dalgonaController == null || strokePrefab == null || dalgonaShapeContainer == null || dalgonaSegmentPrefab == null || toleranceVisualizerContainer == null)
        {
            Utils.LogError("One or more required prefabs/containers are not assigned! Check StrokePainter");
            this.enabled = false;
            return;
        }
        
        ClearShape();
        GenerateShape();
    }


    void Update()
    {
        if (isGameOver || isGameWon) return;
        HandleInput();
    }

    // --- Shape Generation ---
    [ContextMenu("Generate Shape")]
    private void GenerateShape()
    {
        ClearShape();
        // 1. 게임 시작 시, 항상 기본값으로 먼저 초기화합니다.
        _runtimeTolerance = tolerance;
        _runtimeCompletionPercentage = completionPercentage;

        switch (shapeToGenerate)
        {
            case DalgonaShapeType.SierpinskiTriangle: GenerateTriangleByLevel(difficultyLevel); break;
            case DalgonaShapeType.Circle: GenerateCircleByLevel(difficultyLevel); break;
            case DalgonaShapeType.Star: GenerateStarByLevel(difficultyLevel); break;
            case DalgonaShapeType.CustomFromProfile: GenerateCustomShape(); break;
        }
        
        InitializeAfterGeneration();
    }

    // --- Core Game Logic ---
    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0)) StartDrawing();
        if (Input.GetMouseButton(0) && isDrawing) ContinueDrawing();
        if (Input.GetMouseButtonUp(0) && isDrawing) StopDrawing();
    }

    private void StartDrawing()
    {
        Vector3 mousePos = GetMouseWorldPosition();
        GameObject strokeGO = Instantiate(strokePrefab, Vector3.zero, Quaternion.identity, this.transform);
        currentStrokeRenderer = strokeGO.GetComponent<LineRenderer>();
        currentStrokeRenderer.positionCount = 0;
        if (dalgonaLines.Count > 0) currentStrokeRenderer.sortingOrder = dalgonaLines[0].sortingOrder + 1;
        AddPointToLine(mousePos);
        if (!IsPointOnPath(mousePos)) { TriggerGameOver(); return; }
        CheckShapeCoverage(mousePos);
        isDrawing = true;
    }

    private void ContinueDrawing()
    {
        // 사운드 재생 제한: 0.1초마다 한 번씩만 재생
        if (Time.time - lastBurnSoundTime >= BURN_SOUND_INTERVAL)
        {
            SoundManager.Instance.PlaySfxBurn(0.0f);
            lastBurnSoundTime = Time.time;
        }
        
        Vector3 mousePos = GetMouseWorldPosition();
        AddPointToLine(mousePos);
        if (!IsPointOnPath(mousePos)) { TriggerGameOver(); }
        else { CheckShapeCoverage(mousePos); }
    }

    private void StopDrawing()
    {
        isDrawing = false;
        currentStrokeRenderer = null;
        UpdateProgress();
    }

    private void TriggerGameOver()
    {
        if (isGameOver || isGameWon) return;
        isGameOver = true;
        if(failedMaterial != null && currentStrokeRenderer != null) currentStrokeRenderer.material = failedMaterial;
        if (dalgonaController != null) dalgonaController.OnFail();
    }

    private void TriggerGameWon()
    {
        if (isGameOver || isGameWon) return;
        isGameWon = true;
        if (dalgonaController != null) dalgonaController.OnSuccess();
    }
    
    // ... The rest of the file contains all the helper methods for drawing shapes ...
    // ... This part is long but does not need to be changed. ...
    private void InitializeAfterGeneration() { completionPointTrackers = new List<bool[]>(); totalPointsInShape = 0; coveredPointsCount = 0; foreach (LineRenderer lr in dalgonaLines) { completionPointTrackers.Add(new bool[lr.positionCount]); totalPointsInShape += lr.positionCount; } SetupToleranceVisualizers(); }
    private void GenerateTriangleByLevel(int level) { float halfSize = shapeSize / 2f; float height = halfSize * Mathf.Sqrt(3); Vector3 p1 = new Vector3(0, height / 2f, 0); Vector3 p2 = new Vector3(-halfSize, -height / 2f, 0); Vector3 p3 = new Vector3(halfSize, -height / 2f, 0); int depth; switch (level) { case 1: depth = 0; break; case 2: depth = 1; break; case 3: depth = 2; break; default: depth = 0; break; } SierpinskiRecursive(p1, p2, p3, depth); }
    private void GenerateCircleByLevel(int level) { const int segments = 60; float radius = shapeSize / 2f; switch (level) { case 1: CreateCircleLine(Vector3.zero, radius, segments); break; case 2: float vennRadius = shapeSize / 2.9f; float offset = vennRadius * 0.8f; CreateCircleLine(new Vector3(0, offset, 0), vennRadius, segments); CreateCircleLine(new Vector3(-offset, -offset * 0.5f, 0), vennRadius, segments); CreateCircleLine(new Vector3(offset, -offset * 0.5f, 0), vennRadius, segments); break; case 3: GenerateSpiral(); break; } }
    private void GenerateStarByLevel(int level)
    {
        float outerRadius = shapeSize / 2f;
        switch (level)
        {
            case 1:
                // 기본값을 사용
                CreateStarLine(5, outerRadius, outerRadius * 0.5f, Vector3.zero, 90f);
                break;
            case 2:
                // 2. 이 경우에만 Completion을 0.91로 덮어씁니다.
                _runtimeCompletionPercentage = 0.91f;
                GenerateShootingStar();
                break;
            case 3:
                // 3. 이 경우에만 Tolerance를 0.17로 덮어쓰고,
                _runtimeTolerance = 0.17f;
                // DalgonaShapeProfile에 지정된 커스텀 모양을 생성합니다.
                GenerateCustomShape();
                break;
        }
    }

    private void GenerateStarbucksLogo() { float outerRadius = shapeSize / 2f; CreateCircleLine(Vector3.zero, outerRadius, 60); float starRadius = shapeSize * 0.12f; Vector3 starCenter = new Vector3(0, shapeSize * 0.25f, 0); CreateStarLine(5, starRadius, starRadius * 0.5f, starCenter, 90f); List<Vector3> rightFinPoints = new List<Vector3> { new Vector3(shapeSize * 0.15f, -shapeSize * 0.15f, 0), new Vector3(shapeSize * 0.4f, -shapeSize * 0.1f, 0), new Vector3(shapeSize * 0.35f, -shapeSize * 0.25f, 0), new Vector3(shapeSize * 0.45f, -shapeSize * 0.3f, 0), new Vector3(shapeSize * 0.2f, -shapeSize * 0.3f, 0) }; CreateLine(rightFinPoints, true); List<Vector3> leftFinPoints = new List<Vector3>(); foreach (var point in rightFinPoints) { leftFinPoints.Add(new Vector3(-point.x, point.y, point.z)); } CreateLine(leftFinPoints, true); }
    private void CreateCircleLine(Vector3 center, float radius, int segments) { GameObject segmentGO = Instantiate(dalgonaSegmentPrefab, dalgonaShapeContainer); LineRenderer lr = segmentGO.GetComponent<LineRenderer>(); lr.useWorldSpace = true; lr.positionCount = segments + 1; lr.loop = true; float angleStep = 360f / segments; for (int i = 0; i <= segments; i++) { float angle = i * angleStep * Mathf.Deg2Rad; float x = center.x + Mathf.Cos(angle) * radius; float y = center.y + Mathf.Sin(angle) * radius; lr.SetPosition(i, new Vector3(x, y, 0)); } dalgonaLines.Add(lr); }
    private void CreateBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, int segments) { List<Vector3> points = new List<Vector3>(); for (int i = 0; i <= segments; i++) { points.Add(CalculateQuadraticBezierPoint((float)i/segments, p0, p1, p2)); } CreateLine(points, false); }
    private void CreateStarLine(int points, float outerRadius, float innerRadius, Vector3 center, float rotationOffsetDegrees = 0f) { GameObject segmentGO = Instantiate(dalgonaSegmentPrefab, dalgonaShapeContainer); LineRenderer lr = segmentGO.GetComponent<LineRenderer>(); lr.useWorldSpace = true; int totalVertices = points * 2; lr.positionCount = totalVertices + 1; lr.loop = true; float angleStep = 360f / totalVertices; float rotationOffsetRad = rotationOffsetDegrees * Mathf.Deg2Rad; for (int i = 0; i <= totalVertices; i++) { float radius = (i % 2 == 0) ? outerRadius : innerRadius; float currentAngle = rotationOffsetRad + (i * angleStep * Mathf.Deg2Rad); float x = center.x + Mathf.Cos(currentAngle) * radius; float y = center.y + Mathf.Sin(currentAngle) * radius; lr.SetPosition(i, new Vector3(x, y, 0)); } dalgonaLines.Add(lr); }
    private void GenerateShootingStar() { float starSize = shapeSize / 1.5f; float outerRadius = starSize / 2f; float innerRadius = outerRadius * 0.5f; Vector3 starCenter = new Vector3(-shapeSize / 2.5f, 0, 0); float starRotationDegrees = 90f; CreateStarLine(5, outerRadius, innerRadius, starCenter, starRotationDegrees); float rotationRad = starRotationDegrees * Mathf.Deg2Rad; float upperTailStartAngle = rotationRad - (36f * Mathf.Deg2Rad); Vector3 upperTailStart = starCenter + new Vector3(Mathf.Cos(upperTailStartAngle) * innerRadius, Mathf.Sin(upperTailStartAngle) * innerRadius, 0); float lowerTailStartAngle = rotationRad - (108f * Mathf.Deg2Rad); Vector3 lowerTailStart = starCenter + new Vector3(Mathf.Cos(lowerTailStartAngle) * innerRadius, Mathf.Sin(lowerTailStartAngle) * innerRadius, 0); Vector3 midTailStart = (upperTailStart + lowerTailStart) / 2f; Vector3 tailControlOffset = new Vector3(shapeSize * 0.4f, shapeSize * 0.1f, 0); Vector3 tailEndOffset = new Vector3(shapeSize * 0.8f, -shapeSize * 0.1f, 0); Vector3 midTailControl = midTailStart + tailControlOffset; Vector3 midTailEnd = midTailStart + tailEndOffset; int tailSegments = 25; float ribbonWidth = shapeSize / 8f; List<Vector3> upperCurve = new List<Vector3>(); List<Vector3> middleCurve = new List<Vector3>(); List<Vector3> lowerCurve = new List<Vector3>(); for (int i = 0; i <= tailSegments; i++) { float t = (float)i / tailSegments; Vector3 midPoint = CalculateQuadraticBezierPoint(t, midTailStart, midTailControl, midTailEnd); middleCurve.Add(midPoint); Vector3 derivative = 2 * (1 - t) * (midTailControl - midTailStart) + 2 * t * (midTailEnd - midTailControl); Vector3 normal = Vector3.Cross(derivative, Vector3.forward).normalized; upperCurve.Add(midPoint + normal * ribbonWidth); lowerCurve.Add(midPoint - normal * ribbonWidth); } int pointsToSkip = 4; List<Vector3> shorterMidCurve = (middleCurve.Count > pointsToSkip) ? middleCurve.GetRange(pointsToSkip, middleCurve.Count - pointsToSkip) : middleCurve; CreateLine(shorterMidCurve, false); List<Vector3> outlinePoints = new List<Vector3>(); outlinePoints.AddRange(upperCurve); Vector3 upperEnd = upperCurve.Last(); Vector3 lowerEnd = lowerCurve.Last(); Vector3 direction = (midTailEnd - midTailControl).normalized; float indentDepth = ribbonWidth * 1.2f; float spikeLength = ribbonWidth * 0.5f; Vector3 point_upper_quarter = Vector3.Lerp(upperEnd, lowerEnd, 0.25f); Vector3 point_lower_three_quarters = Vector3.Lerp(upperEnd, lowerEnd, 0.75f); Vector3 centerPoint = (upperEnd + lowerEnd) / 2f; Vector3 valley1 = point_upper_quarter - direction * indentDepth; Vector3 midSpike = centerPoint + direction * spikeLength; Vector3 valley2 = point_lower_three_quarters - direction * indentDepth; const int subdivisions = 1; AddSubdividedSegment(outlinePoints, upperEnd, valley1, subdivisions); AddSubdividedSegment(outlinePoints, valley1, midSpike, subdivisions); AddSubdividedSegment(outlinePoints, midSpike, valley2, subdivisions); AddSubdividedSegment(outlinePoints, valley2, lowerEnd, subdivisions); lowerCurve.Reverse(); outlinePoints.AddRange(lowerCurve); Vector3 startUpperCorner = upperCurve.First(); Vector3 startLowerCorner = lowerCurve.Last(); Vector3 startCenter = (startUpperCorner + startLowerCorner) / 2f; Vector3 directionToStar = (starCenter - startCenter).normalized; float startCapDepth = ribbonWidth * 1.5f; Vector3 startValley = startCenter - directionToStar * startCapDepth; outlinePoints.Add(startValley); CreateLine(outlinePoints, true); }
    private void AddSubdividedSegment(List<Vector3> points, Vector3 start, Vector3 end, int divisions) { for (int i = 1; i <= divisions; i++) { points.Add(Vector3.Lerp(start, end, (float)i / (divisions + 1))); } points.Add(end); }
    private void CreateLine(List<Vector3> points, bool loop) { GameObject segmentGO = Instantiate(dalgonaSegmentPrefab, dalgonaShapeContainer); LineRenderer lr = segmentGO.GetComponent<LineRenderer>(); lr.useWorldSpace = true; lr.positionCount = points.Count; lr.SetPositions(points.ToArray()); lr.loop = loop; dalgonaLines.Add(lr); }
    private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2) { float u = 1 - t; float tt = t * t; float uu = u * u; Vector3 p = uu * p0; p += 2 * u * t * p1; p += tt * p2; return p; }
    private void GenerateSpiral() { const float startRadiusFactor = 0.05f; const float endRadiusFactor = 0.5f; const int segments = 150; const int turns = 5; GameObject segmentGO = Instantiate(dalgonaSegmentPrefab, dalgonaShapeContainer); LineRenderer lr = segmentGO.GetComponent<LineRenderer>(); lr.useWorldSpace = true; lr.positionCount = segments; lr.loop = false; float totalAngle = turns * 360f; float spiralStartRadius = shapeSize * startRadiusFactor; float spiralEndRadius = shapeSize * endRadiusFactor; for (int i = 0; i < segments; i++) { float t = (float)i / (segments - 1); float currentAngle = t * totalAngle * Mathf.Deg2Rad; float currentRadius = Mathf.Lerp(spiralStartRadius, spiralEndRadius, t); float x = Mathf.Cos(currentAngle) * currentRadius; float y = Mathf.Sin(currentAngle) * currentRadius; lr.SetPosition(i, new Vector3(x, y, 0)); } dalgonaLines.Add(lr); }
    private void GenerateCustomShape() { if (customShapeProfile == null || customShapeProfile.paths.Count == 0) { Utils.LogError("Custom Shape Profile is not assigned or has no paths to draw."); return; } foreach (var path in customShapeProfile.paths) { if (path.points.Count >= 2) { CreateLine(path.points, path.closeShape); } } }
    private void SierpinskiRecursive(Vector3 p1, Vector3 p2, Vector3 p3, int depth) { if (depth <= 0) { AddTriangle(p1, p2, p3); return; } Vector3 m12 = (p1 + p2) / 2; Vector3 m23 = (p2 + p3) / 2; Vector3 m31 = (p3 + p1) / 2; SierpinskiRecursive(p1, m12, m31, depth - 1); SierpinskiRecursive(p2, m23, m12, depth - 1); SierpinskiRecursive(p3, m31, m23, depth - 1); }
    private void AddTriangle(Vector3 p1, Vector3 p2, Vector3 p3) { GameObject segmentGO = Instantiate(dalgonaSegmentPrefab, dalgonaShapeContainer); LineRenderer lr = segmentGO.GetComponent<LineRenderer>(); lr.useWorldSpace = true; List<Vector3> points = new List<Vector3>(); AddLine(points, p1, p2); points.RemoveAt(points.Count - 1); AddLine(points, p2, p3); points.RemoveAt(points.Count - 1); AddLine(points, p3, p1); lr.positionCount = points.Count; lr.SetPositions(points.ToArray()); dalgonaLines.Add(lr); }
    private void AddLine(List<Vector3> points, Vector3 start, Vector3 end) { const int subdivisions = 3; points.Add(start); for (int i = 1; i <= subdivisions; i++) { float t = (float)i / (subdivisions + 1); points.Add(Vector3.Lerp(start, end, t)); } points.Add(end); }
    [ContextMenu("Clear Shape")] private void ClearShape() { if (Application.isPlaying) { foreach (Transform child in dalgonaShapeContainer) Destroy(child.gameObject); foreach (Transform child in toleranceVisualizerContainer) Destroy(child.gameObject); } else { while (dalgonaShapeContainer.childCount > 0) DestroyImmediate(dalgonaShapeContainer.GetChild(0).gameObject); while (toleranceVisualizerContainer.childCount > 0) DestroyImmediate(toleranceVisualizerContainer.GetChild(0).gameObject); } dalgonaLines.Clear(); toleranceLines.Clear(); }
    private void AddPointToLine(Vector3 position) { if (currentStrokeRenderer == null) return; if (currentStrokeRenderer.positionCount > 0 && Vector3.Distance(currentStrokeRenderer.GetPosition(currentStrokeRenderer.positionCount - 1), position) < 0.01f) return; currentStrokeRenderer.positionCount++; currentStrokeRenderer.SetPosition(currentStrokeRenderer.positionCount - 1, position); }
    private Vector3 GetMouseWorldPosition() { Vector3 mousePos = Input.mousePosition; mousePos.z = mainCamera.nearClipPlane + 10; return mainCamera.ScreenToWorldPoint(mousePos); }
    private void UpdateProgress() { float progress = (float)coveredPointsCount / totalPointsInShape; if (progress >= _runtimeCompletionPercentage) { TriggerGameWon(); } }
    private void CheckShapeCoverage(Vector3 drawnPoint) { Vector2 drawnPoint2D = new Vector2(drawnPoint.x, drawnPoint.y); coveredPointsCount = 0; for (int i = 0; i < dalgonaLines.Count; i++) { LineRenderer line = dalgonaLines[i]; bool[] tracker = completionPointTrackers[i]; for (int j = 0; j < line.positionCount; j++) { if (!tracker[j]) { Vector3 shapePoint3D = line.useWorldSpace ? line.GetPosition(j) : line.transform.TransformPoint(line.GetPosition(j)); if (Vector2.Distance(drawnPoint2D, new Vector2(shapePoint3D.x, shapePoint3D.y)) <= _runtimeTolerance) { tracker[j] = true; } } if (tracker[j]) coveredPointsCount++; } } }
    private bool IsPointOnPath(Vector3 point) { if (dalgonaLines.Count == 0) { return false; } float minDistanceOverall = float.MaxValue; foreach (var line in dalgonaLines) { for (int i = 0; i < line.positionCount - 1; i++) { Vector3 p1_3D = line.useWorldSpace ? line.GetPosition(i) : line.transform.TransformPoint(line.GetPosition(i)); Vector3 p2_3D = line.useWorldSpace ? line.GetPosition(i + 1) : line.transform.TransformPoint(line.GetPosition(i + 1)); float distance = DistancePointToLineSegment(new Vector2(point.x, point.y), new Vector2(p1_3D.x, p1_3D.y), new Vector2(p2_3D.x, p2_3D.y)); if (distance < minDistanceOverall) { minDistanceOverall = distance; } } if (line.loop && line.positionCount > 1) { Vector3 p1_3D = line.useWorldSpace ? line.GetPosition(line.positionCount - 1) : line.transform.TransformPoint(line.GetPosition(line.positionCount - 1)); Vector3 p2_3D = line.useWorldSpace ? line.GetPosition(0) : line.transform.TransformPoint(line.GetPosition(0)); float distance = DistancePointToLineSegment(new Vector2(point.x, point.y), new Vector2(p1_3D.x, p1_3D.y), new Vector2(p2_3D.x, p2_3D.y)); if (distance < minDistanceOverall) { minDistanceOverall = distance; } } } bool isOnPath = minDistanceOverall <= _runtimeTolerance; if (!isOnPath) { Utils.Log($"[DEBUG] 실패! 경로를 벗어났습니다. 마우스 위치: {point}, 가장 가까운 선분과의 거리: {minDistanceOverall}, 허용 오차: {_runtimeTolerance}"); } return isOnPath; }
    public static float DistancePointToLineSegment(Vector2 point, Vector2 p1, Vector2 p2) { if (p1 == p2) return Vector2.Distance(point, p1); Vector2 lineDirection = p2 - p1; float lineLengthSqr = lineDirection.sqrMagnitude; Vector2 pointVector = point - p1; float t = Mathf.Clamp01(Vector2.Dot(pointVector, lineDirection) / lineLengthSqr); Vector2 projection = p1 + t * lineDirection; return Vector2.Distance(point, projection); }
    private void SetupToleranceVisualizers() { ClearToleranceVisualizers(); foreach (var dalgonaLine in dalgonaLines) { GameObject visualizerGO = Instantiate(dalgonaSegmentPrefab, toleranceVisualizerContainer); LineRenderer visualizerLR = visualizerGO.GetComponent<LineRenderer>(); if (toleranceMaterial != null) { visualizerLR.material = toleranceMaterial; } visualizerLR.useWorldSpace = dalgonaLine.useWorldSpace; if (!visualizerLR.useWorldSpace) { visualizerGO.transform.SetParent(dalgonaLine.transform, false); } Vector3[] points = new Vector3[dalgonaLine.positionCount]; dalgonaLine.GetPositions(points); visualizerLR.positionCount = dalgonaLine.positionCount; visualizerLR.SetPositions(points); visualizerLR.startWidth = _runtimeTolerance * 2f; visualizerLR.endWidth = _runtimeTolerance * 2f; visualizerLR.loop = dalgonaLine.loop; visualizerLR.sortingOrder = dalgonaLine.sortingOrder - 1; toleranceLines.Add(visualizerLR); } }
    private void ClearToleranceVisualizers() { if (Application.isPlaying) { foreach (Transform child in toleranceVisualizerContainer) Destroy(child.gameObject); } else { while (toleranceVisualizerContainer.childCount > 0) DestroyImmediate(toleranceVisualizerContainer.GetChild(0).gameObject); } toleranceLines.Clear(); }
}
