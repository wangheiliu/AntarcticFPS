using UnityEngine;
using System;
using System.Collections.Generic;

namespace Player.PlayerData
{
    [Serializable]
    public class PlayerData
    {
        public string username;
        public ProgressionData progressionData;
        public PlayerStats playerStats;

        public List<WeaponsInventory> weaponsOwned;
        public PlayerSettings settings;
    }

    [Serializable]
    public class ProgressionData
    {
        public int money;
        public int xp;
        public int level;
        public List<string> levelsCompleted;
    }

    [Serializable]
    public class PlayerStats
    {
        public int killCounts;
        public int deathCount;

        // add more stats when the game expands
    }

    [Serializable]
    public class PlayerSettings
    {
        public int volume;
        public int fov;
        public bool vSync;
        public bool fullscreen;
        public bool shadowsEnabled;
    }

    [Serializable]
    public class WeaponsInventory
    {
        public string weaponName;
        public string levels;
        public List<string> attachments;
    }
}