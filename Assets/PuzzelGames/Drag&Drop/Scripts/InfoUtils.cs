using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public static class InfoUtils
    {
        static string[] AnimalSounds = new string[]
        {
            "alligator",
            "bear",
            "cow",
            "dog",
            "elephant",
            "fox",
            "giraffe",
            "horse",
            "iguana",
            "jellyfish",
            "koala",
            "lion",
            "monkey",
            "newt",
            "owl",
            "pig",
            "quail",
            "racoon",
            "seagull",
            "tiger",
            "unicorn",
            "vulture",
            "whale",
            "xrayfish",
            "yak",
            "zebra"
        };

        static string[] ObjectSounds = new string[]
        {
            "astronaut",
            "ball",
            "car",
            "drum",
            "envelope",
            "flower",
            "gift",
            "house",
            "island",
            "jeans",
            "kite",
            "ladybug",
            "mushroom",
            "notebook",
            "orange",
            "palette",
            "queen",
            "rainbow",
            "sun",
            "tooth",
            "umbrella",
            "vulcano",
            "watermelon",
            "xylophone",
            "yacht",
            "zeppelin"
        };

        public static int GetIndexFromLetter(char letter)
        {
            return char.ToLower(letter) - 'a';
        }

        public static string GetAnimalSoundNameFromLetter(char letter)
        {
            return AnimalSounds[GetIndexFromLetter(letter)];
        }
        public static string GetObjectSoundNameFromLetter(char letter)
        {
            return ObjectSounds[GetIndexFromLetter(letter)];
        }

    #if UNITY_EDITOR
        public static void PerformTest()
    	{
            for (int i = 0; i < AnimalSounds.Length; i++)
            {
                SoundManager.Instance.AddSFXToQueue(AnimalSounds[i]);
                SoundManager.Instance.AddSFXToQueue(AnimalSounds[i][0] + "_from_" + AnimalSounds[i]);
            }
    		for (int i = 0; i < ObjectSounds.Length; i++)
    		{
    			SoundManager.Instance.AddSFXToQueue(ObjectSounds[i]);
    			SoundManager.Instance.AddSFXToQueue(ObjectSounds[i][0] + "_from_" + ObjectSounds[i]);
    		}
    	}
    #endif
    }


}