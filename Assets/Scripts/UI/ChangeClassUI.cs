using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeClassUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject panelRoot;


    private void Awake()
    {
        if (panelRoot == null)
            panelRoot = gameObject;

        Hide();
    }

    public void Show(PlayerStats stats, Player player)
    {

        // if (panelRoot != null)
        //     panelRoot.SetActive(true);
    }

    public void Hide()
    {
        // if (panelRoot != null)
        //     panelRoot.SetActive(false);

    }

    public void TryChangeClass(ClassType newClass)
    {
        print("Try change class");
        if(GameManager.Instance.localPlayer == null)
            return;
        GameManager.Instance.localPlayer.TryGetComponent<Player>(out Player player);

        // Must be level 15 to perform class change
        if (player.playerExperience == null || !player.playerExperience.CanChangeClass())
            return;
        player.ChangeClass(newClass);
        Hide();
    }
    public void ChangeClassRanger() => TryChangeClass(ClassType.Ranger);
    public void ChangeClassPriest() => TryChangeClass(ClassType.Priest);
    public void ChangeClassTank() => TryChangeClass(ClassType.Tank);

}

