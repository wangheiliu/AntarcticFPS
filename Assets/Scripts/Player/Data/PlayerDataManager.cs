using UnityEngine;
using Player.PlayerData;
using System.Data.Common;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Threading.Tasks;
public class PlayerDataManager : MonoBehaviour
{
    void Awake()
    {
        CurrentPlayerData.Initialize();
        CurrentPlayerData.Data = SaveData.UpdateData(CurrentPlayerData.Data);
    }
}
public static class GeneralPlayerDataManager
{
    public static event Action OnUsernameChanged;
    public static event Action<WeaponsInventory, WeaponType> OnWeaponEquipChanged;
    public static event Action OnWeaponInventoryChanged;
    public static string Username
    {
        get => CurrentPlayerData.Data.username;
        set
        {
            if (CurrentPlayerData.Data.username == value)
            {
                return;
            }

            CurrentPlayerData.Data.username = value;
            OnUsernameChanged?.Invoke();
        }
    }

    public static WeaponsInventory PrimaryWeapon
    {
        get => CurrentPlayerData.Data.primaryWeaponEquipped;
        set
        {
            if (CurrentPlayerData.Data.primaryWeaponEquipped == value)
            {
                return;
            }

            CurrentPlayerData.Data.primaryWeaponEquipped = value;
            OnWeaponEquipChanged?.Invoke(CurrentPlayerData.Data.primaryWeaponEquipped, WeaponType.Primary);
        }
    }

    public static WeaponsInventory SecondaryWeapon
    {
        get => CurrentPlayerData.Data.secondaryWeaponEquipped;
        set
        {
            if (CurrentPlayerData.Data.secondaryWeaponEquipped == value)
            {
                return;
            }

            CurrentPlayerData.Data.secondaryWeaponEquipped = value;
            OnWeaponEquipChanged?.Invoke(CurrentPlayerData.Data.secondaryWeaponEquipped, WeaponType.Secondary);
        }
    }

    public static WeaponsInventory ToolWeapon
    {
        get => CurrentPlayerData.Data.toolsEquipped;
        set
        {
            if (CurrentPlayerData.Data.toolsEquipped == value)
            {
                return;
            }

            CurrentPlayerData.Data.toolsEquipped = value;
            OnWeaponEquipChanged?.Invoke(CurrentPlayerData.Data.toolsEquipped, WeaponType.Tools);
        }
    }

    public static List<WeaponsInventory> WeaponsOwned
    {
        get => CurrentPlayerData.Data.weaponsOwned;
        set
        {
            if (value == null)
            {
                return;
            }
            if (CurrentPlayerData.Data.weaponsOwned == value)
            {
                return;
            }

            CurrentPlayerData.Data.weaponsOwned = value ?? new List<WeaponsInventory>();
            OnWeaponInventoryChanged?.Invoke();
        }
    }
}
public static class ProgressionDataManager
{
    public static event Action OnProgressionDataChanged;
    [DataStatDisplay("Money", "ProfileDisplay")] public static int Money
    {
        get => CurrentPlayerData.Data.progressionData.money;
        set
        {
            if (CurrentPlayerData.Data.progressionData.money == value)
            {
                return;
            }

            CurrentPlayerData.Data.progressionData.money = value;
            OnProgressionDataChanged?.Invoke();
        }
    }

    [DataStatDisplay("Level", "ProfileDisplay")] public static int Level
    {
        get => CurrentPlayerData.Data.progressionData.level;
        set
        {
            if (CurrentPlayerData.Data.progressionData.level == value)
            {
                return;
            }

            CurrentPlayerData.Data.progressionData.level = value;
            OnProgressionDataChanged?.Invoke();
        }
    }

    [DataStatDisplay("XP", "ProfileDisplay")] public static int XP
    {
        get => CurrentPlayerData.Data.progressionData.xp;
        set => CurrentPlayerData.Data.progressionData.xp = value;
    }

    public static List<string> LevelsCompleted
    {
        get => CurrentPlayerData.Data.progressionData.levelsCompleted;
        set
        {
            if (CurrentPlayerData.Data.progressionData.levelsCompleted == value)
            {
                return;
            }
            CurrentPlayerData.Data.progressionData.levelsCompleted = value;
            OnProgressionDataChanged?.Invoke();
        }
    }
}

public class DataStatDisplay : Attribute
{
    public string DisplayName { get; set; }
    public string Location {get;set;}

    public DataStatDisplay(string displayName = "", string location = "")
    {
        DisplayName = displayName;
        Location = location;
    }
}
