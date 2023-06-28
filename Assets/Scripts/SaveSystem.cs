using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem 
{
   public static void SaveStats(StatsManager statsManager){
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/saving.wick";
        FileStream stream = new FileStream(path, FileMode.Create);
        binaryFormatter.Serialize(stream, statsManager);

        stream.Close();
   }

   public static StatsManager LoadStats(){
        string path = Application.persistentDataPath + "/saving.wick";
        if (File.Exists(path)){
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            Debug.Log(path);
            StatsManager statsManager = binaryFormatter.Deserialize(stream) as StatsManager;
            stream.Close();
            return statsManager;
            
        } else{
            Debug.Log("Save File not Found in " + path);
            StatsManager stats = new StatsManager();
            return stats;
        }
   }
}
