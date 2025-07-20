using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A Dalgona game controller that uses multiple LineRenderers to create complex, non-continuous shapes.
/// This version correctly generates Sierpinski triangles without visual artifacts.
/// </summary>
public class StrokePainter : MonoBehaviour
{
    public enum DalgonaShapeType
    {
        SierpinskiTriangle,
        Circle,
        Star,
        CustomFromProfile
    }

    [Header("Shape Selection")]
    [Tooltip("The type of shape to generate for the game.")]
    public DalgonaShapeType shapeToGenerate = DalgonaShapeType.SierpinskiTriangle;

    [Header("Setup")]
    [Tooltip("Prefab for the line a player draws. Must have a LineRenderer.")]
    public GameObject strokePrefab;
    [Tooltip("A container object that will hold all the generated dalgona shape segments.")]
    public Transform dalgonaShapeContainer;
    [Tooltip("A prefab for a single segment of the dalgona shape. Must have a LineRenderer.")]
    public GameObject dalgonaSegmentPrefab;

    [Header("General Settings")]
    [Tooltip("The overall size of all generated shapes.")]
    public float shapeSize = 10f;

    [Header("Game Difficulty")]
    [Tooltip("The difficulty level, which affects shape complexity.")]
    [Range(1, 3)]
    public int difficultyLevel = 1;

    [Tooltip("A profile asset containing the points for a custom shape. Required for 'CustomFromProfile' shape type.")]
    public DalgonaShapeProfile customShapeProfile;

    [Header("Game Rules")]
    [Tooltip("Allowed distance from the shape's path.")]
    public float tolerance = 0.1f;
    [Tooltip("The percentage of the shape that must be traced for success.")]
    [Range(0.1f, 1f)]
    public float completionPercentage = 0.9f;

    [Header("Visuals")]
    [Tooltip("Material to apply to the line when the game is over.")]
    public Material failedMaterial;
    [Tooltip("A container for the tolerance visualizers. Will be auto-populated.")]
    public Transform toleranceVisualizerContainer;

    // --- Private State ---
    private List<LineRenderer> dalgonaLines = new List<LineRenderer>();
    private List<LineRenderer> toleranceLines = new List<LineRenderer>();

    private Camera mainCamera;
    private bool isGameOver, isGameWon, isDrawing;
    private LineRenderer currentStrokeRenderer;

    private List<bool[]> completionPointTrackers;
    private int totalPointsInShape = 0;
    private int coveredPointsCount = 0;

    void Start()
    {
        mainCamera = Camera.main;
        if (dalgonaShapeContainer == null || strokePrefab == null || dalgonaSegmentPrefab == null || toleranceVisualizerContainer == null)
        {
            Debug.LogError("One or more required prefabs/containers are not assigned!");
            this.enabled = false;
            return;
        }

        // Always clear any pre-existing shapes and generate the currently selected one at runtime.
        // This ensures the game view is always in sync with the inspector settings.
        ClearShape();
        GenerateShape();
    }

    [ContextMenu("Generate Shape")]
    private void GenerateShape()
    {
        ClearShape(); // Clear existing shapes before generating new ones

        switch (shapeToGenerate)
        {
            case DalgonaShapeType.SierpinskiTriangle:
                GenerateTriangleByLevel(difficultyLevel);
                break;
            case DalgonaShapeType.Circle:
                GenerateCircleByLevel(difficultyLevel);
                break;
            case DalgonaShapeType.Star:
                GenerateStarByLevel(difficultyLevel);
                break;
            case DalgonaShapeType.CustomFromProfile:
                GenerateCustomShape();
                break;
        }

        InitializeAfterGeneration();
        Debug.Log($"Generated {shapeToGenerate} (Level {difficultyLevel}) with {dalgonaLines.Count} segments and {totalPointsInShape} total points.");
    }

    private void GenerateTriangleByLevel(int level)
    {
        float halfSize = shapeSize / 2f;
        float height = halfSize * Mathf.Sqrt(3);

        Vector3 p1 = new Vector3(0, height / 2f, 0);
        Vector3 p2 = new Vector3(-halfSize, -height / 2f, 0);
        Vector3 p3 = new Vector3(halfSize, -height / 2f, 0);

        int depth;
        switch (level)
        {
            case 1:
                depth = 0; // A single triangle
                break;
            case 2:
                depth = 1; // Basic Sierpinski
                break;
            case 3:
                depth = 2; // Less Complex Sierpinski
                break;
            default:
                depth = 0;
                break;
        }
        SierpinskiRecursive(p1, p2, p3, depth);
    }

