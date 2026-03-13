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
    [SerializeField] private GameObject relicSlotPrefab;
    public void ShowSummary(int experienceGained, List<WeaponInstance> items)
    {
        ShowSummary(experienceGained, items, null);
    }
    public void ShowSummary(int experienceGained, List<WeaponInstance> items, List<RelicInstance> relics)
    {
        expText.text = "Experience Gained: " + experienceGained;
        // show drop items in the container
        //weapon
        foreach (var item in items)
        {
            GameObject slot = Instantiate(slotPrefab, dropItemContainer.transform);
            slot.TryGetComponent<GridLayoutGroup>(out var gridLayoutGroup);
            gridLayoutGroup.cellSize = cellSize;
            slot.TryGetComponent<WeaponInventorySlot>(out var inventorySlot);
            inventorySlot.SetWeapon(item);
        }
        //relic
        if (relics != null && relicSlotPrefab != null && dropItemContainer != null)
        {
            foreach (var relic in relics)
            {
                GameObject slot = Instantiate(relicSlotPrefab, dropItemContainer.transform);
                slot.TryGetComponent<GridLayoutGroup>(out var gridLayoutGroup);
                gridLayoutGroup.cellSize = cellSize;
                slot.TryGetComponent<RelicInventorySlot>(out var inventorySlot);
                inventorySlot.SetRelic(relic);
            }
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