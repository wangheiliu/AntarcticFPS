using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Player.PlayerData;
using UnityEngine;
using UnityEngine.InputSystem;

public static class SaveData
{
    public static string SavePath => Path.Combine(Application.persistentDataPath, "saveData.dat");
    public static string TempSavePath => Path.Combine(Application.persistentDataPath, "saveData.tmp");
    public static string BackupSavePath => Path.Combine(Application.persistentDataPath, "saveData.bak");
    public static void Save(PlayerData data) // i can push default data using this method
    {
        if (data == null)
        {
            Debug.LogError("Cannot save null data.");
            return;
        }
        string json = JsonUtility.ToJson(data);

        try
        {
            File.WriteAllText(TempSavePath, json);

            if (File.Exists(SavePath))
            {
                if (File.Exists(BackupSavePath))
                {
                    File.Delete(BackupSavePath);
                }
                File.Move(SavePath, BackupSavePath);

                File.Delete(SavePath);

            }

            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
            }

            File.Move(TempSavePath, SavePath);

        }
        catch (IOException ex)
        {
            Debug.LogError($"Failed to save data: {ex.Message}");
        }
        finally
        {
            if (File.Exists(TempSavePath))
            {
                File.Delete(TempSavePath);
            }
        }
    }

    public static PlayerData TryLoad(string path)
    {
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json))
                {
                    Debug.LogWarning($"Save file at {path} is empty.");
                    return null;
                }
                return JsonUtility.FromJson<PlayerData>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load data from {path}: {ex.Message}");
            }
        }
        return null;
    }

    public static PlayerData Load()
    {
        PlayerData data;
        data = TryLoad(SavePath);
        if (data != null)
        {
            Debug.Log("Loaded normal data");
            return data;
        }

        data = TryLoad(BackupSavePath);
        if (data != null)
        {
            Debug.Log("Loaded backup");
            return data;
        }

        data = TryLoad(TempSavePath);
        if (data != null)
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    File.Delete(SavePath);
                }
                File.Move(TempSavePath, SavePath);
            }
            catch (IOException ex)
            {
                Debug.LogError($"Failed to restore temp save: {ex.Message}");
            }
            return data;
        }

        return null;
    }

    public static PlayerData UpdateData(PlayerData data)
    {
        var defaultData = new PlayerData();
        FieldInfo[] fields = typeof(PlayerData).GetFields(BindingFlags.Public | BindingFlags.Instance); // this should get the current data model's fields

        foreach (var field in fields)
        {
            object name = field.Name;
            object saveValue = field.GetValue(data);
            object defaultValue = field.GetValue(defaultData);
            RangeAttribute rangeAttribute = field.GetCustomAttribute<RangeAttribute>();

            if (saveValue == null)
            {
                try
                {
                    object converted = Convert.ChangeType(saveValue, defaultValue.GetType());
                    field.SetValue(data, defaultData); // the object PlayerData, NOT saveValue
                    if (rangeAttribute != null && (saveValue is float || saveValue is int))
                    {
                        field.SetValue(data, Mathf.Clamp(Convert.ToSingle(saveValue), rangeAttribute.min, rangeAttribute.max));
                    }
                } catch (InvalidCastException) {
                    continue;
                }
            }

            if (defaultValue == null)
            {
                continue;
            }

            if (saveValue.GetType() != defaultValue.GetType()) {
                try {
                    object converted = Convert.ChangeType(saveValue, defaultValue.GetType());
                    field.SetValue(data, converted);
                } catch (InvalidCastException) {
                    continue;
                } 
            }

            if ((string)name == "saveVersion") {
                field.SetValue(data, defaultValue);
            }
        }

        Save(data);
        return data;
    }
}

public static class CurrentPlayerData
{
    public static PlayerData Data {get; set;}
    public static void Initialize()
    {
        Data = SaveData.Load() ?? new PlayerData();
    }

    public static void Save()
    {
        Debug.Log("Saving data...");
        SaveData.Save(Data);
        Data = SaveData.Load();
    }
}

public static class SavePlayerKey
{
    private static string KeyPath => Path.Combine(Application.persistentDataPath, "save.key");

    public static byte[] GetKey()
    {
        if (File.Exists(KeyPath))
        {
            return File.ReadAllBytes(KeyPath);
        }

        byte[] key = new byte[32];
        RandomNumberGenerator.Fill(key);
        File.WriteAllBytes(KeyPath, key);
        return key;
    }
}