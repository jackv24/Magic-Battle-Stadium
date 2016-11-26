using UnityEngine;
using System.Collections;

namespace CreativeSpore.SuperTilemapEditor
{
    public static class ShortcutKeys
    {
        // TilemapEditor.cs
        public const KeyCode k_FlipH = KeyCode.X;
        public const KeyCode k_FlipV = KeyCode.Y;
        public const KeyCode k_Rot90 = KeyCode.Period;
        public const KeyCode k_Rot90Back = KeyCode.Comma;

        // TilemapGroupEditor.cs
        public const KeyCode k_PrevLayer = KeyCode.KeypadMinus;
        public const KeyCode k_NextLayer = KeyCode.KeypadPlus;
    }
}