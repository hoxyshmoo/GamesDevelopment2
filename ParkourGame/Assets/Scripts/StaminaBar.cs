using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    public Slider slider;

    // Set this to the player's maximum stamina
    public float maxStamina = 100f;

    // Set this to the player's current stamina
    public float currentStamina = 100f;

    public Gradient gradient; // Create a public Gradient field to store your gradient

    private void Start()
    {
        // Set the slider's min and max values based on the player's maximum stamina
        slider.minValue = 0f;
        slider.maxValue = maxStamina;

        // Set the slider's value based on the player's current stamina
        slider.value = currentStamina;

        // Set the gradient color for the slider's fill area
        var fill = slider.fillRect.GetComponent<Image>();
        fill.color = gradient.Evaluate(1f);
    }

    private void Update()
    {
        // Increase the player's current stamina over time 
        currentStamina += Time.deltaTime * 10f;

        // Make sure the current stamina doesn't exceed the maximum stamina
        if (currentStamina > maxStamina)
        {
            currentStamina = maxStamina;
        }

        // Update the slider's value based on the player's current stamina
        slider.value = currentStamina;

        // Set the gradient color for the slider's fill area based on the current stamina level
        var fill = slider.fillRect.GetComponent<Image>();
        fill.color = gradient.Evaluate(currentStamina / maxStamina);

        // Prevent the player's current stamina from going below 0
        if (currentStamina < 0f)
        {
            currentStamina = 0f;
        }
    }
}
