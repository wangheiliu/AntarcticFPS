using UnityEngine;
using Player.PlayerData;
using System.Data.Common;
public class SavePlayerData : MonoBehaviour
{
    void Awake()
    {
        CurrentPlayerData.Initialize();
        CurrentPlayerData.Data = SaveData.UpdateData(CurrentPlayerData.Data);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
