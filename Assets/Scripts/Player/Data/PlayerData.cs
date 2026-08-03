using UnityEngine;
using System;
using System.Collections.Generic;

namespace Player.PlayerData
{
    [Serializable]
    public class PlayerData
    {
        public string username = $"GuestPenguin";
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
        [Range(0, 100)]
        public int volume = 50;
        public int fov = 90;
        public bool vSync = true;
        public bool fullscreen = true;
        public bool shadowsEnabled = true;
    }

    [Serializable]
    public class WeaponsInventory
    {
        public string weaponName = "";
        public string levels = "";
        public List<string> attachments = new List<string>();
    }
}