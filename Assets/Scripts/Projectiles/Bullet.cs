/*
**  Bullet.cs: Moves the gameobject forward by a specified speed until its lifetime ends.
*/

using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    //How fast the bullet will move
    public float moveSpeed = 10f;
    //The amount of seconds after spawning until the bullet will be destroyed
    public float lifeTime = 5f;

    //The amount of damage that will be applied when the bullet collides with a character
    public int damage = 10;

    void Start()
    {
        //Set object to be destroyed after its lifetime ends
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        //Move the object forward by speed (local forward, as bullet rotation is set when spawned)
        transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        CharacterStats stats = col.gameObject.GetComponent<CharacterStats>();

        //if the object that was collided with has stats
        if(stats)
        {
            //Apply damage
            stats.ApplyDamage(damage);

            //destroy bullet
            Destroy(gameObject);
        }
    }
}
