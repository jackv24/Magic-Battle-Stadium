/*
**  CharacterStats.cs: Hold stats for the character it is attached to
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CharacterStats : MonoBehaviour
{
    public int currentHealth = 100;
    public int maxHealth = 100;

    public Slider healthSlider;

    void Start()
    {
        UpdateSlider();
    }

    public void ApplyDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
            Die();

        UpdateSlider();
    }

    void UpdateSlider()
    {
        if (healthSlider)
            healthSlider.value = (float)currentHealth / maxHealth;
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
