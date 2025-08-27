using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

    public class LetterPanelController : MonoBehaviour
    {
        public RectTransform[] Highlights;

        List<char> SelectedChars = new List<char>();
        const float DragDistanceThreshold = 10.0f;
        char startingChar = '*';
        Vector2 lastTouchPos;

        private void OnEnable()
        {
            foreach (var h in Highlights)
            {
                h.gameObject.SetActive(false);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    lastTouchPos = touch.position;
                    Vector3 worldPos = DragManager.GetWorldSpacePos(touch.position);
                    foreach (var h in Highlights)
                    {
                        if (RectTransformUtility.RectangleContainsScreenPoint(h, touch.position))
                        {
                            if (h.gameObject.activeSelf)
                            {
                                SelectedChars.Remove(h.gameObject.name[0]);
                            }
                            else
                            {
                                SelectedChars.Add(h.gameObject.name[0]);
                            }

                            h.gameObject.SetActive(!h.gameObject.activeSelf);
                            startingChar = h.gameObject.name[0];
                            break;
                        }
                    }
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    if (startingChar != '*' && touch.position.Distance(lastTouchPos) > DragDistanceThreshold)
                    {
                        foreach (var h in Highlights)
                        {
                            if (RectTransformUtility.RectangleContainsScreenPoint(h, touch.position))
                            {
                                SelectedChars.Clear();
                                char currentChar = h.gameObject.name[0];
                                if (startingChar < currentChar)
                                {
                                    for (char c = startingChar; c <= currentChar; c++)
                                        SelectedChars.Add(c);
                                }
                                else
                                {
                                    for (char c = currentChar; c <= startingChar; c++)
                                        SelectedChars.Add(c);
                                }
                                foreach (var subHighlight in Highlights)
                                {
                                    subHighlight.gameObject.SetActive(SelectedChars.Contains(subHighlight.gameObject.name[0]));
                                }
                                break;
                            }
                        }
                    }
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    startingChar = '*';
                }
            }
        }

        public void PressContinue()
        {
            char maxLetter = (char)('A' - 1);
            foreach (char c in SelectedChars)
            {
                if (c > maxLetter)
                    maxLetter = c;
            }
            if (maxLetter >= 'A')
            {
                int targetLevelIdx = maxLetter - 'A' + 1;
                ProgressManager.Instance.UnlockLevel(GameData.eGameType.LowerCase, targetLevelIdx);
                ProgressManager.Instance.UnlockLevel(GameData.eGameType.UpperCase, targetLevelIdx);
            }
            OnBoardingController.Instance.ShowAgeSelection();
        }
    }


}