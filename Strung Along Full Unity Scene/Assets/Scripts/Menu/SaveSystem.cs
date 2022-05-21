using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static void SaveData(GameData gameData)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/gameData.bin";
        FileStream stream = new FileStream(path, FileMode.Create);
        formatter.Serialize(stream, gameData);
        stream.Close();
    }

    public static GameData LoadData()
    {
        string path = Application.persistentDataPath + "/gameData.bin";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            GameData data = formatter.Deserialize(stream) as GameData;
            //Debug.Log("Saved file  in" + path);
            stream.Close();

            return data;
        }
        else
        {
            //Debug.LogError("Saved file not found in" + path);
            Debug.Log("No loaded data found, a new game started!!!");
            return null;
        }
      
    }
}

