using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Interactive.DRagDrop
{
    using DG.Tweening;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    public class MixWordGameplay : MonoBehaviour
    {
        [System.Serializable]
        public class letters
        {
            public TextLetterDragController[] Words;

        }
        public letters[] letter;
        public Sprite[] LevelsSprite;
        public GameObject PuzzelSolvedPanel;
        public Image puzzelSprite;
        public string key;
        public int sessionId;

        public RawImage Fader;
        public GameObject PopUp;

        TextLetterDragController currentWord;
        int currentWordIdx = 0;

        int maxLevels = 3;
        int currentLevel = 0;

        // Start is called before the first frame update
        void Start()
        {
            SoundManager.Instance.CrossFadeMusic("DragandDropBgMusic", 1.0f);

            //Utils.Shuffle(Words);
            currentWordIdx = 0;
            wordIndex = PlayerPrefs.GetInt(key, 0);
            Debug.Log($"wordIndex {wordIndex}, currentWordIdx {currentWordIdx}");
            GameProgressController.Instance.SetMaxProgressSteps(maxLevels);
            SpawnWord();

        }
        private int wordIndex;
        void SpawnWord()
        {

            TextLetterDragController wordRef = letter[wordIndex].Words[currentWordIdx];
            currentWord = Instantiate(wordRef.gameObject).GetComponent<TextLetterDragController>();
            currentWord.transform.position = Vector3.zero;
            if (SystemInfo.deviceModel.Contains("iPad"))
                currentWord.transform.localScale *= 0.8f;
            currentWord.ShowIntro = false;
            currentWord.OnDoneEvent += OnDone;
            if (currentWordIdx >= letter[currentWordIdx].Words.Length)
            {
                //Utils.Shuffle(Words);
                //currentWordIdx = 0;
            }
        }

        void OnDone()
        {
            TextLetterDragController tmpWord = currentWord;

            //SoundManager.Instance.PlaySFX(currentWord.GetWord());
            tmpWord.HideLetters().AppendCallback(() =>
            {

                Destroy(tmpWord.gameObject);
                currentLevel++;
                currentWordIdx++;
                GameProgressController.Instance.AddProgressSteps();



                if (currentLevel < maxLevels)
                {
                    SpawnWord();

                    //string[] sfx = new string[] { "awesome", "nextone", "good_job", "doing_great", "feel_rhythm", "bravo", "amazing", "definetly_know" };
                    //SoundManager.Instance.PlaySFX(sfx.GetRandomElement());
                    // SoundManager.Instance.AddSFXToQueue(word); nu ma duce capu , ajiutor pliz :(
                }
                else
                    ShowWinUI();
            });
        }

        public void ShowWinUI()
        {
#if UNITY_IOS
            if (!ProgressManager.Instance.IsReviewShown(3))
            {
                Debug.Log("Asking for review!");
                UnityEngine.iOS.Device.RequestStoreReview();
                ProgressManager.Instance.SetReviewShow(3);
            }
#endif

            if (LevelsSprite.Length > currentLevel)
                puzzelSprite.sprite = LevelsSprite[wordIndex];
            wordIndex++;
            SoundManager.Instance.PlaySFX("FinishMiniGame_3");
            string[] sfxx = new string[] { "good_job", "do_again", "did_great", "did_all", "play_again" };
            SoundManager.Instance.PlaySFX(sfxx.GetRandomElement());
            Transform panelChild = PuzzelSolvedPanel.transform.GetChild(1);
            panelChild.localScale = new Vector3(0.15f, 0.15f, 0.15f);

            PlayerPrefs.SetInt(key, wordIndex);
            Debug.Log($"AdvanceLevel: {PlayerPrefs.GetInt(key)}");

            PuzzelSolvedPanel.SetActive(true);
            // animate scale to 1 smoothly
            panelChild.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            PlayerPrefs.SetInt("Session", sessionId);

            Invoke(nameof(GoHome), 3);
            //DOTween.Sequence()
            //    .AppendInterval(1.0f)
            //    .AppendCallback(() => Fader.gameObject.SetActive(true))
            //    .Append(Fader.DOFade(0.8f, 1.0f))
            //    .AppendCallback(() => PopUp.SetActive(true))
            //    .Append(PopUp.transform.DOScale(Vector3.zero, 0.4f).From().SetEase(Ease.OutBack));
        }
        public void GoHome()
        {
            SceneManager.LoadScene(0);
        }
    }



}