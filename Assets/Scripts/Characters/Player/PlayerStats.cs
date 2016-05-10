/*
**  CharacterStats.cs: Hold stats for the character it is attached to
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerStats : NetworkBehaviour
{
    [SyncVar]
    public int currentHealth = 100;
    public int maxHealth = 100;

    [SyncVar]
    public bool isAlive = true;

    //How long it takes to respawn after dying
    public float respawnTime = 5.0f;
    private Vector3 respawnPos;

    //UI objects
    public Slider healthSlider;
    public float sliderUpdateInterval = 0.1f;

    private Text deadText;
    private string deadTextString;

    private Text healthSliderText;

    private PlayerInfo info;

    void Awake()
    {
        info = GetComponent<PlayerInfo>();
    }

    void Start()
    {
        //if this is the local play, the health bar is in the HUD
        if (isLocalPlayer)
        {
            deadText = GameObject.Find("DeadText").GetComponent<Text>();
            deadTextString = deadText.text;

            //Deactivate health bar below player
            healthSlider.gameObject.SetActive(false);

            //Find the HUD health slider, and the child text component
            healthSlider = GameObject.Find("HealthBar").GetComponent<Slider>();
            healthSliderText = healthSlider.GetComponentInChildren<Text>();
        }

        //Slider is updated using a coroutine (for performance)
        StartCoroutine("UpdateSlider");
    }

    public void ApplyDamage(int amount, string attackerName)
    {
        //Damage can only be applied on the server
        if (!isServer)
            return;

        if (currentHealth > 0)
        {
            currentHealth -= amount;

            if (currentHealth <= 0)
                RpcDie(attackerName);
        }
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

    //Teh server calls the die command on the client (client shows text)
    [ClientRpc]
    void RpcDie(string attackerName)
    {
        if (deadText)
        {
            deadText.enabled = true;
        }

        //set alive to be false (stop movement scripts and such)
        isAlive = false;

        if (isLocalPlayer)
        {
            StartCoroutine("RespawnTimer", respawnTime);
        }

        if (Scoreboard.instance)
        {
            //Add death to scoreboard
            Scoreboard.instance.AddDeath(info.username);
            //Add kill to scoreboard
            Scoreboard.instance.AddKill(attackerName);
        }
    }

    //Respawning is handled by server
    [Command]
    void CmdRespawn()
    {
        //Reset values
        currentHealth = maxHealth;
        //Store server respawn pos so that client also starts in that position
        respawnPos = NetworkManager.singleton.GetStartPosition().position;
        isAlive = true;
    }

    //Count down and then respawn (reflecting time in UI)
    IEnumerator RespawnTimer(float seconds)
    {
        float timeLeft = seconds;

        while (true)
        {
            if (deadText)
            {
                deadText.text = string.Format(deadTextString, timeLeft);
            }

            //Wait for one second and remove 1 second from timeleft
            yield return new WaitForSeconds(1f);
            timeLeft -= 1;

            //if there is no time left, break the loop
            if (timeLeft <= 0)
                break;
        }

        //Call server command to respawn
        CmdRespawn();
        //hide dead text
        deadText.enabled = false;
        transform.position = respawnPos;
    }
}
