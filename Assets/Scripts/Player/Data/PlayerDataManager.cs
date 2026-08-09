using UnityEngine;
using Player.PlayerData;
using System.Data.Common;
using System;
using System.Collections.Generic;
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
}
public static class ProgressionDataManager
{
    public static event Action OnProgressionDataChanged;
    [DataStatDisplay("Money")] public static int Money
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

    [DataStatDisplay("Level")] public static int Level
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

    [DataStatDisplay("XP")] public static int XP
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

    public DataStatDisplay(string displayName)
    {
        DisplayName = displayName;
    }
}
