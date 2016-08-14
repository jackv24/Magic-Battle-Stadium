/*
**  SaveInputField.cs: Save the value of an input field to a playerprefs string using the specified key, whenever the value is changed
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SaveInputField : MonoBehaviour
{
    //The key to save this input field's value as
    public string key;

    //The InputField component atached to this object
    private InputField field;

    void Awake()
    {
        field = GetComponent<InputField>();
    }

    void Start()
    {
        //If the key already exists, load it into the field
        if (PlayerPrefs.HasKey(key))
        {
            //Character limit must be temporarily increased to prevent trancating loaded value
            field.characterLimit++;
            //Load value from playerprefs
            field.text = PlayerPrefs.GetString(key);
            //Return character limit to previous amount
            field.characterLimit--;
        }

        //Add a listener for when the value of the field changes
        field.onValueChanged.AddListener(delegate { SaveFieldText(); });
    }

    //Saves the value of the InputField to the specified key in playerprefs
    void SaveFieldText()
    {
        PlayerPrefs.SetString(key, field.text);
    }
}
