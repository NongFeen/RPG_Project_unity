using System;
using System.Runtime.Serialization;
using Unity.VisualScripting;

public interface IPlayerStatUI
{
    void SetPlayerData(PlayerStats stats, PlayerEquipedItem equip);
}
