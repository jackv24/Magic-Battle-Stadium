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

    void Start()
    {
        //Slots are updated when game starts
        UpdateSlots();
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

    void UpdateSlots()
    {
        PlayerAttack attack = null;

        if (GameManager.instance.localPlayer)
            attack = GameManager.instance.localPlayer.GetComponent<PlayerAttack>();

        //If the attack script was found
        if (attack)
        {
            //make sure the selected slot is withing the bounds of projectile array
            if (selectedSlot < attack.projectilePrefabs.Length)
            {
                //Set attack slot
                attack.selectedAttack = selectedSlot;
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
