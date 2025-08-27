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
using UnityEngine.UI;

    public class MixWordGameplay : MonoBehaviour
    {
        public TextLetterDragController[] Words;
        public RawImage Fader;
        public GameObject PopUp;

        TextLetterDragController currentWord;
        int currentWordIdx = 0;

        int maxLevels = 10;
        int currentLevel = 0;

        // Start is called before the first frame update
        void Start()
        {
            SoundManager.Instance.CrossFadeMusic("DragandDropBgMusic", 1.0f);

            Utils.Shuffle(Words);
            currentWordIdx = 0;
            SpawnWord();

            GameProgressController.Instance.SetMaxProgressSteps(maxLevels);
        }

        void SpawnWord()
        {
            TextLetterDragController wordRef = Words[currentWordIdx++];
            currentWord = Instantiate(wordRef.gameObject).GetComponent<TextLetterDragController>();
            currentWord.transform.position = Vector3.zero;
            if (SystemInfo.deviceModel.Contains("iPad"))
                currentWord.transform.localScale *= 0.8f;
            currentWord.ShowIntro = false;
            currentWord.OnDoneEvent += OnDone;
            if (currentWordIdx >= Words.Length)
            {
                Utils.Shuffle(Words);
                currentWordIdx = 0;
            }
        }

        void OnDone()
        {
            TextLetterDragController tmpWord = currentWord;

            SoundManager.Instance.PlaySFX(currentWord.GetWord());
            tmpWord.HideLetters().AppendCallback(() =>
            {
                Destroy(tmpWord.gameObject);
                currentLevel++;
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

            SoundManager.Instance.PlaySFX("FinishMiniGame_3");
            string[] sfxx = new string[] { "good_job", "do_again", "did_great", "did_all", "play_again" };
            SoundManager.Instance.PlaySFX(sfxx.GetRandomElement());
            DOTween.Sequence()
                .AppendInterval(1.0f)
                .AppendCallback(() => Fader.gameObject.SetActive(true))
                .Append(Fader.DOFade(0.8f, 1.0f))
                .AppendCallback(() => PopUp.SetActive(true))
                .Append(PopUp.transform.DOScale(Vector3.zero, 0.4f).From().SetEase(Ease.OutBack));
        }
    }


}