    private void GenerateCircleByLevel(int level)
    {
        const int segments = 60;
        float radius = shapeSize / 2f;

        switch (level)
        {
            case 1:
                CreateCircleLine(Vector3.zero, radius, segments);
                break;
            case 2:
                // Venn Diagram with 3 circles in a triangular layout
                float vennRadius = shapeSize / 2.9f;
                float offset = vennRadius * 0.8f;
                CreateCircleLine(new Vector3(0, offset, 0), vennRadius, segments);
                CreateCircleLine(new Vector3(-offset, -offset * 0.5f, 0), vennRadius, segments);
                CreateCircleLine(new Vector3(offset, -offset * 0.5f, 0), vennRadius, segments);
                break;
            case 3:
                // Mosquito Coil
                GenerateSpiral();
                break;
        }
    }

    private void GenerateStarByLevel(int level)
    {
        float outerRadius = shapeSize / 2f;
        switch (level)
        {
            case 1:
                // 5 points, pointed up
                CreateStarLine(5, outerRadius, outerRadius * 0.5f, Vector3.zero, 90f);
                break;
            case 2:
                // Shooting Star
                GenerateShootingStar();
                break;
            case 3:
                // Starbucks-inspired logo
                GenerateStarbucksLogo();
                break;
        }
    }

    private void GenerateStarbucksLogo()
    {
        float outerRadius = shapeSize / 2f;

        // 1. Outer Circle
        CreateCircleLine(Vector3.zero, outerRadius, 60);

        // 2. Center Star
        float starRadius = shapeSize * 0.12f;
        Vector3 starCenter = new Vector3(0, shapeSize * 0.25f, 0);
        CreateStarLine(5, starRadius, starRadius * 0.5f, starCenter, 90f);

        // 3. The two iconic tail fins
        // Define the shape for the right fin...
        List<Vector3> rightFinPoints = new List<Vector3>
        {
            new Vector3(shapeSize * 0.15f, -shapeSize * 0.15f, 0),
            new Vector3(shapeSize * 0.4f, -shapeSize * 0.1f, 0),
            new Vector3(shapeSize * 0.35f, -shapeSize * 0.25f, 0),
            new Vector3(shapeSize * 0.45f, -shapeSize * 0.3f, 0),
            new Vector3(shapeSize * 0.2f, -shapeSize * 0.3f, 0)
        };
        CreateLine(rightFinPoints, true);

        // ...and mirror it for the left fin.
        List<Vector3> leftFinPoints = new List<Vector3>();
        foreach (var point in rightFinPoints)
        {
            leftFinPoints.Add(new Vector3(-point.x, point.y, point.z));
        }
        CreateLine(leftFinPoints, true);
    }

