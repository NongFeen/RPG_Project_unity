using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using static SaveSystem;

public class GameSummary : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI expText;
    [SerializeField] public GameObject dropItemContainer;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] public Vector2 cellSize;
    public void ShowSummary(int experienceGained, List<WeaponInstance> items)
    {
        expText.text = "Experience Gained: " + experienceGained;
        // show drop items in the container
        foreach (var item in items)
        {
            GameObject slot = Instantiate(slotPrefab, dropItemContainer.transform);
            slot.TryGetComponent<GridLayoutGroup>(out var gridLayoutGroup);
            gridLayoutGroup.cellSize = cellSize;
            slot.TryGetComponent<InventorySlotNew>(out var inventorySlot);
            inventorySlot.SetItem(item);
        }
    }
    public void ReturnToMainMenu()
    {
        EndSession();
    }
    private void EndSession()
    {
        Debug.Log("Ending multiplayer session...");
        NetworkManager.Singleton.Shutdown();
        GoToMainMenu();
    }

    private void GoToMainMenu()
    {
        UIManager.Instance.DeactivePlayerHUD();
        GameManager.Instance.gameState = GameState.Lobby;
        GameManager.Instance.RequestEndSessionClientRpc();
        // UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        LoadingScreenManager.Instance.LoadClientScene("MainMenu");

    }

}
