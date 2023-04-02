using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartsBar : MonoBehaviour
{
    public int maxHealth = 3;
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    private int _health;

    public int Health
    {
        get => _health;
        set
        {
            _health = Mathf.Clamp(value, 0, maxHealth);
            UpdateHeartsDisplay();
        }
    }

    private void Awake()
    {
        _health = maxHealth;
        UpdateHeartsDisplay();
    }

    private void UpdateHeartsDisplay()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].sprite = i < _health ? fullHeart : emptyHeart;
        }
    }
}
