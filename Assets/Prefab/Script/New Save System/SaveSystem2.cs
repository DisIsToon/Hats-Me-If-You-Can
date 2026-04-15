using System.IO;
using UnityEngine;

public static class SaveSystem2
{
    private static string GetPath(int slot)
    {
        return Application.persistentDataPath + "/save_slot_" + slot + ".json";
    }

    public static void SaveGame(GameData2 data, int slot)
    {
        Debug.LogError("Saving data");
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPath(slot), json);
    }

    public static GameData2 LoadGame(int slot)
    {
        Debug.LogError("Loading data");
        string path = GetPath(slot);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<GameData2>(json);
        }

        return null;
    }

    public static bool SlotHasSave(int slot)
    {
        return File.Exists(GetPath(slot));
    }

    public static void DeleteSave(int slot)
    {
        string path = GetPath(slot);
        if (File.Exists(path))
            File.Delete(path);
    }
}