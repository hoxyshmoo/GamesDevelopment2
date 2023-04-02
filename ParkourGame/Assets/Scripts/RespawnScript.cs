using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnScript : MonoBehaviour
{

    [SerializeField] GameObject player;
    [SerializeField] Transform spawnPoint;
    [SerializeField] float spawnValue;

    // Update is called once per frame
    void Update()
    {
        if(player.transform.position.y < -spawnValue)
        {
            RespawnPoint();
        }
    }

    void RespawnPoint()
    {
        transform.position = spawnPoint.position;
    }
      
}