using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.Touch
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class OrthographicScreenAnchor : MonoBehaviour
    {
    	[System.Flags]
    	public enum eAnchor {
    		Top = 1 << 0,
    		Right = 1 << 1,
    		Bottom = 1 << 2,
    		Left = 1 << 3
    	}

    	[Header("Anchor Settings")]
    	public eAnchor Anchor;
    	public float AnchorInfluence = 1.0f;
    	public bool ReScaleX = false;
    	public bool ReScaleY = false;
    	[Tooltip("This will ignore the RescaleX and Y settings, and will maintain the original scale to fit the screen")]
    	public bool RelativeScale = false;
    	public float ScaleInflulence = 1.0f;
    	[Tooltip("Keep updating the anchor during gameplay")]
    	public bool KeepUpdating = false;

    	[Header("Reference Settings")]
    	public Vector2 ReferenceScreenSize = new Vector2(2778.0f, 1284.0f); // IPhone 13 Pro Max
    	public float ReferenceOrthoSize = 5.0f;
    	public Vector3 OriginalCameraPosition = new Vector3(0f, 0f, -10.0f);


    	bool initialized = false;

    	Vector3 originalPosition;
    	Vector3 originalScale;

    	private void Awake()
    	{
    		if (!initialized)
    		{ // We need to check this as OnValidate can be called earlier than Awake!
    			originalPosition = transform.position;
    			originalScale = transform.localScale;
    		}
    		Debug.Log(Camera.main.aspect);
    		UpdateAnchor();
    		initialized = true;
    	}

    	private void Update()
    	{
    		if (KeepUpdating)
    			UpdateAnchor();
    	}

    	void UpdateAnchor()
    	{
    		float	leftPosition = originalPosition.x,
    				rightPosition = originalPosition.x,
    				topPosition = originalPosition.y,
    				bottomPosition = originalPosition.y;

    		float originalAspect = (ReferenceScreenSize.x / ReferenceScreenSize.y);
    		float newAspect = Camera.main.aspect;

    		float originalVerticalHalfSize = ReferenceOrthoSize;
    		float originalHorizontalHalfSize = originalVerticalHalfSize * originalAspect;

    		float verticalHalfSize = Camera.main.orthographicSize;
    		float horizontalHalfSize = verticalHalfSize * newAspect;
    		Vector3 cameraPos = Camera.main.transform.position;

    		Vector3 position = transform.position;
    		Vector3 scale = originalScale;

    		if (Anchor.HasFlag(eAnchor.Left))
    		{
    			leftPosition = (cameraPos.x - horizontalHalfSize) + originalPosition.x - (OriginalCameraPosition.x - originalHorizontalHalfSize);
    		}
    		if (Anchor.HasFlag(eAnchor.Right))
    		{
    			rightPosition = (cameraPos.x + horizontalHalfSize) - (OriginalCameraPosition.x + originalHorizontalHalfSize - originalPosition.x);
    		}
    		if (Anchor.HasFlag(eAnchor.Bottom))
    		{
    			bottomPosition = (cameraPos.y - verticalHalfSize) + originalPosition.y - (OriginalCameraPosition.y - originalVerticalHalfSize);
    		}
    		if (Anchor.HasFlag(eAnchor.Top))
    		{
    			topPosition = (cameraPos.y + verticalHalfSize) - (OriginalCameraPosition.y + originalVerticalHalfSize - originalPosition.y);
    		}

    		if (RelativeScale)
    		{
    			float hPercentage = horizontalHalfSize / originalHorizontalHalfSize;
    			float vPercentage = verticalHalfSize / originalVerticalHalfSize;
    			if (hPercentage > vPercentage)
    				scale = originalScale * hPercentage;
    			else
    				scale = originalScale * vPercentage;
    		}
    		else
    		{
    			if (ReScaleX)
    				scale.x = originalScale.x * horizontalHalfSize / originalHorizontalHalfSize;
    			if (ReScaleY)
    				scale.y = originalScale.y * verticalHalfSize / originalVerticalHalfSize;
    		}

    		if (Anchor.HasFlag(eAnchor.Left) && Anchor.HasFlag(eAnchor.Right))
    			position.x = (leftPosition + rightPosition) * 0.5f;
    		else if (Anchor.HasFlag(eAnchor.Left))
    			position.x = leftPosition;
    		else if (Anchor.HasFlag(eAnchor.Right))
    			position.x = rightPosition;
    		else
    			position.x = originalPosition.x;

    		if (Anchor.HasFlag(eAnchor.Top) && Anchor.HasFlag(eAnchor.Bottom))
    			position.y = (topPosition + bottomPosition) * 0.5f;
    		else if (Anchor.HasFlag(eAnchor.Top))
    			position.y = topPosition;
    		else if (Anchor.HasFlag(eAnchor.Bottom))
    			position.y = bottomPosition;
    		else
    			position.y = originalPosition.y;


    		transform.position = Vector3.LerpUnclamped(originalPosition, position, AnchorInfluence);
    		transform.localScale = Vector3.LerpUnclamped(originalScale, scale, ScaleInflulence);
    	}

    #if UNITY_EDITOR
    	private void OnValidate()
    	{
    		if (Application.isPlaying)
    		{
                if (initialized)
    				UpdateAnchor();
                else
                    Awake();
    		}
    	}
    #endif
    }


}