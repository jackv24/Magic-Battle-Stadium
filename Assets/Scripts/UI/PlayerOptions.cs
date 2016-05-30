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

    //Loads colours from playerprefs. If there is nothing in playerprefs, default slider value is used
    void LoadColours()
    {
        hatSliders.red.value = PlayerPrefs.GetFloat("hatR", hatSliders.red.value);
        hatSliders.green.value = PlayerPrefs.GetFloat("hatG", hatSliders.green.value);
        hatSliders.blue.value = PlayerPrefs.GetFloat("hatB", hatSliders.blue.value);

        clothesSliders.red.value = PlayerPrefs.GetFloat("clothesR", clothesSliders.red.value);
        clothesSliders.green.value = PlayerPrefs.GetFloat("clothesG", clothesSliders.green.value);
        clothesSliders.blue.value = PlayerPrefs.GetFloat("clothesB", clothesSliders.blue.value);

        skinSliders.red.value = PlayerPrefs.GetFloat("skinR", skinSliders.red.value);
        skinSliders.green.value = PlayerPrefs.GetFloat("skinG", skinSliders.green.value);
        skinSliders.blue.value = PlayerPrefs.GetFloat("skinB", skinSliders.blue.value);
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
}

[System.Serializable]
public class SliderRGB
{
    public Slider red;
    public Slider green;
    public Slider blue;
}