using UnityEngine;

public class SimpleSelectMap : MonoBehaviour
{
    public void SelectMap(MapInvoker map)
    {
        map.SelectMap(map);
    }
}
