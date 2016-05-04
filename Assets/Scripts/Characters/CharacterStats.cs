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

    private ShowText deadText;
    private Text healthSliderText;

    void Start()
    {
        //if this is the local play, the health bar is in the HUD
        if (isLocalPlayer)
        {
            deadText = GameObject.Find("DeadText").GetComponent<ShowText>();

            //Deactivate health bar below player
            healthSlider.gameObject.SetActive(false);

            //Find the HUD health slider, and the child text component
            healthSlider = GameObject.Find("HealthBar").GetComponent<Slider>();
            healthSliderText = healthSlider.GetComponentInChildren<Text>();
        }

        //Slider is updated using a coroutine (for performance)
        StartCoroutine("UpdateSlider");
    }

    public void ApplyDamage(int amount)
    {
        //Damage can only be applied on the server
        if (!isServer)
            return;

        currentHealth -= amount;

        if (currentHealth <= 0)
            RpcDie();

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

                if (healthSliderText)
                    healthSliderText.text = currentHealth + "/" + maxHealth;

                //Update at fixed time intervals (not updating every frame saves performance)
                yield return new WaitForSeconds(sliderUpdateInterval);
            }
        }
    }

    [ClientRpc]
    void RpcDie()
    {
        if(deadText)
            deadText.Show();

        //Finally, set this gameobject inactive (don't delete, since we want to keep player info)
        gameObject.SetActive(false);
    }
}
