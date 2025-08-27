using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.PuzzelShape
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class AdPanelControllerShape : MonoBehaviour
    {
        public Transform[] Slides;
        public Transform[] Dots;
        public Transform Selection;
        public float MaxSlideTimer = 1.0f;
        public float SlideChangeSpeed = 500.0f;
        public float DragMultiplier = 1.0f;
        public float DragThreshold = 100.0f;
        public float SlideSpread = 2000.0f;

        int currentSlide = 0;
        bool isDragging = false;
        float xOffset = 0f;

        float slidePresentTimeout;
        Vector2 lastTouchPos;
        Vector2 initialTouchPos;

        // Start is called before the first frame update
        void Start()
        {
            UpdateSlidePosition();
            slidePresentTimeout = MaxSlideTimer;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    isDragging = true;
                    lastTouchPos = touch.position;
                    initialTouchPos = touch.position;
                }
                else if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended) 
                {
                    isDragging = false;
                    Vector2 diff = touch.position - lastTouchPos;
                    xOffset += diff.x * DragMultiplier;
                    UpdateSlidePosition();

                    float initialDiff = touch.position.x - initialTouchPos.x;
                    if (Mathf.Abs(initialDiff) > DragThreshold)
                    {
                        if (initialDiff < 0)
                            currentSlide = (int)(-xOffset / SlideSpread) + 1;
                        else
                            currentSlide = (int)(-xOffset / SlideSpread);
                        currentSlide = Mathf.Clamp(currentSlide, 0, Slides.Length - 1);
                    }
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    Vector2 diff = touch.position - lastTouchPos;
                    xOffset += diff.x * DragMultiplier;
                    UpdateSlidePosition();
                }
                lastTouchPos = touch.position;
            }

            // Snap to position
            if (!isDragging)
            {
                slidePresentTimeout -= Time.deltaTime;
                if (slidePresentTimeout < 0)
                {
                    currentSlide++;
                    if (currentSlide >= Slides.Length)
                        currentSlide = 0;
                    slidePresentTimeout = MaxSlideTimer;
                }

                float targetOffset = -currentSlide * SlideSpread;
                xOffset = Mathf.MoveTowards(xOffset, targetOffset, SlideChangeSpeed * Time.deltaTime);
                UpdateSlidePosition();
            }
            else
            {
                slidePresentTimeout = MaxSlideTimer;
            }
        }

        void UpdateSlidePosition()
        {
            xOffset = Mathf.Clamp(xOffset, -SlideSpread * (Slides.Length - 1), 0);
            for (int i = 0; i < Slides.Length; i++)
            {
                Vector3 pos = Slides[i].localPosition;
                pos.x = SlideSpread * i + xOffset;
                Slides[i].localPosition = pos;
            }

            Vector3 selectedDotPos = Selection.localPosition;
            selectedDotPos.x = -50.0f * xOffset / SlideSpread - 50.0f;
            Selection.localPosition = selectedDotPos;
        }
    }


}