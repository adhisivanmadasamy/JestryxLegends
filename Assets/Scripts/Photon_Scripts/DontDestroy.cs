using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroy : MonoBehaviour
{
   public static DontDestroy Instance;

    public bool ComingBack;
    private void Awake()
    {
        if (Instance)
        {
           Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;
    }
}
