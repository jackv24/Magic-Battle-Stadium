/*
**  AttackSlotTag.cs: Any UI object this script is attached to will display the attack toolwip when moused over
*/

using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class AttackSlotTag : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //Slot index that this represents
    public int slotIndex = 0;

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Pass slot data to load to the tooltip
        Tooltip.instance.LoadData(slotIndex);
        //Show the toolip
        Tooltip.instance.ShowTooltip(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //Hide the tooltip
        Tooltip.instance.ShowTooltip(false);
    }
}
