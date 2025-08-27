using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.Touch
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class ScrollMenuController : MonoBehaviour
    {
        public float Margins = 5.0f;
        public float DragMultiplier = 1.0f;

        [HideInInspector]
        public bool IsEnabled = false;
        float minX, maxX;

        [HideInInspector]
        public float xOffset = 0f;
        Vector3 lastTouch;

        [HideInInspector]
        public List<Transform> Items = new List<Transform>(); // Just for reference...
        public List<Vector3> ItemsOriginalPositions { get; private set; } = new List<Vector3>(); // Local position

        public bool Initialized { get; private set; } = false;
 
        // Update is called once per frame
        void Update()
        {
            // We're doing this in update because in Start it will spawn the children...
            if (!IsEnabled)
                return;

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Vector3 worldPos = DragManagerTouch.GetWorldSpacePos(touch.position);
                if (touch.phase == TouchPhase.Began)
                {
                    lastTouch = worldPos;
                }

                xOffset += (worldPos.x - lastTouch.x) * DragMultiplier;

                lastTouch = worldPos;
            }

            SetOffset(xOffset);
        }

        public void SetOffset(float offset)
    	{
            xOffset = Mathf.Clamp(offset, minX, maxX);
            transform.position = Vector3.right * xOffset;
        }

        public void Init()
        {
            float leftMost = 0f;
            float rightMost = 0f;
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.position.x < leftMost)
                    leftMost = child.position.x;
                else if (child.position.x > rightMost)
                    rightMost = child.position.x;
            }

            minX = -rightMost + Margins;
            maxX = -leftMost - Margins;

            foreach (var item in Items)
                ItemsOriginalPositions.Add(item.localPosition);

            Initialized = true;
        }

        public void ResetScroll()
        {
            xOffset = 0f;
            transform.position = Vector3.right * xOffset;
        }

        public void ResetItemsPosition()
    	{
            for (int i = 0; i < Items.Count; i++)
                Items[i].localPosition = ItemsOriginalPositions[i];
    	}

        private void OnEnable()
        {
            //ResetScroll();
        }
    }


}