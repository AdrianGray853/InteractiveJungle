using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.PuzzelShape
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class ColorWheel : MonoBehaviour
    {
        public Color[] Colors;
        public GameObject ColorItemRef;
        public Collider2D Wheel;
    	public GameObject Highlight;
        public float Distance = 3.0f;
        public float Separation = 15.0f;
    	public float RotationMultiplier = 0.5f;
    	public float TapThreshold = 30.0f;
    	public float TapTime = 0.1f;

    	public float XPosPercentage = 1.2f;
    	public Vector2 TargetResolution = new Vector2(2778.0f, 1284.0f); // IPhone 13 Pro Max

    	[HideInInspector]
    	public Color SelectedColor { get; private set; } = Color.red;
    	[HideInInspector]
    	public bool IsActive { get; private set; } = false;

        List<SpriteRenderer> colorItems = new List<SpriteRenderer>();
        float offset = 0f;

    	// wheelInfo
    	int fingerId = -1;
    	Vector2 initialTouchPosition;
    	float initialTouchTime;

        // Start is called before the first frame update
        void Start()
        {
    		// Resolution stuff
    		Vector3 pos = transform.position;
    		//pos.x = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height * 0.5f)).x * XPosPercentage - Wheel.transform.localPosition.x;
    		float targetRatio = TargetResolution.x / TargetResolution.y;
    		float currentRatio = (float)Screen.width / Screen.height;
    		pos.x += (1.0f - currentRatio / targetRatio) * XPosPercentage;
    		transform.position = pos;

    		int nrCircles = Mathf.CeilToInt(180.0f / Separation);
            for (int i = 0; i < nrCircles; i++)
    		{
                SpriteRenderer sr = Instantiate(ColorItemRef, transform).GetComponent<SpriteRenderer>();
                sr.gameObject.SetActive(true);
    			colorItems.Add(sr);
    		}
    		UpdateColorPositions();
    	}

        // Update is called once per frame
        void Update()
        {
    		bool found = false; // Failsafe
    		for (int i = 0; i < Input.touchCount; i++)
    		{
    			Touch touch = Input.GetTouch(i);
    			if (fingerId < 0)
    			{
    				if (touch.phase == TouchPhase.Began)
    				{
    					Vector3 worldPos = DragManagerShape.GetWorldSpacePos(touch.position);
    					if (Wheel.OverlapPoint(worldPos)) {
    						fingerId = touch.fingerId;
    						initialTouchPosition = touch.position;
    						initialTouchTime = Time.time;
    						found = true;
    						break;
    					}
    				}
    			}
    			else if (touch.fingerId == fingerId)
    			{
    				found = true;
    				if (touch.phase == TouchPhase.Moved)
    				{
    					offset -= touch.deltaPosition.y * RotationMultiplier;
    					UpdateColorPositions();
    					if (!IsTapping(touch.position))
                        {
    						SetActiveState(true);
    					}
    				} 
    				else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
    				{
    					if (IsTapping(touch.position))
                        { // It's a tap
    						bool activeState = !IsActive;
    						int selectionIndex = Mathf.FloorToInt(90.0f / Separation);
    						Vector3 worldPos = DragManagerShape.GetWorldSpacePos(touch.position);
    						for (int j = 0; j < colorItems.Count; j++)
                            {
    							float angle = Vector3.SignedAngle(colorItems[j].transform.position - Wheel.transform.position, Vector3.left, Vector3.forward);
    							if (Mathf.Abs(angle) < Separation * 0.5f)
    								continue;
    							if (colorItems[j].GetComponent<Collider2D>().OverlapPoint(worldPos))
                                {
    								offset += angle;
    								UpdateColorPositions();
    								activeState = true; // We pressed on a color!
    								break;
                                }
                            }
    						SetActiveState(activeState);
    					}
    					else 
    					{
    						SetActiveState(true);
    						offset -= touch.deltaPosition.y * RotationMultiplier;
    						UpdateColorPositions();
    					}
    					fingerId = -1;
    				}
    			}
    		}

    		if (!found)
    			fingerId = -1; // Should never happen in a real case!

    		if (fingerId == -1)
    		{ // Move towards closest color
    			for (int i = 0; i < colorItems.Count; i++)
    			{
    				if (!colorItems[i].gameObject.activeSelf)
    					continue;

    				float angle = Vector3.Angle(colorItems[i].transform.position - Wheel.transform.position, Vector3.left);
    				if (angle < Separation * .5f)
    				{
    					float targetAngle = Mathf.Round(offset / Separation) * Separation;
    					offset = Mathf.MoveTowards(offset, targetAngle, 90.0f * Time.deltaTime);
    					UpdateColorPositions();
    					break;
    				}
    			}
    		}
    	}

        void UpdateColorPositions()
    	{
    		float residual = 90.0f % Separation;

    		float angle = offset % Separation;
    		if (angle < 0f)
    			angle += Separation;
    		angle += 90.0f;

    		int colorIdx = Mathf.CeilToInt(-offset / Separation) % Colors.Length;
    		if (colorIdx < 0)
    			colorIdx += Colors.Length;
    		for (int i = 0; i < colorItems.Count; i++)
    		{
    			colorItems[i].transform.position = Wheel.transform.position + UtilsShape.AngleToVector3(angle + residual) * Distance;
    			//colorItems[i].color = Colors[colorIdx % Colors.Length];
    			colorItems[i].GetComponent<OutlineControllerShape>().SetTintColor(Colors[colorIdx % Colors.Length]);
    			angle += Separation;
    			colorIdx++;
    		}

    		int selectedIdx = Mathf.RoundToInt(-offset / Separation) % Colors.Length;
    		if (selectedIdx < 0)
    			selectedIdx += Colors.Length;
    		selectedIdx += Mathf.RoundToInt(90.0f / Separation);
    		Color lastColor = SelectedColor;
    		SelectedColor = Colors[selectedIdx % Colors.Length];
    		if (lastColor != SelectedColor)
    			SoundManagerShape.Instance.PlaySFX("PuzzlePlace");
    	}

    	void SetActiveState(bool state)
        {
    		if (IsActive != state)
    		{
    			SoundManagerShape.Instance.PlaySFX(state ? "TurnOn" : "TurnOff");
    		}
    		IsActive = state;
    		Highlight.SetActive(IsActive);
    	}

    	bool IsTapping(Vector2 position)
        {
    		return (position.Distance(initialTouchPosition) < TapThreshold && (Time.time - initialTouchTime) < TapTime);
    	}

    	public void NotifyLevelChange()
    	{
    		SetActiveState(false);
    	}
    	/*
    #if UNITY_EDITOR
    	private void OnValidate()
    	{
            UpdateColorPositions();
        }
    #endif
    	*/
    }



}