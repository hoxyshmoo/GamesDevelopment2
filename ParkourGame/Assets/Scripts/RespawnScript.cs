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
    [SerializeField] float screamValue;
    [SerializeField] AudioClip screamAudio;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            gameObject.AddComponent<AudioSource>();
            audioSource = GetComponent<AudioSource>();
        }
        audioSource.clip = screamAudio;
        audioSource.playOnAwake = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (player.transform.position.y < -screamValue)
        {
            if (screamAudio != null)
            {
                audioSource.PlayOneShot(screamAudio);
                audioSource.pitch = 1.1f;
                // Adjust the volume of the audio clip
                audioSource.volume = 0.05f;

                // Add reverb to the audio clip
                audioSource.spatialBlend = 1.0f;
                audioSource.reverbZoneMix = 0.5f;

                // Add distortion to the audio clip
                audioSource.dopplerLevel = 0.0f;
                audioSource.spread = 360.0f;
                audioSource.minDistance = 1.0f;
                audioSource.maxDistance = 50.0f;

            }
        }
        if (player.transform.position.y < -spawnValue)
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
