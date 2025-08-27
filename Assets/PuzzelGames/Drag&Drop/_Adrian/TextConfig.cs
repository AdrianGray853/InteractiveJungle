using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LettersConfig", menuName = "ScriptableObjects/CreateLettersConfig", order = 1)]
public class TextConfig : ScriptableObject
{
    [System.Serializable]
    public class LetterDescription
    {
        public char character;
        public Sprite sprite;
        public float width;
    }

    public LetterDescription[] Letters;
}