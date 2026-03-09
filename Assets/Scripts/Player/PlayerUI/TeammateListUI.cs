using System.Collections.Generic;
using UnityEngine;

public class TeammateListUI : MonoBehaviour
{
    [SerializeField] private GameObject teammateCardUIPrefab;

    private List<PlayerHealthUI> teammateCards = new();

    private void Start()
    {
        CreateCards();
        PlayerManager.Instance.OnPlayerRegistered += OnPlayerJoined;
        PlayerManager.Instance.OnPlayerUnregistered += OnPlayerLeft;
    }

    private void OnDestroy()
    {
        if (PlayerManager.Instance == null) return;
        PlayerManager.Instance.OnPlayerRegistered -= OnPlayerJoined;
        PlayerManager.Instance.OnPlayerUnregistered -= OnPlayerLeft;
    }

    void OnPlayerJoined(PlayerStats player)
    {
        if (player.IsOwner)
            return;
        CreateCard(player);
    }

    void OnPlayerLeft(PlayerStats player)
    {
        RefreshCard();
    }

    void CreateCards()
    {
        foreach (var player in PlayerManager.Instance.Players)
        {
            if (player.IsOwner)
                continue;
            CreateCard(player);
        }
    }

    void CreateCard(PlayerStats player)
    {
        GameObject obj = Instantiate(teammateCardUIPrefab, transform);
        PlayerHealthUI card = obj.GetComponent<PlayerHealthUI>();
        card.SetPlayerData(player.gameObject);
        teammateCards.Add(card);
    }

    public void RefreshCard()
    {
        foreach (var card in teammateCards)
        {
            Destroy(card.gameObject);
        }
        teammateCards.Clear();
        CreateCards();
    }
}