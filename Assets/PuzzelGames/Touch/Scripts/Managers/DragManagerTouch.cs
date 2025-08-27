using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Interactive.Touch
{
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

    public class DragManagerTouch : MonoBehaviour
    {

    	private static bool IsDestroyed = false;
    	private static DragManagerTouch _instance = null;
    	//private static bool _initialized = false;
    	public static DragManagerTouch Instance
    	{
    		get
    		{
    			if (IsDestroyed)
    				return null;

    			if (_instance == null)// && !_initialized)
    			{
    				Instantiate(Resources.Load("Managers") as GameObject);
    				//GameObject go = new GameObject("DragManagerTouch");
    				//_instance = go.AddComponent<DragManagerTouch>();

    				//_initialized = true;
    			}
    			return _instance;
    		}
    		private set
    		{
    			_instance = value;
    			//_initialized = true;
    		}
    	}
	

    	//public static DragManagerTouch Instance { get; private set; }

    	public const float LeftMarginPercent = 0.05f;
    	public const float RightMarginPercent = 0.05f;
    	public const float TopMarginPercent = 0.1f;
    	public const float BottomMarginPercent = 0.15f;

    	public delegate void OnTouchDelegate(Touch touch);
    	public delegate void OnReleaseDelegate(Touch touch, DragHelperTouch helper);

    	Dictionary<int, DragHelperTouch> dragHelpers = new Dictionary<int, DragHelperTouch>();


    	OnTouchDelegate onTouchDelegates;
    	OnReleaseDelegate onReleaseDelegates;

    	// Start is called before the first frame update
    	void Awake()
    	{
    		if (_instance != null && _instance != this)
    		{
    			Destroy(gameObject);
    		}
    		else
    		{
    			DontDestroyOnLoad(this);
    			Instance = this;
    		}
    	}

    	// Grabbed from https://answers.unity.com/questions/195698/stopping-a-rigidbody-at-target.html for stability!
    	public float _toVel = 40.0f; //2.5f;
    	public float _maxVel = 100.0f; //15.0f;
    	public float _maxForce = 500.0f; //40.0f;
    	public float _gain = 10.0f; //5f;

    	// Update is called once per frame
    	void Update()
    	{
    		if (Input.touchCount > 0)
    		{
    			for (int i = 0; i < Input.touchCount; i++)
    			{
    				Touch touch = Input.GetTouch(i);

    				if (touch.phase == TouchPhase.Began)
    				{
    					if (onTouchDelegates != null)
    						onTouchDelegates(Input.GetTouch(i));
    				}
    				else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
    				{
    					DragHelperTouch dragHelper = dragHelpers.GetValueOrDefault(touch.fingerId);
    					if (onReleaseDelegates != null && dragHelper != null)
    					{
    						dragHelper.Update(GetWorldSpacePos(touch.position));
    						onReleaseDelegates(touch, dragHelper);
    					}
    					dragHelpers.Remove(touch.fingerId);
    				}
    				else if (touch.phase == TouchPhase.Moved)
    				{
    					DragHelperTouch helper = dragHelpers.GetValueOrDefault(touch.fingerId);
    					if (helper != null && !helper.IsRigidBody)
    					{
    						helper.Update(GetWorldSpacePos(touch.position));
    					}
    				} 
    			}
    		}
    	}

    	void FixedUpdate()
    	{ // For rigid bodies
    		if (Input.touchCount > 0)
    		{
    			for (int i = 0; i < Input.touchCount; i++)
    			{
    				Touch touch = Input.GetTouch(i);
    				if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
    				{
    					DragHelperTouch helper = dragHelpers.GetValueOrDefault(touch.fingerId);
    					if (helper != null && helper.IsRigidBody)
    					{
    						helper.Update(GetWorldSpacePos(touch.position));
    					}
    				}
    			}
    		}
    	}

    	public void AddOnTouchCallback(OnTouchDelegate callback) 
    	{
    		onTouchDelegates += callback;
    	}

    	public void RemoveOnTouchCallback(OnTouchDelegate callback)
    	{
    		onTouchDelegates -= callback;
    	}

    	public void AddOnReleaseCallback(OnReleaseDelegate callback)
    	{
    		onReleaseDelegates += callback;
    	}

    	public void RemoveOnReleaseCallback(OnReleaseDelegate callback)
    	{
    		onReleaseDelegates -= callback;
    	}

        public void Clear()
    	{
            dragHelpers.Clear();
            onTouchDelegates = null;
    		onReleaseDelegates = null;
    	}

        public bool HasNewInput()
    	{
            if (Input.touchCount > 0)
    		{
                for (int i = 0; i < Input.touchCount; i++)
    			{
                    if (Input.GetTouch(i).phase == TouchPhase.Began)
                        return true;
    			}
    		}

            return false;
    	}

        public List<Touch> GetNewInputs()
    	{
    		List<Touch> returnList = new List<Touch>();
    		if (Input.touchCount > 0)
    		{
    			for (int i = 0; i < Input.touchCount; i++)
    			{
    				if (Input.GetTouch(i).phase == TouchPhase.Began)
    					returnList.Add(Input.GetTouch(i));
    			}
    		}

    		return returnList;
    	}

        public void AddDrag(int fingerId, Transform objTransform, Vector3 attachPosition, Vector3 restrictOffset, bool isRigidBody = false)
    	{
    		DragHelperTouch newDrag = new DragHelperTouch(fingerId, objTransform, attachPosition, restrictOffset, isRigidBody);
    		dragHelpers.Add(fingerId, newDrag);
    	}

    	public void RemoveDrag(Transform objTransform)
    	{
    		foreach (var helper in dragHelpers.Where(x => x.Value.DraggingTransform == objTransform).ToList())
    			dragHelpers.Remove(helper.Key);
    	}

    	public bool HasDrag(Transform objTransform)
    	{
    		return dragHelpers.Any(x => x.Value.DraggingTransform == objTransform);
    	}

    	public bool IsDragging()
    	{
    		return dragHelpers.Count > 0;
    	}

    	//----------------------------------- Helpers --------------------------------------------

    	public static Collider2D[] GetHitObjectsFromTouchPos(Vector2 position, Camera camera = null)
    	{
    		RaycastHit2D[] hits = RaycastAllTouch(position, camera);
    		if (hits != null && hits.Length > 0)
    		{
    			return hits.Select(x => x.collider).ToArray();
    		}
    		return null;
    	}
    	// Metoda periculoasa, poate ignora obiecte inactive!
    	public static Collider2D GetHitObjectFromTouchPos(Vector2 position, Camera camera = null)
    	{
    		RaycastHit2D hit = RaycastTouch(position, camera);
    		if (hit)
    		{
    			return hit.collider;
    		}
    		return null;
    	}

    	public static RaycastHit2D RaycastTouch(Vector3 touchPosition, Camera camera = null)
    	{
    		if (camera == null)
    			camera = Camera.main;
    		Vector3 position = camera.ScreenToWorldPoint(touchPosition);
    		position.z = 0f;
    		return Physics2D.Raycast(position, Vector3.zero);
    	}

    	public static RaycastHit2D[] RaycastAllTouch(Vector3 touchPosition, Camera camera = null)
    	{
    		if (camera == null)
    			camera = Camera.main;
    		Vector3 position = camera.ScreenToWorldPoint(touchPosition);
    		position.z = 0f;
    		return Physics2D.RaycastAll(position, Vector3.zero);
    	}

    	public static Vector3 GetWorldSpacePos(Vector3 position, Camera camera = null)
    	{
    		if (camera == null)
    			camera = Camera.main;
    		position = camera.ScreenToWorldPoint(position);
    		position.z = 0f;
    		return position;
    	}

    	public static Vector3 ClampPositionToSafeScreen(Vector3 position, Camera camera = null)
    	{
    		if (camera == null)
    			camera = Camera.main;

    		Vector3 TopRight = new Vector3(Screen.width - RightMarginPercent * Screen.width, Screen.height - TopMarginPercent * Screen.height, 0f);
    		Vector3 BottomLeft = new Vector3(LeftMarginPercent * Screen.width, BottomMarginPercent * Screen.height, 0f);
    		TopRight = camera.ScreenToWorldPoint(TopRight).SetZ(0f);
    		BottomLeft = camera.ScreenToWorldPoint(BottomLeft).SetZ(0f);
    		position.x = Mathf.Clamp(position.x, BottomLeft.x, TopRight.x);
    		position.y = Mathf.Clamp(position.y, BottomLeft.y, TopRight.y);
    		position.z = Mathf.Clamp(position.z, BottomLeft.z, TopRight.z);
    		return position;
    	}

    	public static void VisualizeGizmoSafeScreen(Camera camera = null)
    	{
    		if (camera == null)
    			camera = Camera.main;

    		Vector3 TopRight = new Vector3(Screen.currentResolution.width - RightMarginPercent * Screen.currentResolution.width,
    			Screen.currentResolution.height - TopMarginPercent * Screen.currentResolution.height, 0f);
    		Vector3 BottomLeft = new Vector3(LeftMarginPercent * Screen.currentResolution.width, BottomMarginPercent * Screen.currentResolution.height, 0f);
    		TopRight = camera.ScreenToWorldPoint(TopRight).SetZ(0f);
    		BottomLeft = camera.ScreenToWorldPoint(BottomLeft).SetZ(0f);
    		Vector3 cubePosition = (TopRight + BottomLeft) * 0.5f;
    		Vector3 cubeSize = TopRight - BottomLeft;
    		Gizmos.color = Color.yellow;
    		Gizmos.DrawWireCube(cubePosition, cubeSize);
    	}

    	private void OnDrawGizmos()
    	{
    		for (int i = 0; i < dragHelpers.Count; i++)
    			dragHelpers[i].DrawGizmos();
    	}
    	private void OnDestroy()
    	{
    		IsDestroyed = true;
    	}
    }


}