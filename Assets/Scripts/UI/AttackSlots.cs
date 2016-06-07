/*
**  AttackSlots.cs: Displays the attack slots, showing which is selected
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class AttackSlots : MonoBehaviour
{
    [System.Serializable]
    public class Slot
    {
        public Image image;
        public Image icon;
        public Text text;
        public Image cooldownImage;
    }
    public Slot[] slots;

    public int selectedSlot = 0;

    public Color selectedColor = Color.white;
    public Color deselectedColor = Color.gray;

    private PlayerAttack attack;

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
    public void InitialiseSlots()
    {
        attack = GameManager.instance.LocalPlayer.GetComponent<PlayerAttack>();

        //Iterate through all display slots
        for (int i = 0; i < slots.Length; i++)
        {
            //If the attack slot on the player is filled
            if (i < attack.attackSet.Length)
            {
                //Update slot text
                slots[i].text.text = string.Format(slots[i].text.text, i + 1, attack.attackSet[i].attack.manaCost);
                slots[i].icon.sprite = attack.attackSet[i].attack.slotIcon;
                slots[i].icon.color = Color.white;
            }
            else
            {
                //Clear slot text
                slots[i].text.text = "";
                slots[i].icon.color = Color.clear;
            }

            slots[i].cooldownImage.fillAmount = 0;
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
            //Attempt to select slot. If unsuccessful, keep currently selected slot
            if(!attack.SelectSlot(selectedSlot))
                selectedSlot = attack.selectedAttack;
        }

        //Deselect all slots
        foreach (Slot slot in slots)
        {
            slot.image.color = deselectedColor;
        }

        //Select current slot
        slots[selectedSlot].image.color = selectedColor;
    }

    //Public method called externally to start coroutine
    public void StartCooldown(int slotIndex)
    {
        StartCoroutine("DisplayCooldown", slotIndex);
    }

    //Coruotine to animate the cooldown timer display of a particular slot
    IEnumerator DisplayCooldown(int slotIndex)
    {
        //Store reference to cooldown image
        Image cooldown = slots[slotIndex].cooldownImage;

        //Start countdown fill at max
        cooldown.fillAmount = 1f;

        float elapsedTime = 0;
        float cooldownTime = attack.attackSet[slotIndex].attack.coolDownTime;

        while (elapsedTime <= cooldownTime)
        {
            //Lerp fill amount from 1 to 0 based on elapsed time
            cooldown.fillAmount = Mathf.Lerp(1f, 0, elapsedTime / cooldownTime);

            //Increase elapsed time by deltatime and then return to start of next frame
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        //Make sure fill amunt is zero
        cooldown.fillAmount = 0;
    }
}
