using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamBase : MonoBehaviour
{
    public int teamNumber;
    public Color teamColor;
    public GameObject spawnPoint;
    private void Start()
    {
        GameManager.AddTeam(this);
    }
}
