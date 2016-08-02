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

    public int currentMana = 100;
    public int maxMana = 100;

    public float manaRegenInterval = 0.5f;

    [SyncVar]
    public bool isAlive = true;

    //How long it takes to respawn after dying
    public float respawnTime = 5.0f;
    private Vector3 respawnPos;

    //UI objects
    public Slider healthSlider;
    private Text healthSliderText;

    public Slider manaSlider;
    private Text manaSliderText;

    public float sliderUpdateInterval = 0.1f;

    private Text deadText;
    private string deadTextString;

    private PlayerInfo info;
    private PlayerAttack attack;

    void Awake()
    {
        info = GetComponent<PlayerInfo>();
        attack = GetComponent<PlayerAttack>();
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

            manaSlider = GameObject.Find("ManaBar").GetComponent<Slider>();
            manaSliderText = manaSlider.GetComponentInChildren<Text>();
        }

        //Slider is updated using a coroutine (for performance)
        StartCoroutine("UpdateSlider");
        StartCoroutine("RegenerateMana");
    }

    //Bullet collisions are triggered when they enter the trigger
    //since the player has both a wall collider and a bullet trigger
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Projectile")
        {
            Projectile proj = other.GetComponent<Projectile>();

            //Only collide with enemies, not the owner
            if(proj.owner != gameObject)
                proj.Collide(this);
        }
    }

    public void ApplyDamage(int amount, string attackerName, string attackName)
    {
        //Damage can only be applied on the server
        if (!isServer)
            return;

        if (currentHealth > 0)
        {
            currentHealth -= amount;

            if (currentHealth <= 0)
                RpcDie(attackerName, attackName);
        }
    }

    //Heal is run on the server
    [Command]
    public void CmdHeal(int amount)
    {
        currentHealth += amount;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    //Removes mana, returning true if there was enough mana left
    public bool UseMana(int amount)
    {
        //Only use mana if there is enough
        if (currentMana >= amount)
        {
            currentMana -= amount;

            return true;
        }
        else //Else return false as there is not enough mana
            return false;
    }

    IEnumerator RegenerateMana()
    {
        while (true)
        {
            yield return new WaitForSeconds(manaRegenInterval);

            if(currentMana < maxMana)
                currentMana += 1;
        }
    }

    //Slider doesn't need to be updated every frame. Coruotine should save performance.
    IEnumerator UpdateSlider()
    {
        while(true)
        {
            if (healthSlider)
            {
                healthSlider.value = (float)currentHealth / maxHealth;

                if (healthSliderText)
                    healthSliderText.text = currentHealth + "/" + maxHealth;
            }

            if (manaSlider)
            {
                manaSlider.value = (float)currentMana / maxMana;

                if (manaSliderText)
                    manaSliderText.text = currentMana + "/" + maxMana;
            }

            //Update at fixed time intervals (not updating every frame saves performance)
            yield return new WaitForSeconds(sliderUpdateInterval);
        }
    }

    //Teh server calls the die command on the client (client shows text)
    [ClientRpc]
    void RpcDie(string attackerName, string attackName)
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

        //If a scoreboard exists
        if (Scoreboard.instance)
        {
            //Add death to scoreboard
            Scoreboard.instance.AddDeath(info.username);
            //Add kill to scoreboard
            Scoreboard.instance.AddKill(attackerName);
        }

        //If the kill text display exists
        if (DisplayKillText.instance)
        {
            //Add a kill to the killtext, with player name, attacker name, and the name of the attack that killed it
            DisplayKillText.instance.AddKill(attackerName, info.username, attackName);
        }
    }

    //Respawning is handled by server
    [Command]
    void CmdRespawn()
    {
        //Reset values
        currentHealth = maxHealth;
        currentMana = maxMana;
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

        attack.ResetCooldowns();
    }

    void OnDestroy()
    {
        //Remove player from local scoreboard (should be called on every client as the player is destroyed on every client)
        Scoreboard.instance.RemovePlayer(info.username);
    }
}
