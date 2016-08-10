/*
**  Bullet.cs: Moves the gameobject forward by a specified speed until its lifetime ends.
*/

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Bullet : Projectile
{
    //How fast the bullet will move
    public float moveSpeed = 10f;

    //Rotation is a SyncVar so that it is synced when the bullet is first spawned, 
    //thus removing the need to sync bullet position since it will not change direction.
    [SyncVar]
    [HideInInspector] //Public because it is set externally, but should not show in inspector
    public Vector3 initialRotation;

    public override void Start()
    {
        base.Start();

        transform.eulerAngles = initialRotation;
    }

    public virtual void Update()
    {
        //Move the object forward by speed (local forward, as bullet rotation is set when spawned)
        transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
    }
}
