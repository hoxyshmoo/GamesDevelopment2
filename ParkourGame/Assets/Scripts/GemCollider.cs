using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemCollider : MonoBehaviour
{
    public AudioClip collisionSound;
    private AudioSource audioSource;
    private GameManager gameManager; // Add this line to reference the GameManager

    private void Start()
    {
        // Find the GameManager instance in the scene
        gameManager = FindObjectOfType<GameManager>(); // Add this line

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            gameObject.AddComponent<AudioSource>();
            audioSource = GetComponent<AudioSource>();
        }
        audioSource.clip = collisionSound;
        audioSource.playOnAwake = false;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (gameManager != null)
        {
            gameManager.UpdateScore(100);
        }

        if (collisionSound != null)
        {
            audioSource.PlayOneShot(collisionSound);
        }

        StartCoroutine(DeactivateAfterDelay(0.5f));
    }

    private IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}
