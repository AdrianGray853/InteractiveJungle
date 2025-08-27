using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace Interactive.Touch
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

    public class MenuItemSetting : MonoBehaviour
    {
        public Collider2D ItemCollider;
        public SortingGroup SortGroup;
        public UnityEvent ClickEvent;

        /*
        const float minDistThreshold = 1.0f;
        Vector2 lastTouchPosition;
        bool touched = false;
        */

        // Start is called before the first frame update
        void Awake()
        {
            if (ItemCollider == null)
                ItemCollider = GetComponent<Collider2D>();
            if (SortGroup == null)
                SortGroup = GetComponent<SortingGroup>();
        }

        // Update is called once per frame
        void Update()
        {
            /*
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Vector3 worldPos = DragManagerTouch.GetWorldSpacePos(touch.position);
                if (touch.phase == TouchPhase.Began)
                {
                    if (ItemCollider.OverlapPoint(worldPos))
                    {
                        touched = true;
                        lastTouchPosition = touch.position;
                    }
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    if (touched && touch.position.Distance(lastTouchPosition) < minDistThreshold && ClickEvent != null)
                    {
                        ClickEvent.Invoke();
                    }

                    touched = false;
                }
            }
            */
        }
    }


}