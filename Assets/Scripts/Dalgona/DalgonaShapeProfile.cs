using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewDalgonaShape", menuName = "Dalgona/Dalgona Shape Profile", order = 1)]
public class DalgonaShapeProfile : ScriptableObject
{
    [Tooltip("A list of points that define the custom shape. The points will be connected in order.")]
    public List<Vector3> points = new List<Vector3>();

    [Tooltip("Whether the shape should be closed (i.e., connect the last point to the first).")]
    public bool closeShape = true;
}
