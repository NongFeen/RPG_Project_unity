using UnityEngine;

public class MapInvoker : MonoBehaviour
{
    [SerializeReference] private MapName mapName;
    public void SelectMap(MapInvoker curMapName)
    {
        GameManager.Instance.SelectMap(curMapName.mapName);
        // print(GameManager.Instance.selectMapName.ToString());
    }
}
