using UnityEngine;
using UnityEngine.UI;

public class HeartsBar : MonoBehaviour
{
    public Image[] hearts;
    public Sprite fullHeart;
    public int maxHearts = 3;
    private int currentHearts;

    void Start()
    {
        currentHearts = maxHearts;
        UpdateHeartsUI();
    }

    public void LoseHeart()
    {
        currentHearts--;
        UpdateHeartsUI();
        if (currentHearts <= 0)
        {
            // Handle game over condition
        }
    }

    public void AddHeart()
    {
        if (currentHearts < maxHearts)
        {
            currentHearts++;
            UpdateHeartsUI();
        }
    }

    private void UpdateHeartsUI()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHearts)
            {
                hearts[i].sprite = fullHeart;
                hearts[i].enabled = true;
            }
            else
            {
                hearts[i].enabled = false;
            }
        }
    }
}
