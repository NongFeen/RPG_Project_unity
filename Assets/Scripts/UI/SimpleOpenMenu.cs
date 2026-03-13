using System.Collections.Generic;
using UnityEngine;

public class SimpleOpenMenu : MonoBehaviour
{
    [SerializeField] List<GameObject> targetMenu; 

    public void OpenTargetMenu(){
        foreach (GameObject menu in targetMenu)
        {
            UIManager.Instance.OpenMenu(menu);
        }
    }
    public void Close()
    {
        UIManager.Instance.CloseTopMenu();
    }
}