    private void CreateCircleLine(Vector3 center, float radius, int segments)
    {
        GameObject segmentGO = Instantiate(dalgonaSegmentPrefab, dalgonaShapeContainer);
        LineRenderer lr = segmentGO.GetComponent<LineRenderer>();
        lr.useWorldSpace = true;

        lr.positionCount = segments + 1;
        lr.loop = true;

        float angleStep = 360f / segments;
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = center.x + Mathf.Cos(angle) * radius;
            float y = center.y + Mathf.Sin(angle) * radius;
            lr.SetPosition(i, new Vector3(x, y, 0));
        }
        dalgonaLines.Add(lr);
    }

    private void CreateBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, int segments)
    {
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i <= segments; i++)
        {
            points.Add(CalculateQuadraticBezierPoint((float)i/segments, p0, p1, p2));
        }
        CreateLine(points, false);
    }

    private void CreateStarLine(int points, float outerRadius, float innerRadius, Vector3 center, float rotationOffsetDegrees = 0f)
    {
        GameObject segmentGO = Instantiate(dalgonaSegmentPrefab, dalgonaShapeContainer);
        LineRenderer lr = segmentGO.GetComponent<LineRenderer>();
        lr.useWorldSpace = true;

        int totalVertices = points * 2;
        lr.positionCount = totalVertices + 1;
        lr.loop = true;

        float angleStep = 360f / totalVertices;
        float rotationOffsetRad = rotationOffsetDegrees * Mathf.Deg2Rad;

        for (int i = 0; i <= totalVertices; i++)
        {
            float radius = (i % 2 == 0) ? outerRadius : innerRadius;
            float currentAngle = rotationOffsetRad + (i * angleStep * Mathf.Deg2Rad);

            float x = center.x + Mathf.Cos(currentAngle) * radius;
            float y = center.y + Mathf.Sin(currentAngle) * radius;
            lr.SetPosition(i, new Vector3(x, y, 0));
        }
        dalgonaLines.Add(lr);
    }

    private void GenerateShootingStar()
    {
        // --- Setup ---
        float starSize = shapeSize / 1.5f;
        float outerRadius = starSize / 2f;
        float innerRadius = outerRadius * 0.5f;
        Vector3 starCenter = new Vector3(-shapeSize / 2.5f, 0, 0);
        float starRotationDegrees = 90f; // Point up

        // --- 1. Draw the Star ---
        CreateStarLine(5, outerRadius, innerRadius, starCenter, starRotationDegrees);

        // --- 2. Calculate Tail Attachment Points ---
        float rotationRad = starRotationDegrees * Mathf.Deg2Rad;
        
        // The tails emerge from the two "notches" on the right side of the point-up star.
        // Top-right notch is at vertex index 9 (angle: 90 - 36 = 54 deg)
        float upperTailStartAngle = rotationRad - (36f * Mathf.Deg2Rad);
        Vector3 upperTailStart = starCenter + new Vector3(Mathf.Cos(upperTailStartAngle) * innerRadius, Mathf.Sin(upperTailStartAngle) * innerRadius, 0);

        // Bottom-right notch is at vertex index 3 (angle: 90 - 108 = -18 deg)
        float lowerTailStartAngle = rotationRad - (108f * Mathf.Deg2Rad);
        Vector3 lowerTailStart = starCenter + new Vector3(Mathf.Cos(lowerTailStartAngle) * innerRadius, Mathf.Sin(lowerTailStartAngle) * innerRadius, 0);

        // --- 3. Define the centerline for the shared edge ---
        Vector3 midTailStart = (upperTailStart + lowerTailStart) / 2f;
        Vector3 tailControlOffset = new Vector3(shapeSize * 0.4f, shapeSize * 0.1f, 0);
        Vector3 tailEndOffset     = new Vector3(shapeSize * 0.8f, -shapeSize * 0.1f, 0); // Shortened the tail length
        Vector3 midTailControl = midTailStart + tailControlOffset;
        Vector3 midTailEnd   = midTailStart + tailEndOffset;

        // --- 4. Generate the three parallel curves (upper, middle, lower) ---
        int tailSegments = 25;
        float ribbonWidth = shapeSize / 7f; // This is the distance from the centerline to an outer edge.

        List<Vector3> upperCurve = new List<Vector3>();
        List<Vector3> middleCurve = new List<Vector3>();
        List<Vector3> lowerCurve = new List<Vector3>();

        for (int i = 0; i <= tailSegments; i++)
        {
            float t = (float)i / tailSegments;
            Vector3 midPoint = CalculateQuadraticBezierPoint(t, midTailStart, midTailControl, midTailEnd);
            middleCurve.Add(midPoint);

            Vector3 derivative = 2 * (1 - t) * (midTailControl - midTailStart) + 2 * t * (midTailEnd - midTailControl);
            Vector3 normal = Vector3.Cross(derivative, Vector3.forward).normalized;
            
            upperCurve.Add(midPoint + normal * ribbonWidth);
            lowerCurve.Add(midPoint - normal * ribbonWidth);
        }

        // --- 5. Create the final shapes: one outline and one centerline to avoid overlap ---
        
        // To prevent the middle line from overlapping the star, we shorten it by removing the first few points.
        int pointsToSkip = 4; // Adjust this to control the size of the gap.
        List<Vector3> shorterMidCurve = (middleCurve.Count > pointsToSkip)
            ? middleCurve.GetRange(pointsToSkip, middleCurve.Count - pointsToSkip)
            : middleCurve;

        // The centerline is the shared middle curve, drawn once as an open line
        CreateLine(shorterMidCurve, false);

        // The outline is a single closed loop made from the outer curves
        List<Vector3> outlinePoints = new List<Vector3>();
        outlinePoints.AddRange(upperCurve);
        
        // Add a spikier, "double V" end cap
        Vector3 upperEnd = upperCurve.Last();
        Vector3 lowerEnd = lowerCurve.Last();
        Vector3 direction = (midTailEnd - midTailControl).normalized;

        float indentDepth = ribbonWidth * 1.2f;
        float spikeLength = ribbonWidth * 0.5f;

        // Define the 5 key points of the double V shape, spreading the valleys further apart.
        Vector3 point_upper_quarter = Vector3.Lerp(upperEnd, lowerEnd, 0.25f);
        Vector3 point_lower_three_quarters = Vector3.Lerp(upperEnd, lowerEnd, 0.75f);
        Vector3 centerPoint = (upperEnd + lowerEnd) / 2f;

        Vector3 valley1 = point_upper_quarter - direction * indentDepth;
        Vector3 midSpike = centerPoint + direction * spikeLength;
        Vector3 valley2 = point_lower_three_quarters - direction * indentDepth;

        const int subdivisions = 1; // Add 1 point on each segment of the cap

        // Segment 1: upperEnd -> valley1
        AddSubdividedSegment(outlinePoints, upperEnd, valley1, subdivisions);
        
        // Segment 2: valley1 -> midSpike
        AddSubdividedSegment(outlinePoints, valley1, midSpike, subdivisions);

        // Segment 3: midSpike -> valley2
        AddSubdividedSegment(outlinePoints, midSpike, valley2, subdivisions);

        // Segment 4: valley2 -> lowerEnd
        AddSubdividedSegment(outlinePoints, valley2, lowerEnd, subdivisions);

        lowerCurve.Reverse();
        outlinePoints.AddRange(lowerCurve);
        
        // Add a V-shaped start cap that connects to the star
        Vector3 startUpperCorner = upperCurve.First();
        Vector3 startLowerCorner = lowerCurve.Last(); // This is the final point in the list now
        Vector3 startCenter = (startUpperCorner + startLowerCorner) / 2f;
        Vector3 directionToStar = (starCenter - startCenter).normalized;
        float startCapDepth = ribbonWidth * 1.5f;
        // Reversed the direction of the V-cap to point outwards
        Vector3 startValley = startCenter - directionToStar * startCapDepth;

        outlinePoints.Add(startValley);

        CreateLine(outlinePoints, true); // Create a closed loop for the outline
    }

    private void AddSubdividedSegment(List<Vector3> points, Vector3 start, Vector3 end, int divisions)
    {
        for (int i = 1; i <= divisions; i++)
        {
            points.Add(Vector3.Lerp(start, end, (float)i / (divisions + 1)));
        }
        points.Add(end);
    }

    private void CreateLine(List<Vector3> points, bool loop)
    {
        GameObject segmentGO = Instantiate(dalgonaSegmentPrefab, dalgonaShapeContainer);
        LineRenderer lr = segmentGO.GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = points.Count;
        lr.SetPositions(points.ToArray());
        lr.loop = loop;
        dalgonaLines.Add(lr);
    }

    private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        // B(t) = (1-t)^2 * P0 + 2(1-t)t * P1 + t^2 * P2
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;
        return p;
    }

    private void GenerateSpiral()
    {
        const float startRadiusFactor = 0.05f;
        const float endRadiusFactor = 0.5f;
        const int segments = 150;
        const int turns = 5;

        GameObject segmentGO = Instantiate(dalgonaSegmentPrefab, dalgonaShapeContainer);
        LineRenderer lr = segmentGO.GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = segments;
        lr.loop = false;

        float totalAngle = turns * 360f;
        float spiralStartRadius = shapeSize * startRadiusFactor;
        float spiralEndRadius = shapeSize * endRadiusFactor;

        for (int i = 0; i < segments; i++)
        {
            // t is the normalized progress of the spiral, from 0 to 1
            float t = (float)i / (segments - 1);
            float currentAngle = t * totalAngle * Mathf.Deg2Rad;
            float currentRadius = Mathf.Lerp(spiralStartRadius, spiralEndRadius, t);

            float x = Mathf.Cos(currentAngle) * currentRadius;
            float y = Mathf.Sin(currentAngle) * currentRadius;
            lr.SetPosition(i, new Vector3(x, y, 0));
        }
        dalgonaLines.Add(lr);
    }

    private void GenerateCustomShape()
    {
        if (customShapeProfile == null || customShapeProfile.paths.Count == 0)
        {
            Debug.LogError("Custom Shape Profile is not assigned or has no paths to draw.");
            return;
        }

        foreach (var path in customShapeProfile.paths)
        {
            if (path.points.Count >= 2)
            {
                CreateLine(path.points, path.closeShape);
            }
        }
    }
    
    private void SierpinskiRecursive(Vector3 p1, Vector3 p2, Vector3 p3, int depth)
    {
        if (depth <= 0)
        {
            AddTriangle(p1, p2, p3);
            return;
        }

        Vector3 m12 = (p1 + p2) / 2;
        Vector3 m23 = (p2 + p3) / 2;
        Vector3 m31 = (p3 + p1) / 2;

        SierpinskiRecursive(p1, m12, m31, depth - 1);
        SierpinskiRecursive(p2, m23, m12, depth - 1);
        SierpinskiRecursive(p3, m31, m23, depth - 1);
    }
    
    private void AddTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        GameObject segmentGO = Instantiate(dalgonaSegmentPrefab, dalgonaShapeContainer);
        LineRenderer lr = segmentGO.GetComponent<LineRenderer>();
        lr.useWorldSpace = true; // Ensure the line renderer uses world space for its points.
        
        List<Vector3> points = new List<Vector3>();
        AddLine(points, p1, p2);
        points.RemoveAt(points.Count - 1); // Remove duplicate point before next segment
        AddLine(points, p2, p3);
        points.RemoveAt(points.Count - 1);
        AddLine(points, p3, p1);

        lr.positionCount = points.Count;
        lr.SetPositions(points.ToArray());
        dalgonaLines.Add(lr);
    }

    private void AddLine(List<Vector3> points, Vector3 start, Vector3 end)
    {
        const int subdivisions = 3;
        points.Add(start);
        for (int i = 1; i <= subdivisions; i++)
        {
            float t = (float)i / (subdivisions + 1);
            points.Add(Vector3.Lerp(start, end, t));
        }
        points.Add(end);
    }
    
    [ContextMenu("Clear Shape")]
    private void ClearShape()
    {
        if (Application.isPlaying)
        {
            foreach (Transform child in dalgonaShapeContainer) Destroy(child.gameObject);
            foreach (Transform child in toleranceVisualizerContainer) Destroy(child.gameObject);
        }
        else
        {
            while (dalgonaShapeContainer.childCount > 0) DestroyImmediate(dalgonaShapeContainer.GetChild(0).gameObject);
            while (toleranceVisualizerContainer.childCount > 0) DestroyImmediate(toleranceVisualizerContainer.GetChild(0).gameObject);
        }
        dalgonaLines.Clear();
        toleranceLines.Clear();
    }
    
    private void InitializeAfterGeneration()
    {
        completionPointTrackers = new List<bool[]>();
        totalPointsInShape = 0;
        coveredPointsCount = 0;
        
        foreach (LineRenderer lr in dalgonaLines)
        {
            completionPointTrackers.Add(new bool[lr.positionCount]);
            totalPointsInShape += lr.positionCount;
        }

        SetupToleranceVisualizers();
    }

    // --- Core Game Logic ---
    void Update() { if (isGameOver || isGameWon) return; if (Input.GetMouseButtonDown(0)) StartDrawing(); if (Input.GetMouseButton(0) && isDrawing) ContinueDrawing(); if (Input.GetMouseButtonUp(0) && isDrawing) StopDrawing(); }
    private void StartDrawing() { Vector3 mousePos = GetMouseWorldPosition(); GameObject strokeGO = Instantiate(strokePrefab, Vector3.zero, Quaternion.identity, this.transform); currentStrokeRenderer = strokeGO.GetComponent<LineRenderer>(); currentStrokeRenderer.positionCount = 0; if (dalgonaLines.Count > 0) currentStrokeRenderer.sortingOrder = dalgonaLines[0].sortingOrder + 1; AddPointToLine(mousePos); if (!IsPointOnPath(mousePos)) { TriggerGameOver(); return; } else { CheckShapeCoverage(mousePos); } isDrawing = true; }
    private void ContinueDrawing()
    {
        Vector3 mousePos = GetMouseWorldPosition();
        AddPointToLine(mousePos);
        if (!IsPointOnPath(mousePos))
        {
            TriggerGameOver();
        }
        else
        {
            CheckShapeCoverage(mousePos);
            // DEBUG: Log current progress
            float currentCoverage = (totalPointsInShape > 0) ? (float)coveredPointsCount / totalPointsInShape : 0f;
            Debug.Log($"진행률 (Progress): {currentCoverage * 100:F1}% ({coveredPointsCount} / {totalPointsInShape} points)");
        }
    }
    private void StopDrawing() { isDrawing = false; currentStrokeRenderer = null; float currentCoverage = 0f; if (totalPointsInShape > 0) currentCoverage = (float)coveredPointsCount / totalPointsInShape; Debug.Log($"현재 총 완성도 (Total Coverage): {currentCoverage * 100:F1}%"); if (currentCoverage >= completionPercentage) TriggerGameWon(); }
    private void TriggerGameOver() { Debug.LogError("실패! 경로를 벗어났습니다. (Game Over! You strayed from the path.)"); isGameOver = true; isDrawing = false; if (currentStrokeRenderer != null && failedMaterial != null) currentStrokeRenderer.material = failedMaterial; }
    private void TriggerGameWon() { Debug.Log("성공! 모양을 완성했습니다! (Success! You completed the shape!)"); isGameWon = true; this.enabled = false; }
    private void AddPointToLine(Vector3 position) { if (currentStrokeRenderer == null) return; if (currentStrokeRenderer.positionCount > 0 && Vector3.Distance(currentStrokeRenderer.GetPosition(currentStrokeRenderer.positionCount - 1), position) < 0.01f) return; currentStrokeRenderer.positionCount++; currentStrokeRenderer.SetPosition(currentStrokeRenderer.positionCount - 1, position); }
    private Vector3 GetMouseWorldPosition() { Vector3 mousePos = Input.mousePosition; mousePos.z = mainCamera.nearClipPlane + 10; return mainCamera.ScreenToWorldPoint(mousePos); }

    private void CheckShapeCoverage(Vector3 drawnPoint)
    {
        Vector2 drawnPoint2D = new Vector2(drawnPoint.x, drawnPoint.y);
        coveredPointsCount = 0; // Recalculate from scratch each time

        for (int i = 0; i < dalgonaLines.Count; i++)
        {
            LineRenderer line = dalgonaLines[i];
            bool[] tracker = completionPointTrackers[i];
            for (int j = 0; j < line.positionCount; j++)
            {
                if (!tracker[j])
                {
                    Vector3 shapePoint3D = line.useWorldSpace ? line.GetPosition(j) : line.transform.TransformPoint(line.GetPosition(j));
                    if (Vector2.Distance(drawnPoint2D, new Vector2(shapePoint3D.x, shapePoint3D.y)) <= tolerance)
                    {
                        tracker[j] = true;
                    }
                }
                if (tracker[j]) coveredPointsCount++;
            }
        }
    }

    private bool IsPointOnPath(Vector3 point)
    {
        if (dalgonaLines.Count == 0)
        {
            // Debug.LogWarning("[DEBUG] dalgonaLines가 비어있습니다. 에디터에서 [Generate Sierpinski Triangle]을 실행했는지 또는 게임 시작 시 모양이 생성되는지 확인해주세요.");
            return false;
        }

        float minDistanceOverall = float.MaxValue;
        foreach (var line in dalgonaLines)
        {
            // We find the minimum distance from the point to any of the line segments.
            for (int i = 0; i < line.positionCount - 1; i++)
            {
                Vector3 p1_3D = line.useWorldSpace ? line.GetPosition(i) : line.transform.TransformPoint(line.GetPosition(i));
                Vector3 p2_3D = line.useWorldSpace ? line.GetPosition(i + 1) : line.transform.TransformPoint(line.GetPosition(i + 1));
                float distance = DistancePointToLineSegment(new Vector2(point.x, point.y), new Vector2(p1_3D.x, p1_3D.y), new Vector2(p2_3D.x, p2_3D.y));
                if (distance < minDistanceOverall)
                {
                    minDistanceOverall = distance;
                }
            }
            // If the line is a loop, we must also check the segment connecting the last and first points.
            if (line.loop && line.positionCount > 1)
            {
                Vector3 p1_3D = line.useWorldSpace ? line.GetPosition(line.positionCount - 1) : line.transform.TransformPoint(line.GetPosition(line.positionCount - 1));
                Vector3 p2_3D = line.useWorldSpace ? line.GetPosition(0) : line.transform.TransformPoint(line.GetPosition(0));
                float distance = DistancePointToLineSegment(new Vector2(point.x, point.y), new Vector2(p1_3D.x, p1_3D.y), new Vector2(p2_3D.x, p2_3D.y));
                if (distance < minDistanceOverall)
                {
                    minDistanceOverall = distance;
                }
            }
        }

        bool isOnPath = minDistanceOverall <= tolerance;

        // Detailed log to show exactly what's happening.
        if (!isOnPath)
        {
            Debug.LogError($"[DEBUG] 실패! 경로를 벗어났습니다. 마우스 위치: {point}, 가장 가까운 선분과의 거리: {minDistanceOverall}, 허용 오차: {tolerance}");
        }
        else
        {
            // Debug.Log($"[DEBUG] 경로 위에 있습니다. 마우스 위치: {point}, 가장 가까운 선분과의 거리: {minDistanceOverall}, 허용 오차: {tolerance}");
        }

        return isOnPath;
    }

    private bool IsPointOnSinglePath(Vector3 point, LineRenderer line)
    {
        float minDistance = float.MaxValue;
        Vector2 point2D = new Vector2(point.x, point.y);
        for (int i = 0; i < line.positionCount - 1; i++)
        {
            Vector3 p1_3D = line.useWorldSpace ? line.GetPosition(i) : line.transform.TransformPoint(line.GetPosition(i));
            Vector3 p2_3D = line.useWorldSpace ? line.GetPosition(i + 1) : line.transform.TransformPoint(line.GetPosition(i + 1));
            float distance = DistancePointToLineSegment(point2D, new Vector2(p1_3D.x, p1_3D.y), new Vector2(p2_3D.x, p2_3D.y));
            if (distance < minDistance) minDistance = distance;
        }
        return minDistance <= tolerance;
    }

    public static float DistancePointToLineSegment(Vector2 point, Vector2 p1, Vector2 p2) { if (p1 == p2) return Vector2.Distance(point, p1); Vector2 lineDirection = p2 - p1; float lineLengthSqr = lineDirection.sqrMagnitude; Vector2 pointVector = point - p1; float t = Mathf.Clamp01(Vector2.Dot(pointVector, lineDirection) / lineLengthSqr); Vector2 projection = p1 + t * lineDirection; return Vector2.Distance(point, projection); }
    
    private void SetupToleranceVisualizers()
    {
        ClearToleranceVisualizers();
        foreach (var dalgonaLine in dalgonaLines)
        {
            GameObject visualizerGO = Instantiate(dalgonaSegmentPrefab, toleranceVisualizerContainer);
            LineRenderer visualizerLR = visualizerGO.GetComponent<LineRenderer>();
            visualizerLR.useWorldSpace = dalgonaLine.useWorldSpace;
            if(!visualizerLR.useWorldSpace)
            {
                 visualizerGO.transform.SetParent(dalgonaLine.transform, false);
            }
            Vector3[] points = new Vector3[dalgonaLine.positionCount];
            dalgonaLine.GetPositions(points);
            visualizerLR.positionCount = dalgonaLine.positionCount;
            visualizerLR.SetPositions(points);
            visualizerLR.startWidth = tolerance * 2f;
            visualizerLR.endWidth = tolerance * 2f;
            visualizerLR.loop = dalgonaLine.loop;
            visualizerLR.sortingOrder = dalgonaLine.sortingOrder - 1;
            toleranceLines.Add(visualizerLR);
        }
    }

    private void ClearToleranceVisualizers()
    {
        if (Application.isPlaying) { foreach (Transform child in toleranceVisualizerContainer) Destroy(child.gameObject); }
        else { while (toleranceVisualizerContainer.childCount > 0) DestroyImmediate(toleranceVisualizerContainer.GetChild(0).gameObject); }
        toleranceLines.Clear();
    }
}
