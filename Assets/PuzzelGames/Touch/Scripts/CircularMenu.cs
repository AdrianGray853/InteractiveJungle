using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.Touch
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class CircularMenu : MonoBehaviour
    {
        public MenuItemSetting[] Items;
        public float Spread = 5.0f;
        public float ZScale = 1.5f;
        public float ScaleOffset = 10.0f;
        public float RotateMultiplier = 2.0f;
        public float GoToSpeed = 360.0f;

        Vector3 lastTouch;
        Vector3 initialTouch;
        float touchDistance;
        float angleOffset = 0f;
        int frontItem = 0;

        float targetAngle = 0f;
        bool gotoTarget = false;

        const float TOUCH_DISTANCE_THRESHOLD = 0.75f; // In World Units

        // Update is called once per frame
        void Update()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Vector3 worldPos = DragManagerTouch.GetWorldSpacePos(touch.position);
                if (touch.phase == TouchPhase.Began)
                {
                    initialTouch = worldPos;
                    lastTouch = worldPos;
                    gotoTarget = false;
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    bool snap = true; // Should we calculate the target angle based on nearest items?
                    if (touchDistance < TOUCH_DISTANCE_THRESHOLD)
                    {
                        int touchedItem = -1;
                        float maxOrder = -10000;
                        for (int i = 0; i < Items.Length; i++)
                        {
                            if (Items[i].ItemCollider.OverlapPoint(worldPos) && Items[i].SortGroup.sortingOrder > maxOrder)
                            {
                                touchedItem = i;
                                maxOrder = Items[i].SortGroup.sortingOrder;
                            }
                        }

                        if (touchedItem >= 0)
                        {
                            snap = false;
                            if (touchedItem == frontItem)
                            {
                                if (Items[touchedItem].ClickEvent != null)
                                {
                                    angleOffset = targetAngle;
                                    Items[touchedItem].ClickEvent.Invoke();
                                }
                            }
                            else
                            {
                                targetAngle = -GetAngleFromItemId(touchedItem);
                                frontItem = touchedItem;
                            }
                        }
                    }

                    if (snap)
                    {
                        /*
                        float nearestItemAngle = 0f;
                        float nearestDiff = 100000.0f;
                        for (int i = 0; i < Items.Length; i++)
                        {
                            float angle = GetAngleFromItemId(i);
                            float diff = Mathf.Abs(Mathf.DeltaAngle(angle, angleOffset));
                            Debug.Log(angle + " " + angleOffset + " " + Mathf.DeltaAngle(angle, angleOffset));
                            if (diff < nearestDiff)
                            {
                                nearestItemAngle = angle;
                                nearestDiff = diff;
                            }
                        }

                        targetAngle = nearestItemAngle;
                        Debug.Log(nearestDiff);
                        */

                        int itemId = GetNearestItemFromAngle(angleOffset);
                        if (worldPos.x - initialTouch.x > 0f)
                            itemId = (itemId + 1) % Items.Length;
                        targetAngle = GetAngleFromItemId(itemId);
                        frontItem = itemId;
                    }

                    gotoTarget = true;
                    touchDistance = 0f;
                }

                angleOffset += (worldPos.x - lastTouch.x) * RotateMultiplier;
                touchDistance += lastTouch.Distance(worldPos);
                lastTouch = worldPos;
            }

            if (gotoTarget)
            {
                angleOffset = Mathf.LerpAngle(angleOffset, targetAngle, Time.deltaTime * GoToSpeed);
            }

            PositionItemsAtAngle(angleOffset);
        }

        int GetNearestItemFromAngle(float angle)
        {
            float angleIncrement = 360.0f / Items.Length;
            int id = Mathf.FloorToInt(angle / angleIncrement);
            id %= Items.Length;
            if (id < 0)
                id += Items.Length;
            return id;
        }

        float GetAngleFromItemId(int id)
        {
            float angleIncrement = 360.0f / Items.Length;
            return angleIncrement * id;
        }

        void PositionItemsAtAngle(float angle)
        {
            float angleIncrement = 360.0f / Items.Length;
            for (int i = 0; i < Items.Length; i++)
            {
                float targetAngle = (i * angleIncrement + angle) * Mathf.Deg2Rad;
                Vector3 position = new Vector3(
                    Mathf.Sin(targetAngle) * Spread,
                    0f,
                    Mathf.Cos(targetAngle) * Spread
                    );
                Items[i].transform.position = position + Vector3.up * Mathf.Sin(Time.time * 1.5f + i * 2.0f) * 0.5f;

                Items[i].transform.localScale = Vector3.one * ((Items[i].transform.position.z + ScaleOffset) * ZScale);

                Items[i].SortGroup.sortingOrder = (int)(Items[i].transform.position.z * 100.0f) + 100;
            }
        }
    }


}