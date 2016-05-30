/*
**  PlayerColor.cs: Loads the players colour and updates it across the network
*/

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerColour : NetworkBehaviour
{
    [System.Serializable]
    public class PlayerGraphics
    {
        public SpriteRenderer hat;
        public SpriteRenderer clothes;
        public SpriteRenderer skin;
    }

    //The sprite renderers to colour
    public PlayerGraphics graphics;

    //The range in which the generated colour will lie
    public float minSaturation = 0.25f;
    public float maxSaturation = 0.75f;

    //hooked to UpdateColour to change the sprite renderers when a player's colour changes
    [SyncVar(hook = "UpdateHatColor")]
    public Color hatColor;
    [SyncVar(hook = "UpdateClothesColor")]
    public Color clothesColor;
    [SyncVar(hook = "UpdateSkinColor")]
    public Color skinColor;

    void Start()
    {
        //If this is the local player
        if (isLocalPlayer)
        {
            //Get colour from playerprefs
            hatColor = new Color(PlayerPrefs.GetFloat("hatR"), PlayerPrefs.GetFloat("hatG"), PlayerPrefs.GetFloat("hatB"));
            clothesColor = new Color(PlayerPrefs.GetFloat("clothesR"), PlayerPrefs.GetFloat("clothesG"), PlayerPrefs.GetFloat("clothesB"));
            skinColor = new Color(PlayerPrefs.GetFloat("skinR"), PlayerPrefs.GetFloat("skinG"), PlayerPrefs.GetFloat("skinB"));

            //Update the colour on the server
            CmdUpdateColor(hatColor, clothesColor, skinColor);
        }
        else //If this is not the local player
        {
            //Only update the local color
            graphics.hat.color = hatColor;
            graphics.clothes.color = clothesColor;
            graphics.skin.color = skinColor;
        }
    }

    [Command]
    void CmdUpdateColor(Color hat, Color clothes, Color skin)
    {
        //Update color on server
        hatColor = hat;
        clothesColor = clothes;
        skinColor = skin;

        //Update colour on clients
        RpcUpdateColors(hatColor, clothesColor, skinColor);
    }

    //Hooks for colours
    void UpdateHatColor(Color color) { hatColor = color; }
    void UpdateClothesColor(Color color) { clothesColor = color; }
    void UpdateSkinColor(Color color) { skinColor = color; }

    //Call to update colours on every client, for every player
    [ClientRpc]
    void RpcUpdateColors(Color hat, Color clothes, Color skin)
    {
        graphics.hat.color = hat;
        graphics.clothes.color = clothes;
        graphics.skin.color = skin;
    }
}
