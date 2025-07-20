using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DalgonaPath
{
    public string name = "New Path";
    public bool closeShape = true;
    public List<Vector3> points = new List<Vector3>();
}

[CreateAssetMenu(fileName = "NewDalgonaShape", menuName = "Dalgona/Dalgona Shape Profile", order = 1)]
public class DalgonaShapeProfile : ScriptableObject
{
    public List<DalgonaPath> paths = new List<DalgonaPath>();
}
