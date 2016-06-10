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
    public float[] nextAttackTime;

    public int currentAttackSet = 0;
    public AttackSet attackSet;

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

    void Start()
    {
        //Load current attack set if this is the local player
        //Attack set is blank for remote players since firing happens on the server anyway
        if (isLocalPlayer)
        {
            currentAttackSet = GameManager.instance.currentAttackSet;
            attackSet = GameManager.instance.attackSets[currentAttackSet];

            nextAttackTime = new float[attackSet.attacks.Length];
        }
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
            if (Time.time > nextAttackTime[selectedAttack])
            {
                //Use mana to attack. If not enough mana is left, attack cannot be performed
                if (stats.UseMana(attackSet.attacks[selectedAttack].manaCost))
                {
                    //Set next attack time
                    nextAttackTime[selectedAttack] = Time.time + attackSet.attacks[selectedAttack].coolDownTime;
                    GameManager.instance.attackSlots.StartCooldown(selectedAttack);

                    CmdFire(inputVector, selectedAttack, currentAttackSet);
                }
            }
        }
    }

    public bool SelectSlot(int slotIndex)
    {
        if (attackSet.attacks[slotIndex] != null)
        {
            //If the attack is a projectile, select it
            if (attackSet.attacks[slotIndex].type == Attack.Type.Projectile)
                selectedAttack = slotIndex;
            //If the attack is a trap, it shouldn't be selected, and instead should be used immediately
            else if (attackSet.attacks[slotIndex].type == Attack.Type.Trap || attackSet.attacks[slotIndex].type == Attack.Type.Spawn)
            {
                //Only use attack if there is enough mana and it is not on cooldown
                if (Time.time > nextAttackTime[slotIndex] && stats.UseMana(attackSet.attacks[slotIndex].manaCost))
                {
                    //Set next cooldown time for this attack
                    nextAttackTime[slotIndex] = Time.time + attackSet.attacks[slotIndex].coolDownTime;
                    GameManager.instance.attackSlots.StartCooldown(slotIndex);

                    //Should only spawn one unless spawn type defines more
                    for(int i = 0; i < attackSet.attacks[slotIndex].amountToSpawn; i++)
                        CmdFire(Vector2.zero, slotIndex, currentAttackSet);
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
    void CmdFire(Vector2 direction, int attack, int attackSet)
    {
        //Spawn projectile
        GameObject obj = (GameObject)Instantiate(GameManager.instance.attackSets[attackSet].attacks[attack].attackPrefab, transform.position, Quaternion.identity);

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
