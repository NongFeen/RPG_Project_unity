using UnityEngine;

public class SimpleReturnToMainMenu : MonoBehaviour
{
    public void ReturnToMainMenu()
    {
        SaveSystem.Save();
        GameManager.Instance.GoToMainMenu();
    }
}
