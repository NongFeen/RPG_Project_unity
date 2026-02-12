using UnityEngine;

public class SimpleSaveScript : MonoBehaviour
{
    public void SaveScript()
    {
        GameManager.Instance.Save();
    }
}
