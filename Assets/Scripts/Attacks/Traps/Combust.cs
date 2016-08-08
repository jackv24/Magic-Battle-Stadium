/*
**  Combust.cs: Script for Comnbust attack
*/

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Combust : NetworkBehaviour
{
    public int damage = 25;

    //How long after spawning until it is destroyed
    public float lifeTime = 10f;

    //The owner of this trap
    [SyncVar]
    public GameObject owner;

    void Start()
    {
        //Destroy gameobject after time
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //Handle colisions only on the server
        if (!isServer)
            return;

        //Add all player that enter, except for the owner
        if (other.transform.parent.tag == "Player" && other.transform.parent.gameObject != owner)
        {
            PlayerStats playerStats = other.transform.parent.GetComponent<PlayerStats>();
            playerStats.CmdApplyDamage(damage, owner.GetComponent<PlayerInfo>().username, "Explosion");
        }
    }

    //Called via sendmessage from playerattack
    void SetOwner(GameObject obj)
    {
        owner = obj;
    }
}
