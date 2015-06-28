using System;
using UnityEngine;

public static class Util
{
    static public T InstantiatePrefab<T>(T prefab) where T : Component
    {
        return InstantiatePrefab<T>(prefab, prefab.transform.position, prefab.transform.rotation);
    }
    
    static public GameObject InstantiatePrefab(GameObject prefab)
    {
        return InstantiatePrefab(prefab, prefab.transform.position, prefab.transform.rotation);
    }
    
    static public T InstantiatePrefab<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
    {
        GameObject gob = InstantiatePrefab(prefab.gameObject, position, rotation);
        return gob.GetComponent<T>();
    }
    
    static public GameObject InstantiatePrefab(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return (GameObject)UnityEngine.Object.Instantiate(prefab, position, rotation);
    }
}

