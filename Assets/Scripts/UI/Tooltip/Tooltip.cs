/*
**  Tooltip.cs: Controls a tooltip which shows information about an attack
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    //Static instance for easy access
    public static Tooltip instance;

    //Offset from the mouse position
    public Vector2 offset;
    private bool isActive = true;

    //Text abjects to display attack info
    public Text title;
    public Text description;

    //Accounts for difference between mouse origin and UI origin
    private Vector3 screenOffset;
    //The bottom of the screen
    private float minY;

    private RectTransform rectTrans;
    //Keep track of changes to screen height
    private float screenHeight;

    private AttackSet attackSet;

    void Awake()
    {
        instance = this;

        rectTrans = GetComponent<RectTransform>();
    }

    void Start()
    {
        //Start tooltip hidden
        ShowTooltip(false);
    }

    void Update()
    {
        //If the tooltip is active
        if (isActive)
        {
            //Update values if screen height has changed
            if (screenHeight != Screen.height)
            {
                screenHeight = Screen.height;

                screenOffset = new Vector3(Screen.width / 2 + offset.x, Screen.height / 2 + offset.y, transform.position.z);
                minY = (Screen.height / 2) * -1;
            }

            //Move tooltip to mouse position
            transform.localPosition = Input.mousePosition - screenOffset;

            //If tooltip is off the bottom of the screen...
            if (transform.localPosition.y - rectTrans.rect.height < minY)
            {
                //...move it up so the whole tooltip is visible
                Vector3 pos = transform.localPosition;
                pos.y = minY + rectTrans.rect.height;
                transform.localPosition = pos;
            }
        }
    }

    //Shows or hides the tooltip
    public void ShowTooltip(bool state)
    {
        isActive = state;

        //Only hide children so this script still runs
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(isActive);
        }
    }

    //Load attack data from current attack set, using attack index
    public void LoadData(int index)
    {
        //Get current attack set
        attackSet = GameManager.instance.attackSets[PlayerPrefs.GetInt("AttackSet", 0)];

        if (attackSet)
        {
            //An index of less than zero means to display the attack set info instead
            if (index < 0)
            {
                title.text = attackSet.setName;
                description.text = attackSet.description;
            }
            else
            {
                //Get selected attack
                Attack selectedAttack = attackSet.attacks[index];

                if (selectedAttack)
                {
                    //Set data
                    title.text = selectedAttack.attackName;
                    description.text = selectedAttack.description;
                }
            }
        }
    }
}
