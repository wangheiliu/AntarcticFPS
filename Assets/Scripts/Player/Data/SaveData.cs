using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
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
        string json = JsonUtility.ToJson(data, true);

        byte[] plainText = Encoding.UTF8.GetBytes(json);

        byte[] key = SavePlayerKey.GetKey();

        byte[] nonce = new byte[12];
        RandomNumberGenerator.Fill(nonce);

        byte[] cipherText = new byte[plainText.Length];
        byte[] tag = new byte[16];

        using (AesGcm aes = new AesGcm(key))
        {
            aes.Encrypt(nonce, plainText, cipherText, tag); //fills the tag array
        }

        using (FileStream file = File.Create(TempSavePath))
        {
            file.Write(nonce);
            file.Write(tag);
            file.Write(cipherText);
            file.Flush(true);
        }

        if (File.Exists(SavePath))
        {
            if (File.Exists(BackupSavePath))
            {
                File.Delete(BackupSavePath);
            }
            File.Move(SavePath, BackupSavePath);
        }

        File.Move(TempSavePath, SavePath);
    }

    public static PlayerData TryLoad(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return null;
            }
            byte[] fileData = File.ReadAllBytes(path);

            if (fileData.Length <= 28)
            {
                return null;
            }

            byte[] nonce = new byte[12];
            byte[] tag = new byte[16];
            byte[] cipherText = new byte[fileData.Length - 28]; // the 28 came from the nonce and tag added together
            byte[] plainText = new byte[cipherText.Length];
            Buffer.BlockCopy(fileData, 0, nonce, 0, 12);
            Buffer.BlockCopy(fileData, 12, tag, 0, 16); // since nonce is first, we have to offset it
            Buffer.BlockCopy(fileData, 28, cipherText, 0, cipherText.Length);

            using (AesGcm aes = new AesGcm(SavePlayerKey.GetKey()))
            {
                aes.Decrypt(nonce, cipherText, tag, plainText);
            }

            string json = Encoding.UTF8.GetString(plainText);
            return JsonUtility.FromJson<PlayerData>(json);
        }
        catch (CryptographicException ex)
        {
            Debug.LogError($"Save authentication failed: {ex.Message}");
            return null;
        }
        catch (IOException ex)
        {
            Debug.LogError($"Save IO failed: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error: {ex.Message}");
            return null;
        }
    }

    public static PlayerData Load()
    {
        PlayerData data;
        data = TryLoad(SavePath);
        if (data != null)
        {
            return data;
        }

        data = TryLoad(BackupSavePath);
        if (data != null)
        {
            return data;
        }

        data = TryLoad(TempSavePath);
        if (data != null)
        {
            try
            {
                File.Move(TempSavePath, SavePath);
                File.Delete(BackupSavePath);
            }
            catch (IOException ex)
            {
                Debug.LogError($"Failed to restore temp save: {ex.Message}");
            }
            return data;
        }

        return null;
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