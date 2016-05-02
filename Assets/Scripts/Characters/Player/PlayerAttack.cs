/*
**  PlayerAttack.cs: Gets input and attacks in that direction
*/

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerAttack : NetworkBehaviour
{
    //The delay between each consecutive attack
    public float attackTime = 0.5f;
    //The time after which the next bullet can be fired
    private float nextAttackTime;

    //The projectile to spawn
    public GameObject projectilePrefab;

    //attack direction input
    private Vector2 inputVector;
    //the last direction input - allows next bullet to be fired immediately if the direction changed
    private Vector3 oldDirection;

    void Update()
    {
        if (!isLocalPlayer)
            return;

        //Get input
        inputVector = new Vector2(Input.GetAxisRaw("Attack Horizontal"), Input.GetAxisRaw("Attack Vertical"));

        //If a direction is being pressed
        if (inputVector != Vector2.zero)
        {
            //If the next attack time has passed OR the driection has changed
            if (Time.time > nextAttackTime)
            {
                //Set next attack time
                nextAttackTime = Time.time + attackTime;

                CmdFire(inputVector);
            }
        }
    }

    //Returns one of four euler angle vectors based in the input vector direction
    Vector3 DirectionToRotation(Vector2 input)
    {
        if (input.y < 0)
            return new Vector3(0, 0, 270);
        else if (input.x > 0)
            return new Vector3(0, 0, 0);
        else if (input.x < 0)
            return new Vector3(0, 0, 180);
        else if (input.y > 0)
            return new Vector3(0, 0, 90);

        return Vector3.zero;
    }

    //returns true if the direction of attack has changed
    bool DirectionChanged()
    {
        if (oldDirection != DirectionToRotation(inputVector))
        {
            oldDirection = DirectionToRotation(inputVector);
            return true;
        }

        return false;
    }

    //Fire command executed on server (passed direction by client)
    [Command]
    void CmdFire(Vector2 direction)
    {
        //Spawn projectile
        GameObject obj = (GameObject)Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        Bullet bullet = obj.GetComponent<Bullet>();
        bullet.rotation = DirectionToRotation(direction);
        bullet.owner = gameObject;

        //Spawn bullet on the network
        NetworkServer.Spawn(obj);
    }
}
