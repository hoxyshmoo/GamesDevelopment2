using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnScript : MonoBehaviour
{
    public GameManager gameManager;
    public HeartsBar heartsBar;

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
        gameManager.LifeLoss(1);
        heartsBar.LoseHeart();
    }
      
}