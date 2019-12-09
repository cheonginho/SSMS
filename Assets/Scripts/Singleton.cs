using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Singleton<T>  : MonoBehaviour where T : class 
{
   
    protected static T instance = null;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = System.Activator.CreateInstance(typeof(T)) as T;
            }
            return instance;
        }
    }
}
