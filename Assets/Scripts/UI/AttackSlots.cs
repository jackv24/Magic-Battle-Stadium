/*
**  AttackSlots.cs: Displays the attack slots, showing which is selected
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class AttackSlots : MonoBehaviour
{
    //UI images for slots
    public Image[] slots;
    public int selectedSlot = 0;

    public Color selectedColor = Color.white;
    public Color deselectedColor = Color.gray;

    private PlayerAttack attack;

    void Start()
    {
        GameManager.instance.attackSlots = this;
    }

    void Update()
    {
        //If a number key (1-4) is pressed, set that to be the current attack slot
        if (Input.GetButtonDown("Attack1"))
            selectedSlot = 0;
        else if (Input.GetButtonDown("Attack2"))
            selectedSlot = 1;
        else if (Input.GetButtonDown("Attack3"))
            selectedSlot = 2;
        else if (Input.GetButtonDown("Attack4"))
            selectedSlot = 3;
        else
            //if no button was pressed, do nothing
            return;

        //Update slots
        UpdateSlots();

    }

    //Sets appropriate images and text
    public void InitialiseSlots(PlayerAttack playerAttack)
    {
        attack = playerAttack;

        //Iterate through all display slots
        for (int i = 0; i < slots.Length; i++)
        {
            //Should have a single text object in child
            Text slotsText = slots[i].GetComponentInChildren<Text>();

            //If the attack slot on the player is filled
            if (i < attack.projectilePrefabs.Length)
                //Update slot text
                slotsText.text = string.Format(slotsText.text, i + 1, attack.projectilePrefabs[i].GetComponent<Bullet>().manaCost);
            else
                //Clear slot text
                slotsText.text = "";
        }

        //Update the selected slot
        UpdateSlots();
    }

    //Updates which slot is selected
    void UpdateSlots()
    {
        //If the attack script was found
        if (attack)
        {
            //make sure the selected slot is withing the bounds of projectile array
            if (selectedSlot < attack.projectilePrefabs.Length)
            {
                //Set attack slot
                attack.selectedAttack = selectedSlot;
                //Next attack time should be reset when a new attack is selected
                attack.nextAttackTime = 0;
            }
            else //if the selected slot is out of bounds
                //Don't change slot (and keep display)
                selectedSlot = attack.selectedAttack;
        }

        //Deselect all slots
        foreach (Image slot in slots)
        {
            slot.color = deselectedColor;
        }

        //Select current slot
        slots[selectedSlot].color = selectedColor;
    }
}
