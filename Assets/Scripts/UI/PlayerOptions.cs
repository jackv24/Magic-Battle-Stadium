/*
**  PlayerOptions.cs: Manages the UI for player options
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerOptions : MonoBehaviour
{
    //Collections of sliders for graphics
    public SliderRGB hatSliders;
    public SliderRGB clothesSliders;
    public SliderRGB skinSliders;

    //Collection of player graphics
    [System.Serializable]
    public class PlayerGraphics
    {
        public SpriteRenderer hat;
        public SpriteRenderer clothes;
        public SpriteRenderer skin;
    }
    public PlayerGraphics playerGraphics;

    [System.Serializable]
    public class Slot
    {
        public Image image;
        public Text text;
    }
    public Slot[] slots;
    private string slotsTextString;

    //Attack sets
    public Text attackSetTitle;
    private string attackSetTitleString;

    public Text attackSetSubtitle;

    private AttackSet[] attackSets;
    private int currentAttackSet;

    void Start()
    {
        //Load existing colours
        LoadColours();

        //Initialise sliders
        Initialise(hatSliders);
        Initialise(clothesSliders);
        Initialise(skinSliders);

        //Update colours
        UpdateColours();

        slotsTextString = slots[0].text.text;

        attackSetTitleString = attackSetTitle.text;

        attackSets = GameManager.instance.attackSets;
        currentAttackSet = PlayerPrefs.GetInt("AttackSet", 0);
        DisplayAttackSet();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            PlayerPrefs.DeleteAll();
    }

    //Sets listeners on sliders
    void Initialise(SliderRGB sliders)
    {
        sliders.red.onValueChanged.AddListener(delegate { UpdateColours(); });
        sliders.green.onValueChanged.AddListener(delegate { UpdateColours(); });
        sliders.blue.onValueChanged.AddListener(delegate { UpdateColours(); });
    }

    //Updates and saves colours
    void UpdateColours()
    {
        //Set graphic colours to slider values
        playerGraphics.hat.color = new Color(hatSliders.red.value, hatSliders.green.value, hatSliders.blue.value);
        playerGraphics.clothes.color = new Color(clothesSliders.red.value, clothesSliders.green.value, clothesSliders.blue.value);
        playerGraphics.skin.color = new Color(skinSliders.red.value, skinSliders.green.value, skinSliders.blue.value);

        //Save colours
        SaveColours();
    }

    //Loads colours from playerprefs. If there is nothing in playerprefs, random values are used
    void LoadColours()
    {
        hatSliders.red.value = PlayerPrefs.GetFloat("hatR", Random.Range(0, 1f));
        hatSliders.green.value = PlayerPrefs.GetFloat("hatG", Random.Range(0, 1f));
        hatSliders.blue.value = PlayerPrefs.GetFloat("hatB", Random.Range(0, 1f));

        clothesSliders.red.value = PlayerPrefs.GetFloat("clothesR", Random.Range(0, 1f));
        clothesSliders.green.value = PlayerPrefs.GetFloat("clothesG", Random.Range(0, 1f));
        clothesSliders.blue.value = PlayerPrefs.GetFloat("clothesB", Random.Range(0, 1f));

        skinSliders.red.value = PlayerPrefs.GetFloat("skinR", Random.Range(0, 1f));
        skinSliders.green.value = PlayerPrefs.GetFloat("skinG", Random.Range(0, 1f));
        skinSliders.blue.value = PlayerPrefs.GetFloat("skinB", Random.Range(0, 1f));
    }

    //Save slider values to playerprefs
    void SaveColours()
    {
        PlayerPrefs.SetFloat("hatR", hatSliders.red.value);
        PlayerPrefs.SetFloat("hatG", hatSliders.green.value);
        PlayerPrefs.SetFloat("hatB", hatSliders.blue.value);

        PlayerPrefs.SetFloat("clothesR", clothesSliders.red.value);
        PlayerPrefs.SetFloat("clothesG", clothesSliders.green.value);
        PlayerPrefs.SetFloat("clothesB", clothesSliders.blue.value);

        PlayerPrefs.SetFloat("skinR", skinSliders.red.value);
        PlayerPrefs.SetFloat("skinG", skinSliders.green.value);
        PlayerPrefs.SetFloat("skinB", skinSliders.blue.value);
    }

    //Generate and set random slider values
    public void RandomiseColours()
    {
        hatSliders.red.value = Random.Range(0, 1f);
        hatSliders.green.value = Random.Range(0, 1f);
        hatSliders.blue.value = Random.Range(0, 1f);

        clothesSliders.red.value = Random.Range(0, 1f);
        clothesSliders.green.value = Random.Range(0, 1f);
        clothesSliders.blue.value = Random.Range(0, 1f);

        skinSliders.red.value = Random.Range(0, 1f);
        skinSliders.green.value = Random.Range(0, 1f);
        skinSliders.blue.value = Random.Range(0, 1f);

        //Save colours
        SaveColours();
    }

    //Updates the display of attack slots with selected attack set
    void DisplayAttackSet()
    {
        AttackSet set = attackSets[currentAttackSet];

        //Update titles with set info
        attackSetTitle.text = string.Format(attackSetTitleString, set.setName);
        attackSetSubtitle.text = string.Format("{0} of {1}", currentAttackSet + 1, attackSets.Length);

        //Update slots with attack info
        for (int i = 0; i < slots.Length; i++)
        {
            if (set.attacks[i] != null)
            {
                slots[i].image.color = Color.white;
                slots[i].image.sprite = set.attacks[i].slotIcon;
                slots[i].text.text = string.Format(slotsTextString, i + 1, set.attacks[i].manaCost);
            }
            else
            {
                slots[i].image.color = Color.clear;
                slots[i].text.text = "";
            }
        }

        //Save selected attack
        PlayerPrefs.SetInt("AttackSet", currentAttackSet);

        //Tell gamemanager what set the player will use
        GameManager.instance.currentAttackSet = currentAttackSet;
    }

    //Changes which attack set is currently selected (next or previous by how many)
    public void MoveAttackSet(int distance)
    {
        int index = currentAttackSet + distance;

        //If outside of bounds, wrap around
        if (index >= attackSets.Length)
            index = 0;
        else if (index < 0)
            index = attackSets.Length - 1;

        currentAttackSet = index;
        DisplayAttackSet();
    }
}

[System.Serializable]
public class SliderRGB
{
    public Slider red;
    public Slider green;
    public Slider blue;
}