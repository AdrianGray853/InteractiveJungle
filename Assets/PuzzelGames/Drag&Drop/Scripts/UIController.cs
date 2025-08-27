using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class UIController : MonoBehaviour
    {
        public string BackScene = "MainMenu";

        public void GoBack()
        {
            SoundManager.Instance.PlaySFX("ClickButton");
            /*
            string targetScene = "MainMenu";
            if (GameData.Instance.GameType == GameData.eGameType.UpperCase)
                targetScene = "UpperCaseMap";
            else if (GameData.Instance.GameType == GameData.eGameType.LowerCase)
                targetScene = "LowerCaseMap";
            else if (GameData.Instance.GameType == GameData.eGameType.MixedCase)
                targetScene = "CombinationMap";
            else if (GameData.Instance.GameType == GameData.eGameType.MiniGames)
                targetScene = "MiniGames";

            GameData.Instance.GameType = GameData.eGameType.None;
            TransitionManager.Instance.ShowFade(2.0f, () => SceneLoader.Instance.LoadScene(targetScene));
            */
            GameData.Instance.GameType = GameData.eGameType.None;
            TransitionManager.Instance.ShowFade(2.0f, () => SceneLoader.Instance.LoadScene(BackScene));
        }

        public void GoHome()
        {
            SoundManager.Instance.PlaySFX("ClickButton");
            TransitionManager.Instance.ShowFade(2.0f, () => SceneLoader.Instance.LoadScene("MainMenu"));
        }

        public void GoNext()
        {
            SoundManager.Instance.PlaySFX("ClickButton");
            Debug.LogWarning("Do we even need that?");
        }

        public void Restart()
        {
            SoundManager.Instance.PlaySFX("ClickButton");
            SceneLoader.Instance.ReloadScene();
        }
    }


}