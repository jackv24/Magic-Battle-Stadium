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

    public float lifeTime = 10f;

    //The player who shot this bullet - they should not be damaged
    [SyncVar]
    public GameObject owner;

    public virtual void Start()
    {
        //Set object to be destroyed after its lifetime ends
        Destroy(gameObject, lifeTime);
    }

    public void Collide(PlayerStats stats)
    {
        //if the object that was collided with has stats, and is alive
        if (stats && stats.isAlive)
        {
            //Apply damage (name of bullet owner is also sent to identify who killed who)
            stats.ApplyDamage(damage, owner.GetComponent<PlayerInfo>().username, projectileName);

            //destroy bullet
            Destroy(gameObject);
        }
    }

    //Called via sendmessage from playerattack
    void SetOwner(GameObject obj)
    {
        owner = obj;
    }
}
