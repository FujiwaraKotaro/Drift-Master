using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallSet : MonoBehaviour
{
    [SerializeField] GameObject Wall;

    public static bool easyMode = false;
    public void Set()
    {
        Wall.SetActive(true);
        easyMode = true;
    }
}
