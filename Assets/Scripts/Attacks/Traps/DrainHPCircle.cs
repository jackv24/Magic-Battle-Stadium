/*
**  DrainHPCircle.cs: Script for trap. Drain enemies health over time when stood inside it, and also slows them down.
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DrainHPCircle : MonoBehaviour
{
    //How much hp is drained per second
    public float drainRate = 2f;

    //Change the speed of a player that enters
    public float speedMultiplier = 0.5f;

    //How long after spawning until it is destroyed
    public float lifeTime = 10f;

    //The owner of this trap
    public GameObject owner;

    //List of player stats to affect
    private List<PlayerStats> playerStats = new List<PlayerStats>();

    void Start()
    {
        //Destroy gameobject after time
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //Add all player that enter, except for the owner
        if (other.tag == "Player" && other.gameObject != owner)
        {
            playerStats.Add(other.GetComponent<PlayerStats>());

            //Change speed by multiplier (usually damping)
            other.GetComponent<PlayerMove>().moveSpeed *= speedMultiplier;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player" && playerStats.Contains(other.GetComponent<PlayerStats>()))
        {
            playerStats.Remove(other.GetComponent<PlayerStats>());

            //Return speed to normal
            other.GetComponent<PlayerMove>().moveSpeed *= 1 / speedMultiplier;
        }
    }

    void OnDestroy()
    {
        //Return all speeds before destruction
        foreach (PlayerStats stats in playerStats)
        {
            stats.GetComponent<PlayerMove>().moveSpeed *= 1 / speedMultiplier;
        }
    }

    //Drains HP over time
    IEnumerator Drain()
    {
        //While there is still an owner
        while (owner)
        {
            yield return new WaitForSeconds(1 / drainRate);

            //Apply damage to every player in range
            foreach (PlayerStats stats in playerStats)
            {
                stats.ApplyDamage(1, owner.GetComponent<PlayerInfo>().username, "HP Drain Circle");
            }
        }

        //If owner no longer exists, destroy gameobject
        Destroy(gameObject);
    }

    //Called via sendmessage from playerattack
    void SetOwner(GameObject obj)
    {
        owner = obj;
        //Start drain coroutine after owner is set
        StartCoroutine("Drain");
    }
}
