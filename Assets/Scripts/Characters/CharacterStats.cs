/*
**  CharacterStats.cs: Hold stats for the character it is attached to
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class CharacterStats : NetworkBehaviour
{
    [SyncVar]
    public int currentHealth = 100;
    public int maxHealth = 100;

    public Slider healthSlider;
    public float sliderUpdateInterval = 0.1f;

    void Start()
    {
        StartCoroutine("UpdateSlider");
    }

    public void ApplyDamage(int amount)
    {
        //Damage can only be applied on the server
        if (!isServer)
            return;

        currentHealth -= amount;

        if (currentHealth <= 0)
            Die();

        UpdateSlider();
    }

    //Slider doesn't need to be updated every frame. Coruotine should save performance.
    IEnumerator UpdateSlider()
    {
        if (healthSlider)
        {
            while (true)
            {
                healthSlider.value = (float)currentHealth / maxHealth;

                yield return new WaitForSeconds(sliderUpdateInterval);
            }
        }
    }

    void Die()
    {
        //Destroy(gameObject);
    }
}
