using System.Collections.Generic;
using UnityEngine;

public class SimpleOpenMenu : MonoBehaviour
{
    [SerializeField] public List<GameObject> targetMenu; 

    public void OpenTargetMenu(){
        UIManager.Instance.OpenMenuGroup(targetMenu);
    }
    public void Close()
    {
        UIManager.Instance.CloseTopMenu();
    }
    public void SetTargetMenu(List<GameObject> newTargetmenu)
    {
        targetMenu = newTargetmenu;
    }
}
