using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class UFOEvents : MonoBehaviour
    {
        public UFOSortController UFOController;
        public UFOSelectController UFOGameplayController;

        // For UFO Sort Controller
        public void SpawnBigLetters()
        {
            UFOController.SpawnLetters(true);
        }

        public void SpawnSmallLetters()
        {
            UFOController.SpawnLetters(false);
        }

        public void RemoveLetters()
        {
            UFOController.RemoveLetters();
        }

        // For UFO Select Controller
        public void SpawnLetters()
        {
            UFOGameplayController.SpawnLetters();
            UFOGameplayController.SelectPair(); 
        }
    }


}