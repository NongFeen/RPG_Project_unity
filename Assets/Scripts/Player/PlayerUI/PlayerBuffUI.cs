using System.Collections.Generic;
using UnityEngine;

public class PlayerBuffUI : MonoBehaviour, IPlayerStatUI
{
    [SerializeField]public PlayerStats playerStats;
    [SerializeField]public GameObject buffCardPrefab;
    [SerializeField]public Transform buffContainer;
    [SerializeField]public List<GameObject> activeBuffCards = new List<GameObject>();

    public void SetPlayerData(GameObject player)
    {
        playerStats = player.GetComponent<PlayerStats>();
        
    }
    public void Update()
    {
        if (playerStats == null) return;

        UpdateBuffList();
    }

    private void UpdateBuffList()
    {
        ClearBuffList();

        foreach (var buff in playerStats.GetActiveBuffs().Values)
        {
            BaseBuff buffData = buff;

            GameObject card = Instantiate(buffCardPrefab, buffContainer);

            BuffCardUI cardUI = card.GetComponent<BuffCardUI>();
            cardUI.SetBuff(buffData);
            activeBuffCards.Add(card);
        }
    }
     private void ClearBuffList()
    {
        foreach (GameObject card in activeBuffCards)
        {
            Destroy(card);
        }

        activeBuffCards.Clear();
    }
}