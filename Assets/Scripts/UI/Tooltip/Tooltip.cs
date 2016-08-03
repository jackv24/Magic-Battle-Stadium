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

    private Camera cam;
    private Vector3 screenOffset;

    private AttackSet attackSet;

    void Awake()
    {
        instance = this;

        cam = Camera.main;
    }

    void Start()
    {
        screenOffset = new Vector3(Screen.width / 2 + offset.x, Screen.height / 2 + offset.y, transform.position.z);

        ShowTooltip(false);
    }

    void Update()
    {
        if(isActive)
            transform.localPosition = Input.mousePosition - screenOffset;
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
