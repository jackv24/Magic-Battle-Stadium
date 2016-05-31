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
    public Attack[] attackSet;
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
                //Use mana to attack. If not enough mana is left, attack cannot be performed
                if (stats.UseMana(attackSet[selectedAttack].manaCost))
                {
                    //Set next attack time
                    nextAttackTime = Time.time + attackSet[selectedAttack].coolDownTime;

                    CmdFire(inputVector, selectedAttack);
                }
            }
        }
    }

    public bool SelectSlot(int slotIndex)
    {
        if (slotIndex < attackSet.Length)
        {
            //If the attack is a projectile, select it
            if (attackSet[slotIndex].type == Attack.Type.Projectile)
                selectedAttack = slotIndex;
            //If the attack is a trap, it shouldn't be selected, and instead should be used immediately
            else if (attackSet[slotIndex].type == Attack.Type.Trap)
            {
                //Only use attack if there is enough mana
                if(stats.UseMana(attackSet[slotIndex].manaCost))
                    CmdFire(Vector2.zero, slotIndex);

                //Attack was not selected (it was used immediately instead)
                return false;
            }

            //Reset attack time
            nextAttackTime = 0;

            //Attack was selected
            return true;
        }
        //If index is out of bounds return false
        else
            return false;
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
        GameObject obj = (GameObject)Instantiate(attackSet[attack].attackPrefab, transform.position, Quaternion.identity);

        //Projectiles have a direction
        if (direction != Vector2.zero)
        {
            Bullet bullet = obj.GetComponent<Bullet>();
            bullet.initialRotation = DirectionToRotation(direction);
        }

        //Sets the owner no matter what script (must have method though)
        obj.SendMessage("SetOwner", gameObject);

        //Spawn bullet on the network
        NetworkServer.Spawn(obj);
    }
}
