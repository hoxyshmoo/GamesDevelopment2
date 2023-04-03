using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinCollider : MonoBehaviour
{
    public AudioClip winSound;
    private AudioSource audioSource;
    private GameManager gameManager; // Reference to the GameManager

    private void Start()
    {
        // Find the GameManager instance in the scene
        gameManager = FindObjectOfType<GameManager>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            gameObject.AddComponent<AudioSource>();
            audioSource = GetComponent<AudioSource>();
        }
        audioSource.clip = winSound;
        audioSource.playOnAwake = false;
    }

    public void OnTriggerEnter(Collider other)
    {
       
            // Call the GameManager's Win() method
            if (gameManager != null)
            {
                gameManager.EndGame(true);
            }

            // Play the win sound
            if (winSound != null)
            {
                audioSource.PlayOneShot(winSound);
            }

            // Deactivate this collider after a short delay
            StartCoroutine(DeactivateAfterDelay(1.5f));
       
    }

    private IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}
