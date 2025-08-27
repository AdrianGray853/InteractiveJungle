using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class MixMenuController : MonoBehaviour
    {
        public float MinDrag = -100.0f;
        public float MaxDrag = 100.0f;
        public float DragMultiplier = 20.0f;
        public float CenterAddOffset = 0f;

        public MixPlanetDescription[] Planets;
        public string[] TargetScenes;

        public FingerHintController TapHint;
        public FingerHintController SlideHint;
        public float MaxHintCooldown = 7.0f;
        public float HintCooldown = 3.0f;
        bool showSlide = true;
        bool didSlide = false;

        Vector3 initialPosition;
        float xOffset = 0f;
        Vector2 touchPosStart;
        float targetOffset = 0f;
        bool goToTarget = false;
        float goToTargetFailSafeTimer = 5.0f; // Safe Timer just in case we somehow don't reach the target, don't block the input!
        float scrollSpeed = 2.0f;

        // Start is called before the first frame update
        void Start()
        {
            SoundManager.Instance.CrossFadeMusic("ThirdPlanetBgMusic", 1.0f);
            string[] sfx = new string[] { "alright_what_here", "arrived_planet", "explore" };
            SoundManager.Instance.PlaySFX(sfx.GetRandomElement(), 1.0f, "voiceover", 2);

            initialPosition = transform.position;

            goToTarget = true;
            targetOffset = -Planets[1].transform.position.x + CenterAddOffset;
            xOffset = targetOffset;
        }

        // Update is called once per frame
        void Update()
        {
            float oldXOffset = xOffset;

            if (Input.touchCount > 0 && !TransitionManager.Instance.InTransition)
            {
                float relativeMovementX = Input.GetTouch(0).deltaPosition.x;
                relativeMovementX /= Screen.width;

                xOffset += relativeMovementX * DragMultiplier;

                //xOffset = Mathf.Clamp(xOffset, MinDrag, MaxDrag);

                //transform.position = initialPosition + Vector3.right * xOffset;
                didSlide = true;

                // Handle module selection
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    goToTarget = false;
                    scrollSpeed = 10.0f;
                    touchPosStart = Input.GetTouch(0).position;
                }
                else if (Input.GetTouch(0).phase == TouchPhase.Ended && (touchPosStart - Input.GetTouch(0).position).magnitude < 10.0f)
                {
                    Vector3 pos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
                    for (int i = 0; i < Planets.Length; i++)
                    {
                        if (Planets[i].Collider.OverlapPoint(pos))
                        {
                            SoundManager.Instance.PlaySFX("LevelDone");
                            GameData.Instance.GameType = GameData.eGameType.MixedCase;
                            string targetScene = Planets[i].TargetScene;
                            ProgressManager.Instance.SetMixLevelVisited(Planets[i].PlanetIndex);
                            TransitionManager.Instance.ShowFade(2.0f, () => SceneLoader.Instance.LoadScene(targetScene));
                        }
                    }

                }

                if (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled)
                {
                    goToTarget = true;
                    goToTargetFailSafeTimer = 5.0f;
                    float minX = float.MaxValue;
                    int planetIdx = 0;
                    for (int i = 0; i < Planets.Length; i++)
                    {
                        MixPlanetDescription planet = Planets[i];
                        if (Mathf.Abs(planet.transform.position.x) < minX)
                        {
                            targetOffset = xOffset - planet.transform.position.x + CenterAddOffset;
                            minX = Mathf.Abs(planet.transform.position.x);
                            planetIdx = i;
                        }
                    }
                    float xDiff = (touchPosStart - Input.GetTouch(0).position).x;
                    if (targetOffset < xOffset && xDiff < 0f)
                    { // We actually drag in a different direction!
                        planetIdx--;
                    }
                    else if (targetOffset > xOffset && xDiff > 0f)
                    {
                        planetIdx++;
                    }

                    // Readjust
                    planetIdx = Mathf.Clamp(planetIdx, 0, Planets.Length - 1);
                    targetOffset = xOffset - Planets[planetIdx].transform.position.x + CenterAddOffset;
                }
            }

            if (goToTarget)
            {
                goToTargetFailSafeTimer -= Time.deltaTime;
                if (goToTargetFailSafeTimer < 0f)
                    scrollSpeed = 10.0f;

                xOffset = Mathf.MoveTowards(xOffset, targetOffset, Mathf.Max(1.0f, Mathf.Abs(xOffset - targetOffset) * scrollSpeed) * Time.deltaTime);
                if (xOffset == targetOffset)
                    scrollSpeed = 10.0f;
            }
            else
            {
                goToTargetFailSafeTimer = 5.0f; // 5 sec to get to target
            }

            xOffset = Mathf.Clamp(xOffset, MinDrag, MaxDrag);
            transform.position = initialPosition + Vector3.right * xOffset;

            TapHint.transform.position += Vector3.right * (xOffset - oldXOffset);
            if (HintCooldown > 0f)
            {
                HintCooldown -= Time.deltaTime;
                if (HintCooldown < 0)
                {
                    if (showSlide && !didSlide)
                    {
                        SlideHint.MainAnimator.Play("ShowHorizontalToRight", -1, 0f);
                    }
                    else
                    {
                        List<Vector3> planetCenters = new List<Vector3>();
                        foreach (var planet in Planets)
                        {
                            Vector3 viewportPt = Camera.main.WorldToViewportPoint(planet.transform.position);
                            if (viewportPt.x > 0f && viewportPt.x < 1.0f)
                                planetCenters.Add(planet.transform.position);
                        }

                        TapHint.transform.position = planetCenters.GetRandomElement();
                        TapHint.MainAnimator.Play("ShowTap", -1, 0f);
                    }
                    showSlide = !showSlide;
                    HintCooldown = MaxHintCooldown;
                }
            }
        }
    }


}