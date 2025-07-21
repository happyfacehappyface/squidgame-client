using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic; // Added for List

[CustomEditor(typeof(StrokePainter))]
public class StrokePainterEditor : Editor
{
    private StrokePainter painter;
    
    // Match the new structure of StrokePainter.cs
    private SerializedProperty shapeToGenerateProp;
    private SerializedProperty shapeSizeProp;
    private SerializedProperty difficultyLevelProp;
    private SerializedProperty customShapeProfileProp;
    
    private SerializedProperty toleranceProp;
    private SerializedProperty completionPercentageProp;
    
    private SerializedProperty dalgonaControllerProp;
    private SerializedProperty strokePrefabProp;
    private SerializedProperty dalgonaShapeContainerProp;
    private SerializedProperty dalgonaSegmentPrefabProp;
    private SerializedProperty toleranceVisualizerContainerProp;
    private SerializedProperty failedMaterialProp;
    private SerializedProperty toleranceMaterialProp;
    
    private int selectedPathIndex = -1;

    private void OnEnable()
    {
        painter = (StrokePainter)target;
        
        // Find all the new and existing properties
        shapeToGenerateProp = serializedObject.FindProperty("shapeToGenerate");
        shapeSizeProp = serializedObject.FindProperty("shapeSize");
        difficultyLevelProp = serializedObject.FindProperty("difficultyLevel");
        customShapeProfileProp = serializedObject.FindProperty("customShapeProfile");
        
        toleranceProp = serializedObject.FindProperty("tolerance");
        completionPercentageProp = serializedObject.FindProperty("completionPercentage");

        dalgonaControllerProp = serializedObject.FindProperty("dalgonaController");
        strokePrefabProp = serializedObject.FindProperty("strokePrefab");
        dalgonaShapeContainerProp = serializedObject.FindProperty("dalgonaShapeContainer");
        dalgonaSegmentPrefabProp = serializedObject.FindProperty("dalgonaSegmentPrefab");
        toleranceVisualizerContainerProp = serializedObject.FindProperty("toleranceVisualizerContainer");
        failedMaterialProp = serializedObject.FindProperty("failedMaterial");
        toleranceMaterialProp = serializedObject.FindProperty("toleranceMaterial");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Group properties for better organization
        EditorGUILayout.LabelField("Game Control", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(dalgonaControllerProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Shape & Difficulty", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(shapeToGenerateProp);
        StrokePainter.DalgonaShapeType shapeType = (StrokePainter.DalgonaShapeType)shapeToGenerateProp.enumValueIndex;

        if (shapeType == StrokePainter.DalgonaShapeType.CustomFromProfile)
        {
            EditorGUILayout.PropertyField(customShapeProfileProp);
            DrawPathEditor();
        }
        else
        {
            EditorGUILayout.PropertyField(shapeSizeProp);
            EditorGUILayout.PropertyField(difficultyLevelProp);
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Game Rules", EditorStyles.boldLabel);

        bool starLevel2 = (shapeType == StrokePainter.DalgonaShapeType.Star && difficultyLevelProp.intValue == 2);
        bool starLevel3 = (shapeType == StrokePainter.DalgonaShapeType.Star && difficultyLevelProp.intValue == 3);

        // -- Tolerance Field --
        EditorGUI.BeginDisabledGroup(starLevel3);
        EditorGUILayout.PropertyField(toleranceProp, new GUIContent(starLevel3 ? "Tolerance (Fixed)" : "Tolerance"));
        EditorGUI.EndDisabledGroup();
        if (starLevel3)
        {
            toleranceProp.floatValue = 0.17f;
            EditorGUILayout.HelpBox("Tolerance is fixed to 0.17 for this level (Starbucks Logo).", MessageType.Info);
        }

        // -- Completion Percentage Field --
        EditorGUI.BeginDisabledGroup(starLevel2);
        EditorGUILayout.PropertyField(completionPercentageProp, new GUIContent(starLevel2 ? "Completion % (Fixed)" : "Completion %"));
        EditorGUI.EndDisabledGroup();
        if (starLevel2)
        {
            completionPercentageProp.floatValue = 0.91f;
            EditorGUILayout.HelpBox("Completion Percentage is fixed to 0.91 for this level (Shooting Star).", MessageType.Info);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Setup (Prefabs & Containers)", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(strokePrefabProp);
        EditorGUILayout.PropertyField(dalgonaShapeContainerProp);
        EditorGUILayout.PropertyField(dalgonaSegmentPrefabProp);
        EditorGUILayout.PropertyField(toleranceVisualizerContainerProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Visuals", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(failedMaterialProp);
        EditorGUILayout.PropertyField(toleranceMaterialProp);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawPathEditor()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Path Editor", EditorStyles.boldLabel);

        if (painter.customShapeProfile != null)
        {
            DalgonaShapeProfile profile = painter.customShapeProfile;

            for (int i = 0; i < profile.paths.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                // Selection button
                GUI.backgroundColor = (i == selectedPathIndex) ? Color.cyan : Color.white;
                if (GUILayout.Button(">", GUILayout.Width(25)))
                {
                    selectedPathIndex = (selectedPathIndex == i) ? -1 : i;
                    SceneView.RepaintAll();
                }
                GUI.backgroundColor = Color.white;

                // Editable name field
                string newName = EditorGUILayout.TextField(profile.paths[i].name);
                if (newName != profile.paths[i].name)
                {
                    Undo.RecordObject(profile, "Rename Path");
                    profile.paths[i].name = newName;
                    EditorUtility.SetDirty(profile);
                }

                // Delete button
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    Undo.RecordObject(profile, "Remove Path");
                    profile.paths.RemoveAt(i);
                    if (selectedPathIndex == i) selectedPathIndex = -1;
                    EditorUtility.SetDirty(profile);
                    EditorGUILayout.EndHorizontal();
                    break; // Exit loop to avoid issues with list modification
                }
                EditorGUILayout.EndHorizontal();
            }

            // Add symmetry button if a path is selected
            if (selectedPathIndex != -1 && selectedPathIndex < profile.paths.Count)
            {
                if (GUILayout.Button("Symmetrize Right to Left (Y-Axis)"))
                {
                    if (EditorUtility.DisplayDialog("Confirm Symmetry",
                        "This will delete all points left of the Y-axis (X<0) and mirror the right side. This cannot be undone.",
                        "Symmetrize", "Cancel"))
                    {
                        SymmetrizePath(profile.paths[selectedPathIndex]);
                    }
                }

                if (GUILayout.Button("Duplicate & Mirror Path (Y-Axis)"))
                {
                    if (EditorUtility.DisplayDialog("Confirm Mirror Duplicate",
                        "This will create a new, separate path that is a mirrored version of the selected one. Continue?",
                        "Duplicate & Mirror", "Cancel"))
                    {
                        DuplicateAndMirrorPath(profile.paths[selectedPathIndex]);
                    }
                }
            }

            // Button to add a new path
            if (GUILayout.Button("Add New Path"))
            {
                Undo.RecordObject(profile, "Add Path");
                profile.paths.Add(new DalgonaPath());
                selectedPathIndex = profile.paths.Count - 1;
                EditorUtility.SetDirty(profile);
            }
        }
    }

    private void OnSceneGUI()
    {
        painter = (StrokePainter)target;
        if (painter == null || painter.shapeToGenerate != StrokePainter.DalgonaShapeType.CustomFromProfile)
        {
            return;
        }

        DalgonaShapeProfile profile = painter.customShapeProfile;
        if (profile == null) return;

        Transform handleTransform = painter.transform;

        for (int i = 0; i < profile.paths.Count; i++)
        {
            DalgonaPath path = profile.paths[i];
            bool isSelected = (i == selectedPathIndex);

            // Make unselected paths more visible with a solid gray color and increased thickness.
            Handles.color = isSelected ? Color.black : Color.gray;
            float lineWidth = isSelected ? 5.0f : 3.0f;
            
            if (path.points.Count >= 2)
            {
                List<Vector3> worldPoints = path.points.Select(p => handleTransform.TransformPoint(p)).ToList();
                if (path.closeShape)
                {
                    worldPoints.Add(worldPoints[0]);
                }
                Handles.DrawAAPolyLine(lineWidth, worldPoints.ToArray());
            }

            // If this path is selected, draw its handles and handle input
            if (isSelected)
            {
                // Make handles smaller and less obtrusive for precision work
                float handleSize = HandleUtility.GetHandleSize(Vector3.zero) * 0.05f;

                for (int j = 0; j < path.points.Count; j++)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 worldPos = handleTransform.TransformPoint(path.points[j]);
                    
                    // Use a simple, non-intrusive dot handle instead of the large position arrows
                    var fmh_180_76_638886567110674702 = Quaternion.identity; Vector3 newWorldPos = Handles.FreeMoveHandle(worldPos, handleSize, Vector3.zero, Handles.DotHandleCap);
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        // If Control key is held, move the entire path
                        if (Event.current.control)
                        {
                            Undo.RecordObject(profile, "Move Path");
                            Vector3 worldDelta = newWorldPos - worldPos;
                            for (int k = 0; k < path.points.Count; k++)
                            {
                                path.points[k] = handleTransform.InverseTransformPoint(handleTransform.TransformPoint(path.points[k]) + worldDelta);
                            }
                        }
                        else // Otherwise, move just the single point
                        {
                            Undo.RecordObject(profile, "Move Point");
                            path.points[j] = handleTransform.InverseTransformPoint(newWorldPos);
                        }
                        EditorUtility.SetDirty(profile);
                    }
                }
                HandleAddPoint(path, handleTransform);
            }
        }
    }

    private void HandleAddPoint(DalgonaPath path, Transform handleTransform)
    {
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.shift)
        {
            Plane plane = new Plane(handleTransform.forward, handleTransform.position);
            Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            if (plane.Raycast(worldRay, out float distance))
            {
                Vector3 worldHitPoint = worldRay.GetPoint(distance);
                Undo.RecordObject(painter.customShapeProfile, "Add Point");
                int insertIndex = FindBestInsertionIndex(handleTransform.InverseTransformPoint(worldHitPoint), path);
                path.points.Insert(insertIndex, handleTransform.InverseTransformPoint(worldHitPoint));
                EditorUtility.SetDirty(painter.customShapeProfile);
                Event.current.Use();
            }
        }
    }
    
    private int FindBestInsertionIndex(Vector3 newPoint, DalgonaPath path)
    {
        if (path.points.Count < 2) return path.points.Count;

        float minDistanceSqr = float.MaxValue;
        int bestIndex = path.points.Count;

        for (int i = 0; i < path.points.Count; i++)
        {
            if (i == path.points.Count - 1 && !path.closeShape) continue;

            Vector3 p1 = path.points[i];
            Vector3 p2 = path.points[(i + 1) % path.points.Count];
            
            // Using a simplified projection check
            Vector3 projection = p1 + Vector3.Project(newPoint - p1, p2 - p1);
            float distSqr = Vector3.SqrMagnitude(newPoint - projection);

            if (distSqr < minDistanceSqr)
            {
                minDistanceSqr = distSqr;
                bestIndex = i + 1;
            }
        }
        return bestIndex;
    }

    private void SymmetrizePath(DalgonaPath path)
    {
        Undo.RecordObject(painter.customShapeProfile, "Symmetrize Path");

        // Keep points on the right side and on the axis itself
        List<Vector3> rightSidePoints = path.points.Where(p => p.x >= 0).ToList();
        
        // Get the points that are strictly on the right side to be mirrored
        List<Vector3> pointsToMirror = rightSidePoints.Where(p => p.x > 0.001f).ToList();

        // Create the new mirrored points
        List<Vector3> mirroredPoints = pointsToMirror.Select(p => new Vector3(-p.x, p.y, p.z)).ToList();

        // The final list is the original right side plus the new mirrored points
        path.points = rightSidePoints.Concat(mirroredPoints).ToList();
        
        EditorUtility.SetDirty(painter.customShapeProfile);
        SceneView.RepaintAll();
    }

    private void DuplicateAndMirrorPath(DalgonaPath originalPath)
    {
        DalgonaShapeProfile profile = painter.customShapeProfile;
        Undo.RecordObject(profile, "Duplicate and Mirror Path");

        // Create a new path to hold the mirrored data
        DalgonaPath newPath = new DalgonaPath
        {
            name = $"{originalPath.name} (Mirrored)",
            closeShape = originalPath.closeShape,
            // Mirror each point across the Y-axis
            points = originalPath.points.Select(p => new Vector3(-p.x, p.y, p.z)).ToList()
        };

        // Add the new path to the list
        profile.paths.Add(newPath);

        EditorUtility.SetDirty(profile);
        SceneView.RepaintAll();
    }
} 