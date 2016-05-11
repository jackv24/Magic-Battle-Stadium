/*
**  PlayerAttack.cs: Gets input and attacks in that direction
*/

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(PlayerStats))]
[NetworkSettings(channel = 1, sendInterval = 0)]
public class PlayerAttack : NetworkBehaviour
{
    //The time after which the next bullet can be fired
    public float nextAttackTime;

    //The projectile to spawn
    public GameObject[] projectilePrefabs;
    public int selectedAttack = 0;

    //attack direction input
    private Vector2 inputVector;
    //the last direction input - allows next bullet to be fired immediately if the direction changed
    private Vector3 oldDirection;

    private PlayerStats stats;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (!isLocalPlayer || !stats.isAlive)
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
                nextAttackTime = Time.time + projectilePrefabs[selectedAttack].GetComponent<Bullet>().fireTime;

                CmdFire(inputVector, selectedAttack);
            }
        }
    }

    //Returns one of four euler angle vectors based in the input vector direction
    Vector3 DirectionToRotation(Vector2 input)
    {
        if (input.y < 0)
            return new Vector3(0, 0, 270);
        else if (input.y > 0)
            return new Vector3(0, 0, 90);
        else if (input.x > 0)
            return new Vector3(0, 0, 0);
        else if (input.x < 0)
            return new Vector3(0, 0, 180);

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
    void CmdFire(Vector2 direction, int attack)
    {
        //Spawn projectile
        GameObject obj = (GameObject)Instantiate(projectilePrefabs[attack], transform.position, Quaternion.identity);

        Bullet bullet = obj.GetComponent<Bullet>();
        bullet.initialRotation = DirectionToRotation(direction);
        bullet.owner = gameObject;

        //Spawn bullet on the network
        NetworkServer.Spawn(obj);
    }
}
