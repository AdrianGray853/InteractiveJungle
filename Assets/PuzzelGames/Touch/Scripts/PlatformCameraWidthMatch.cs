using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.Touch
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    [RequireComponent(typeof(Camera))]
    public class PlatformCameraWidthMatch : MonoBehaviour
    {
        [Range(0f, 1.0f)]
        public float MatchWidth = 1.0f;
        public float YOffsetPercentage = 0f;

        public const float OriginalOrtographicSize = 5.0f;
        public float NewOrtographicSize { get; private set; } = OriginalOrtographicSize;

        public float GetOrtographicSize
    	{
            get
    		{
                if (cam != null)
                    return cam.orthographicSize;
                else
                    return OriginalOrtographicSize;
    		}
    	}

        private Camera cam;
        Vector3 originalPosition;

        private readonly Vector2 ReferenceScreenSize = new Vector2(2778.0f, 1284.0f); // IPhone 13 Pro Max

    	private void Awake()
    	{
            cam = GetComponent<Camera>();
            originalPosition = cam.transform.position;
            UpdateSize();
    	}

    #if UNITY_EDITOR
        private void OnValidate()
    	{
            if (Application.isPlaying)
            {
                if (cam == null)
                    Awake();
                else
                    UpdateSize();
            }
    	}
    #endif

        void UpdateSize()
    	{
            float originalRatio = ReferenceScreenSize.x / ReferenceScreenSize.y;
            float newRatio = Screen.currentResolution.width / (float)Screen.currentResolution.height;
            NewOrtographicSize = OriginalOrtographicSize * Mathf.Lerp(1.0f, originalRatio / newRatio, MatchWidth);
            cam.orthographicSize = NewOrtographicSize;
            cam.transform.position = originalPosition + Vector3.up * (YOffsetPercentage * originalRatio / newRatio);
        }

        /*
    #if UNITY_EDITOR
    	private void Update()
    	{
            UpdateSize();
    	}
    #endif
        */
    }


}