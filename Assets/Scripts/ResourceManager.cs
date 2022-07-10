using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
public static class ResourceManager
{
    /// <summary>
    /// Get object inside Resources/Throwables/
    /// </summary>
    public static GameObject GetThrowable(string name)
    {
        return Object.Instantiate(Resources.Load<GameObject>("Throwables/" + name));
    }

    /// <summary>
    /// Get object inside Resources/GameObjects/
    /// </summary>
    public static GameObject GetGameObject(string name)
    {
        return Object.Instantiate(Resources.Load<GameObject>("GameObjects/" + name));
    }
}
