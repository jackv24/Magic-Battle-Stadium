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

    void Start()
    {
        deadText = GameObject.Find("DeadText").GetComponent<ShowText>();

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

                yield return new WaitForSeconds(sliderUpdateInterval);
            }
        }
    }

    [ClientRpc]
    void RpcDie()
    {
        if(isLocalPlayer)
            deadText.Show();

        //Finally, set this gameobject inactive (don't delete, since we want to keep player info)
        gameObject.SetActive(false);
    }
}
