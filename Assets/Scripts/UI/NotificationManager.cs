/*
**  NotificationManager.cs: A custom notification system. Displays a notice in the UI when called.
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NotificationManager : MonoBehaviour
{
    //The prefab to instantiate on game start
    public GameObject noticePrefab;

    //How long until a notice disappears
    public float displayTime = 5.0f;

    //private references to relevent notice gameobject components
    private GameObject notice;
    private Text iconText;
    private Text descriptionText;

    //static instance for easy access (there should only ever be one anyway)
    public static NotificationManager instance;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        //Instantiate, name, and parent this notice to the main canvas
        notice = Instantiate(noticePrefab);
        notice.name = noticePrefab.name;
        notice.transform.SetParent(GameObject.FindWithTag("MainCanvas").transform, false);

        //Relying on names may not be the best idea, but it works well enough
        iconText = notice.transform.FindChild("IconText").GetComponent<Text>();
        descriptionText = notice.transform.FindChild("Text").GetComponent<Text>();

        //Start with this notice hidden
        notice.SetActive(false);
    }

    //Displays a notice with the first string as a large icon (prefarrably one character), and the second string as the text
    public void ShowNotice(string icon, string text)
    {
        //Set notice text values
        iconText.text = icon;
        descriptionText.text = text;

        //enable gameobject
        notice.SetActive(true);

        //Reset and start coutdown until notice disappears
        StopCoroutine("Disable");
        StartCoroutine("Disable");
    }

    //Wairs for some time then hides the notice
    IEnumerator Disable()
    {
        yield return new WaitForSeconds(displayTime);

        notice.SetActive(false);
    }
}
