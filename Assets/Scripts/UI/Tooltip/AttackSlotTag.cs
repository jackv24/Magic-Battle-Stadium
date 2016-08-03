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
        Tooltip.instance.LoadData(slotIndex);
        Tooltip.instance.ShowTooltip(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Tooltip.instance.ShowTooltip(false);
    }
}
