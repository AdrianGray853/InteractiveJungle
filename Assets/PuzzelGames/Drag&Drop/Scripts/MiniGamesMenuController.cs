using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class MiniGamesMenuController : MonoBehaviour
    {
        public Collider2D[] ModuleColliders;
        public string[] TargetScenes;

        public FingerHintController[] FingerHints;
        public float HintMaxCountdown = 5.0f;
        float hintCountdown = 5.0f;

        Vector2 touchPosStart;

        // Start is called before the first frame update
        void Start()
        {
            Sequence s = DOTween.Sequence();
            s.AppendInterval(3.0f);
            s.AppendCallback(() => SoundManager.Instance.PlaySFX("alright_what_here"));
            s.AppendInterval(4.5f);
            s.AppendCallback(() => SoundManager.Instance.PlaySFX("arrived_planet"));
            SoundManager.Instance.CrossFadeMusic("FourthPlanetBgMusic", 1.0f);

            hintCountdown = HintMaxCountdown;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.touchCount > 0 && !TransitionManager.Instance.InTransition)
            {
                // Handle module selection
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    touchPosStart = Input.GetTouch(0).position;
                }
                else if (Input.GetTouch(0).phase == TouchPhase.Ended && (touchPosStart - Input.GetTouch(0).position).magnitude < 10.0f)
                {
                    Vector3 pos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
                    for (int i = 0; i < ModuleColliders.Length; i++)
                    {
                        if (ModuleColliders[i].OverlapPoint(pos))
                        {
                            GameData.Instance.GameType = GameData.eGameType.MiniGames;
                            string targetScene = TargetScenes[i];
                            TransitionManager.Instance.ShowFade(2.0f, () => SceneLoader.Instance.LoadScene(targetScene));

                            hintCountdown = -1.0f;
                            foreach (var hint in FingerHints)
                            {
                                //hint.StopPlayback(); // Doesn't work
                                hint.MainAnimator.enabled = false;
                                hint.FingerSprite.DOFade(0f, 0.3f);
                            }
                            break;
                        }
                    }
                }
            }

            if (hintCountdown > 0)
            {
                hintCountdown -= Time.deltaTime;
                if (hintCountdown < 0)
                {
                    FingerHintController hint = FingerHints.GetRandomElement();
                    hint.MainAnimator.Play("ShowTap", -1, 0f);
                    hintCountdown = HintMaxCountdown;
                    if(Random.value < 0.3f)
                        SoundManager.Instance.PlaySFX("explore");
                }
            }
        }
    }


}