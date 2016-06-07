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
    [System.Serializable]
    public struct AttackInfo
    {
        public Attack attack;
        public float nextAttackTime;
    }

    public AttackInfo[] attackSet;

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
            if (Time.time > attackSet[selectedAttack].nextAttackTime)
            {
                //Use mana to attack. If not enough mana is left, attack cannot be performed
                if (stats.UseMana(attackSet[selectedAttack].attack.manaCost))
                {
                    //Set next attack time
                    attackSet[selectedAttack].nextAttackTime = Time.time + attackSet[selectedAttack].attack.coolDownTime;
                    GameManager.instance.attackSlots.StartCooldown(selectedAttack);

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
            if (attackSet[slotIndex].attack.type == Attack.Type.Projectile)
                selectedAttack = slotIndex;
            //If the attack is a trap, it shouldn't be selected, and instead should be used immediately
            else if (attackSet[slotIndex].attack.type == Attack.Type.Trap)
            {
                //Only use attack if there is enough mana and it is not on cooldown
                if (Time.time > attackSet[slotIndex].nextAttackTime && stats.UseMana(attackSet[slotIndex].attack.manaCost))
                {
                    //Set next cooldown time for this attack
                    attackSet[slotIndex].nextAttackTime = Time.time + attackSet[slotIndex].attack.coolDownTime;
                    GameManager.instance.attackSlots.StartCooldown(slotIndex);

                    CmdFire(Vector2.zero, slotIndex);
                }

                //Attack was not selected (it was used immediately instead)
                return false;
            }
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
        GameObject obj = (GameObject)Instantiate(attackSet[attack].attack.attackPrefab, transform.position, Quaternion.identity);

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
