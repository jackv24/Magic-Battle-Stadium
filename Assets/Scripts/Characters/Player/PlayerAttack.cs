/*
**  PlayerAttack.cs: Gets input and attacks in that direction
*/

using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
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
        //Get input
        inputVector = new Vector2(Input.GetAxisRaw("Attack Horizontal"), Input.GetAxisRaw("Attack Vertical"));

        //If a direction is being pressed
        if (inputVector != Vector2.zero)
        {
            //If the next attack time has passed OR the driection has changed
            if (Time.time > nextAttackTime || DirectionChanged())
            {
                //Set next attack time
                nextAttackTime = Time.time + attackTime;

                //Spawn projectile
                GameObject obj = (GameObject)Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                //Rotate projectile in the direction of input
                obj.transform.eulerAngles = InputToRotation(inputVector);

                //Make sure the bullet doesn't collide with the player who fired it
                Physics2D.IgnoreCollision(obj.GetComponent<Collider2D>(), GetComponent<Collider2D>());
            }
        }
    }

    //Returns one of four euler angle vectors based in the input vector direction
    Vector3 InputToRotation(Vector2 input)
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
        if (oldDirection != InputToRotation(inputVector))
        {
            oldDirection = InputToRotation(inputVector);
            return true;
        }

        return false;
    }
}
