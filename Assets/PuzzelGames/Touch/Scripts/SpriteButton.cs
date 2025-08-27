using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Interactive.Touch
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

    public class SpriteButton : MonoBehaviour
    {
        [System.Serializable]
        public class ButtonClickedEvent : UnityEvent { }

        [SerializeField]
        private ButtonClickedEvent m_OnClick = new ButtonClickedEvent();

        public ButtonClickedEvent onClick
        {
            get { return m_OnClick; }
            set { m_OnClick = value; }
        }

        Collider2D colliderRef;
        Animator animator;
        int fingerId = -1;
        Vector3 touchPos;

        const float touchMaxDistance = 0.1f; // World Space

        private void Awake()
        {
            colliderRef = GetComponent<Collider2D>();
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (colliderRef == null)
                return;
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    Vector3 worldPos = DragManagerTouch.GetWorldSpacePos(touch.position);

                    if (colliderRef.OverlapPoint(worldPos))
                    {
                        if (touch.phase == TouchPhase.Began)
                        {
                            if (fingerId == -1)
                            {
                                fingerId = touch.fingerId;
                                touchPos = worldPos;
                            }
                        }
                        else if (touch.phase == TouchPhase.Ended && touch.fingerId == fingerId)
                        {
                            if (worldPos.Distance(touchPos) < touchMaxDistance)
                            {
                                m_OnClick.Invoke();
                                if (animator != null)
                                    animator.Play("Play", -1, 0f);
                            }
                            fingerId = -1;
                        }
                    }
                    else if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && touch.fingerId == fingerId)
                    {
                        fingerId = -1;
                    }
                }
            }
        }
    }


}