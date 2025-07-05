using UnityEngine;

public static class Initializer
{

    public static void SpawnPersistentObjects()
    {
        Debug.Log("Initializer scripti ile 'PERSISTOBJECTS' tarafından yüklendi!");
        Object.DontDestroyOnLoad(Object.Instantiate(Resources.Load("Player")));

    }

}
