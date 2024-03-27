using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Spawnpoint : MonoBehaviour
{
    [SerializeField] GameObject visual;

    private void Awake()
    {
        visual.SetActive(false);
    }
}
