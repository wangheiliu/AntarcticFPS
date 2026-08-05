using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Player.PlayerData
{
    [Serializable]
    public class PlayerData
    {
        public int saveVersion = 1;
        public string username = $"GuestPenguin";
        public bool hasSetUserName = false;
        public ProgressionData progressionData = new ProgressionData();
        public PlayerStats playerStats = new PlayerStats();

        public List<WeaponsInventory> weaponsOwned = new List<WeaponsInventory>();
        public PlayerSettings settings = new PlayerSettings();
    }

    [Serializable]
    public class ProgressionData
    {
        public int money = 0;
        public int xp = 0;
        public int level = 1;
        public List<string> levelsCompleted = new List<string>();
    }

    [Serializable]
    public class PlayerStats
    {
        public int killCounts = 0;
        public int deathCount = 0;

        // add more stats when the game expands
    }

    [Serializable]
    public class PlayerSettings
    {
        [Range(0, 100)] [SettingsAttribute("Volume")]
        public int volume = 50;
        [Range(0, 100)][SettingsAttribute("Field of View")]
        public int fov = 90;
        [SettingsAttribute("V-Sync")]
        public bool vSync = true;
        [SettingsAttribute("Fullscreen")]
        public bool fullscreen = true;
        [SettingsAttribute("Shadows")]
        public bool shadowsEnabled = true;
    }

    [Serializable]
    public class WeaponsInventory
    {
        public string weaponName = "";
        [FormerlySerializedAs("level")] public int weaponLevel = 1;
        public List<string> attachments = new List<string>();
    }
}