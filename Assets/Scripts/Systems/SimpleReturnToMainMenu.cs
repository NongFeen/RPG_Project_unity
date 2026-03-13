using UnityEngine;

public class SimpleReturnToMainMenu : MonoBehaviour
{
    public void ReturnToMainMenu()
    {
        GameManager.Instance.GoToMainMenu();
    }
}
