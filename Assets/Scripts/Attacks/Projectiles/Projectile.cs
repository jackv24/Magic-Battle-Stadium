/*
**  Projectile.cs: Base class for projectiles. Handles collision, damage, etc.
*/

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Projectile : NetworkBehaviour
{
    public string projectileName;

    public int damage = 5;

    public GameObject hitEffect;

    public int health = 0;
    public float lifeTime = 10f;

    public bool destroyOnPlayerHit = true;
    public bool damagedByProjectiles = true;

    //The player who shot this bullet - they should not be damaged
    [SyncVar]
    public GameObject owner;

    public virtual void Start()
    {
        //Projectile can absorb as much damage as it can deal
        health = damage;

        //Set object to be destroyed after its lifetime ends
        Destroy(gameObject, lifeTime);
    }

    //Player collision handled by player trigger
    public void Collide(PlayerStats stats)
    {
        //if the object that was collided with has stats, and is alive
        if (isServer && stats && stats.isAlive)
        {
            //Apply damage (name of bullet owner is also sent to identify who killed who)
            stats.CmdApplyDamage(damage, owner.GetComponent<PlayerInfo>().username, projectileName);

            //Projectile expends it's health when colliding with a player
            if(destroyOnPlayerHit)
                health = 0;
        }

        if (hitEffect)
        {
            //Instantiate hit effect - Spawn effect on player if the projectile is not destroyed, else at projectile pos.
            GameObject obj = (GameObject)Instantiate(hitEffect, (destroyOnPlayerHit || !stats) ? transform.position : stats.transform.position - (Vector3.up / 2), Quaternion.identity);
            Destroy(obj, 2f);
        }

        //Destroy bullet if no health left
        if(health <= 0)
            Destroy(gameObject);
    }

    //Called via sendmessage from playerattack
    void SetOwner(GameObject obj)
    {
        owner = obj;
    }

    //Enables collision with things that aren't players
    void OnCollisionEnter2D(Collision2D col)
    {
        //Can not collide with owner
        if (col.gameObject != owner)
        {
            if(damagedByProjectiles)
                //Projectile collision damages health
                health -= col.gameObject.GetComponent<Projectile>().damage;

            Collide(null);
        }
    }
}
