using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Interactive.Touch
{
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

    public class StampAndPatternController : MonoBehaviour
    {
    	public string StartSound;
    	public Material BlitPatternMaterial;
    	public Material BlitStampMaterial;
    	public float Scale = 0.075f;
    	public float StampScale = 0.03f;
    	[Range(0f, 1f)]
    	public float Hardness = 0.9f;
    	Animator animator;

    	RenderTexture rt;
    	Texture2D originalTex;

    	Vector3 previousTouch; // WorldSpace;
    	SpriteGroups spriteGroups;
    	int fingerId = -1;
    	SpriteGroups.SpriteGroup touchedGroup;
    	int touchedGroupIdx = -1;
    	bool inputActive = false;
    	bool goNextElement = false;
    	const float TimeToGetToDestination = 1.0f;
    	float BaseOrthoSize = 5.0f;

    	List<List<Vector3>> checkPoints; // Used to track progress
    	List<int> totalCheckPoints;
    	const float POINT_DISTANCE = 0.1f;

    	float nonTouchTimer = -1.0f; // < 0 didn't touch this group, > 0 touched and still on this group

    	private void Awake()
    	{
    		BlitStampMaterial = Instantiate(BlitStampMaterial);
    		BlitPatternMaterial = Instantiate(BlitPatternMaterial);
    		animator = GetComponentInChildren<Animator>();
    		animator.keepAnimatorStateOnDisable = true;
    		animator.enabled = false; // Stupid hack for Unity because if an animation has animated property, any empty animation will lock that value too!

    		spriteGroups = GetComponent<SpriteGroups>();
    		GameDataTouch.Instance.GameType = GameDataTouch.eGameType.PatternAndStamps;
    	}

    	// Start is called before the first frame update
    	void Start()
    	{
    		// Reset Camera
    		BaseOrthoSize = Camera.main.GetComponent<PlatformCameraWidthMatch>().NewOrtographicSize;
    		Camera.main.transform.position = new Vector3(0f, 0f, -10.0f);
    		Camera.main.orthographicSize = BaseOrthoSize;

    		float startTime = Time.realtimeSinceStartup;
    		BuildCheckPoints();
    		Debug.Log("Built Points in: " + (Time.realtimeSinceStartup - startTime) + " seconds");

    		// CreateIcon();

    		// THIS IS  BAD, can select THE BAAACKGROUND
    		//SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();
    		SpriteRenderer renderer = null; // Should not remain null, otherwise we fucked up in the setup phase!
    		foreach (var group in spriteGroups.RendererGroups)
            {
    			foreach (var rend in group.Group)
                {
    				if (rend != null)
    					renderer = rend;
                }
            }

    		originalTex = renderer.sprite.texture;
    		rt = new RenderTexture(originalTex.width, originalTex.height, 0, RenderTextureFormat.ARGB32);
    		rt.wrapMode = TextureWrapMode.Clamp;
    		ClearOutRenderTexture(rt, originalTex);
    		UpdateTextures();

    		StartCoroutine(PaintingCoroutine());

    		if (StartSound != null && StartSound != "")
    		{
    			SoundManagerTouch.Instance.AddSFXToQueue(StartSound, 1.0f, "voiceover");
    			GameManagerTouch.Instance.PlayIntroVoice();
    		}
    	}

        private void OnDestroy()
        {
    		Destroy(rt);
        }

        public void ClearOutRenderTexture(RenderTexture renderTexture, Texture2D tex = null)
    	{
    		RenderTexture lastRT = RenderTexture.active;
    		RenderTexture.active = renderTexture;
    		GL.Clear(true, true, Color.clear);
    		//if (tex != null)
    		//	Graphics.Blit(tex, rt);
    		RenderTexture.active = lastRT;
    	}

    	void UpdateTextures()
    	{
    		SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();

    		foreach (var render in renderers)
    		{
    			render.material.SetTexture("_RTTex", rt);
    		}
    	}

    	Bounds CalculateBounds(SpriteGroups.SpriteGroup group)
        {
    		Bounds bound = new Bounds();
    		for (int i = 0; i < group.Group.Count; i++)
            {
    			if (i == 0)
    				bound = group.Group[i].bounds;
    			else
    				bound.Encapsulate(group.Group[i].bounds);
            }
    		return bound;
        }

    	// Update is called once per frame
    	void Update()
    	{
    		if (!inputActive)
    			return;

    		if (nonTouchTimer > -0.5f)
    			nonTouchTimer += Time.deltaTime;

    		if (Input.touchCount > 0 && touchedGroup != null)
    		{
    			Touch touch = Input.GetTouch(0);
    			/*
    			if (touch.phase != TouchPhase.Began && touch.phase != TouchPhase.Moved)
    			{
    				if (touch.fingerId == fingerId && (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended))
    				{
    					fingerId = -1;
    				}
    				return; // Only react to these
    			}
    			*/

    			Vector3 worldPos = DragManagerTouch.GetWorldSpacePos(touch.position);
    			worldPos.z = 0f;
    			if (touch.phase == TouchPhase.Began)
    			{
    				//SpriteRenderer sr = touchedGroup.GetOverlapRenderer(worldPos);
    				//if (sr != null)
                    {
    					previousTouch = worldPos;
    					fingerId = touch.fingerId;
    				}
    			}

    			if (touch.phase == TouchPhase.Stationary && worldPos.Distance(previousTouch) < 0.01f) // For some reason Unity might report stationary while it actually moved!
    			{
    				//UpdateDrawingAudio(0f, 0.0f, 0.5f);
    				return;
    			}

    			if (touch.fingerId != fingerId)
    				return; // Only accept one finger for drawing!

    			if (GameManagerTouch.Instance.SelectedTool == GameManagerTouch.eTool.Stamp && worldPos.Distance(previousTouch) < 0.4f &&
    				touch.phase != TouchPhase.Began)
    				return;

    			checkPoints[touchedGroupIdx].RemoveAll(x => UtilsTouch.DistancePointSegment(x, previousTouch, worldPos) < POINT_DISTANCE * 2.1f); //x => x.Distance(worldPos) < POINT_DISTANCE * 2.1f);
    			float percentageDone = 1.0f - checkPoints[touchedGroupIdx].Count / (float)totalCheckPoints[touchedGroupIdx];
    			if (percentageDone > 0.01f)
    				GameManagerTouch.Instance.ShowNextGroupButton();

    			foreach (var sr in touchedGroup.Group)
    			{
    				Sprite sprite = sr.sprite;

    				Rect drawingBoundings = sprite.rect;
    				drawingBoundings.x /= sprite.texture.width;
    				drawingBoundings.y /= sprite.texture.height;
    				drawingBoundings.width /= sprite.texture.width;
    				drawingBoundings.height /= sprite.texture.height;
    				Debug.Log(drawingBoundings);

    				//Debug.Log("WorldPos: " + worldPos + "; Sprite Bounds: " + sprite.bounds + " : " + sr.bounds + "; Sprite Rect: " + sprite.rect + "; Texture Size: " + new Vector2(sprite.texture.width, sprite.texture.height));

    				Vector2 currentTexPos = GetTexturePosition(worldPos, sr);
    				Vector2 lastTexPos = GetTexturePosition(previousTouch, sr);

    				float pressure = (Input.touchPressureSupported && touch.maximumPossiblePressure > 0.1f) ? touch.pressure / touch.maximumPossiblePressure : 1.0f;
    				PaintAtTexPos(currentTexPos, lastTexPos, sr, drawingBoundings, pressure);
    				Debug.Log(sr.gameObject.name);
    			}

    			nonTouchTimer = 0f;
    			previousTouch = worldPos;

    			// Check these at the end to make sure we draw the last point
    			if (touch.fingerId == fingerId && (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended))
    			{
    				fingerId = -1;
    			}
    		}

    		if (nonTouchTimer > 10.0f)
    		{
    			GameManagerTouch.Instance.ShowNextGroupButton();
    		}
    	}

    	Vector2 GetTexturePosition(Vector3 worldPos, SpriteRenderer spriteRenderer)
        {
    		Sprite sprite = spriteRenderer.sprite;
    		return new Vector2(
    					(sprite.rect.min.x + (worldPos.x - spriteRenderer.bounds.min.x) / spriteRenderer.bounds.size.x * sprite.rect.width), // / sprite.texture.width,
    					(sprite.rect.min.y + (worldPos.y - spriteRenderer.bounds.min.y) / spriteRenderer.bounds.size.y * sprite.rect.height)); // / sprite.texture.height);
    	}

    	void PaintAtTexPos(Vector2 currentPosition, Vector2 lastPosition, SpriteRenderer sr, Rect drawingBoundings, float pressure)
    	{
    		bool isEraser = (GameManagerTouch.Instance.SelectedTool == GameManagerTouch.eTool.Eraser);
    		bool isStamp = (GameManagerTouch.Instance.SelectedTool == GameManagerTouch.eTool.Stamp);
    		Vector4 brushPos = new Vector4(currentPosition.x, currentPosition.y, lastPosition.x, lastPosition.y);
    		float finalScale = Mathf.Lerp(300.0f, Scale, pressure);
    		float finalStampScale = Mathf.Lerp(300.0f, StampScale, pressure);
    		// Scale, Hardness, Eraser (0-1), Ratio
    		// or for Stamp
    		// Scale, Rotation, Eraser (0-1), Ratio
    		Vector4 settings = new Vector4(
    			isStamp ? finalStampScale * Random.Range(0.8f, 1.3f) : finalScale, 
    			isStamp ? Random.Range(-30.0f, 30.0f) : Hardness, 
    			isEraser ? 1f : 0f, 
    			sr.sprite.texture.height / (float)sr.sprite.texture.width);
    		Material selectedMaterial = isStamp ? BlitStampMaterial : BlitPatternMaterial;
    		selectedMaterial.SetColor("_BrushColor", Color.white);
    		selectedMaterial.SetVector("_BrushSettings", settings);
    		selectedMaterial.SetVector("_BrushPosition", brushPos);
    		selectedMaterial.SetVector("_Boundings", new Vector4(drawingBoundings.min.x, drawingBoundings.min.y, drawingBoundings.max.x, drawingBoundings.max.y));
    		//Graphics.Blit(BrushTexture, rt, BlitMaterial); // pentru brush stickers

    		//Graphics.Blit(null, rt, BlitMaterial); // Pentru majoritatea
    		Graphics.Blit(originalTex, rt, selectedMaterial);
    	}

    	public void NextGroup()
        {
    		goNextElement = true;
    		inputActive = false;
    		fingerId = -1;
    	}

    	void SetAllGroupsAlphaExceptOne(float alpha, SpriteGroups.SpriteGroup group = null)
        {
    		if (flashSequence != null)
    			flashSequence.Kill(true);

    		foreach (var rendererGroup in spriteGroups.RendererGroups)
    		{
    			foreach (var renderer in rendererGroup.Group)
    			{
    				Color color = renderer.color;
    				color.a = group == rendererGroup ? 1.0f : alpha;
    				renderer.color = color;
    			}
    		}
    	}

    	Sequence flashSequence;

    	void FlashGroup(SpriteGroups.SpriteGroup group)
        {
    		if (flashSequence != null)
    			flashSequence.Kill(true);

    		flashSequence = DOTween.Sequence();

    		foreach (var renderer in group.Group)
    		{
    			flashSequence.Join(renderer.DOColor(new Color(1.0f, 0.841532f, 0.4025156f, 1.0f), 1.0f).SetEase(Ease.Flash, 4));
    		}
    	}

    	IEnumerator PaintingCoroutine()
        {
    		yield return new WaitForSeconds(1.0f);
    		/*
    		// Find the max sorting group in order to know where to put the active element (in front of others!)
    		int maxSortingOrder = -1;
    		for (int i = 0; i < spriteGroups.RendererGroups.Count; i++)
            {
    			for (int j = 0; j < spriteGroups.RendererGroups[i].Group.Count; j++)
                {
    				int spriteSortOrder = spriteGroups.RendererGroups[i].Group[j].sortingOrder;
    				if (spriteSortOrder > maxSortingOrder)
    					maxSortingOrder = spriteSortOrder;
                }
            }
    		*/
    		int maxSortingOrder = 100; // Just works... ))) ... the method above is good but... we can have elements that will render in front like Eyes and Mouth!

    		inputActive = false;
    		for (int i = 0; i < spriteGroups.RendererGroups.Count; i++)
            {
    			var currentGroup = spriteGroups.RendererGroups[i];
			
    			SetAllGroupsAlphaExceptOne(0.5f, currentGroup);
    			nonTouchTimer = -1.0f;

    			List<int> sortOrders = new List<int>();
    			for (int j = 0; j < currentGroup.Group.Count; j++)
                {
    				sortOrders.Add(currentGroup.Group[j].sortingOrder);
    				currentGroup.Group[j].sortingOrder += maxSortingOrder;
                }

    			touchedGroup = currentGroup;
    			goNextElement = false;
    			touchedGroupIdx = i;

    			yield return AnimateCamera(currentGroup);
    			inputActive = true;

    			FlashGroup(currentGroup);

    			yield return new WaitUntil(() => goNextElement); // See NextGroup method for how this wait is ended

    			for (int j = 0; j < currentGroup.Group.Count; j++)
    			{
    				currentGroup.Group[j].sortingOrder = sortOrders[j];
    			}
    		}


    		// Restore alpha
    		foreach (var rendererGroup in spriteGroups.RendererGroups)
    		{
    			foreach (var renderer in rendererGroup.Group)
    			{
    				Color color = renderer.color;
    				color.a = 1.0f;
    				renderer.color = color;
    			}
    		}

    		// Go to full view
    		Sequence s = DOTween.Sequence()
    					.Append(Camera.main.transform.DOMove(new Vector3(0f, 0f, -10.0f), TimeToGetToDestination))
    					.Join(Camera.main.DOOrthoSize(BaseOrthoSize, TimeToGetToDestination));

    		yield return new WaitUntil(() => !s.IsActive());

    		yield return CreateIcon();

    		animator.enabled = true;
    		animator.Play("Play");

    		GameManagerTouch.Instance.ShowDoneButton();
    	}

    	IEnumerator AnimateCamera(SpriteGroups.SpriteGroup group)
    	{
    		Camera mainCamera = Camera.main;

    		//Bounds uiBounds = new Bounds()

    		Bounds bound = CalculateBounds(group);

    		float screenRatio = (GameManagerTouch.Instance.UIBounds.size.x) / GameManagerTouch.Instance.UIBounds.size.y;
    		float boundRatio = bound.size.x / bound.size.y;
    		float wantedSize;
    		if (screenRatio > boundRatio)
    		{
    			wantedSize = bound.size.y * 0.5f;
    		}
    		else
    		{
    			wantedSize = bound.size.x / screenRatio * 0.5f;
    		}
    		Debug.Log("Wanted Size Before " + wantedSize);
    		float addedSize = 0f; // Mathf.Max(0f, (boundRatio - 1.0f) * 0.4f); // This is for UI, as we have lateral UI the sprite can get under the UI, so we use > 1 bound ratio as additional padding
    		Debug.Log("Bound Ratio: " + boundRatio + " : Screen Ratio: " + screenRatio);
    		Debug.Log("Added Size " + (wantedSize + 0.5f + addedSize));
    		wantedSize = Mathf.Clamp(wantedSize + 0.5f + addedSize, 1.0f, BaseOrthoSize); // Limit the zoom!
    		Debug.Log("Wanted Size After " + wantedSize);

    		Vector3 targetCamPos = bound.center.SetZ(Camera.main.transform.position.z) - GameManagerTouch.Instance.UIBounds.center * (wantedSize / BaseOrthoSize);

    		// Clamp to the screen boundings
    		float halfScreenHeight = BaseOrthoSize;
    		float halfScreenWidth = halfScreenHeight * mainCamera.aspect;
    		float halfWantedScreenHeight = wantedSize;
    		float halfWantedScreenWidth = halfWantedScreenHeight * mainCamera.aspect;
    		targetCamPos.x = Mathf.Clamp(targetCamPos.x, -halfScreenWidth + halfWantedScreenWidth, halfScreenWidth - halfWantedScreenWidth);
    		targetCamPos.y = Mathf.Clamp(targetCamPos.y, -halfScreenHeight + halfWantedScreenHeight, halfScreenHeight - halfWantedScreenHeight);

    		Sequence s = DOTween.Sequence()
    			.Append(Camera.main.transform.DOMove(targetCamPos, TimeToGetToDestination).SetEase(Ease.OutQuad))
    			.Join(Camera.main.DOOrthoSize(wantedSize, TimeToGetToDestination).SetEase(Ease.OutQuad))
    			.InsertCallback(TimeToGetToDestination * 0.5f, () => inputActive = true);

    		yield return new WaitUntil(() => !s.IsActive());
    	}

    	public Coroutine CreateIcon()
    	{
    		// Icon
    		Bounds bounds = new Bounds();
    		foreach (var rendererGroup in spriteGroups.RendererGroups)
    		{
    			foreach (var renderor in rendererGroup.Group)
    			{
    				bounds.Encapsulate(renderor.bounds);
    			}
    		}
    		return null /*CaptureManager.Instance.CreateIcon(bounds, new GameObject[] { GameManagerTouch.Instance.LeftBoundObject.parent.gameObject })*/;
    	}

    	public void SetStamp(Sprite sprite)
    	{
    		BlitStampMaterial.SetTexture("_StampTex", sprite.texture);
    	}

    	public void SetPattern(Sprite sprite)
    	{
    		BlitPatternMaterial.SetTexture("_PatternTex", sprite.texture);
    	}

    	void BuildCheckPoints()
    	{
    		checkPoints = new List<List<Vector3>>(spriteGroups.RendererGroups.Count);
    		for (int i = 0; i < spriteGroups.RendererGroups.Count; i++)
    		{
    			var spriteGroup = spriteGroups.RendererGroups[i];
    			List<Vector3> points = new List<Vector3>();
    			for (int j = 0; j < spriteGroup.Group.Count; j++)
    			{
    				List<Vector3> spritePoints = GeneratePointsInBounds(spriteGroup.Group[j].bounds, POINT_DISTANCE);
    				spritePoints.RemoveAll(x => !spriteGroup.Colliders[j].OverlapPoint(x));
    				points.AddRange(spritePoints);
    			}
    			checkPoints.Add(points);
    		}

    		totalCheckPoints = new List<int>();
    		for (int i = 0; i < checkPoints.Count; i++)
    		{
    			totalCheckPoints.Add(checkPoints[i].Count);
    		}
    	}

    	List<Vector3> GeneratePointsInBounds(Bounds bounds, float distance)
    	{
    		int nrXPoints = Mathf.CeilToInt(bounds.size.x / distance);
    		int nrYPoints = Mathf.CeilToInt(bounds.size.y / distance);
    		List<Vector3> retList = new List<Vector3>(nrXPoints * nrYPoints);

    		for (int i = 0; i <= nrXPoints; i++)
    		{
    			for (int j = 0; j <= nrYPoints; j++)
    			{
    				Vector3 pt = bounds.min + new Vector3(distance * i, distance * j, 0f);
    				pt.z = 0f;
    				retList.Add(pt);
    			}
    		}
    		return retList;
    	}

    #if UNITY_EDITOR || DEVELOPMENT_BUILD
    	private void OnGUI()
    	{
    		//if (CaptureManager.Instance.ScreenshotIsRunning)
    		//	return;

    		//GUIStyle myButtonStyle = new GUIStyle(GUI.skin.button);
    		//myButtonStyle.fontSize = 50;
    		//if (GUI.Button(new Rect(200, 500, 300, 150), "ShowSticker!", myButtonStyle))
    		//{
    		//	Camera.main.transform.position = new Vector3(0f, 0f, -10.0f);
    		//	Camera.main.orthographicSize = 5.0f;
    		//	GameManagerTouch.Instance.ShowStickerReward();
    		//}
    		//if (GUI.Button(new Rect(200, 650, 300, 150), "SkipGroups!", myButtonStyle))
    		//{
    		//	goNextElement = true;
    		//}

    		//if (GUI.Button(new Rect(200, 800, 300, 150), "Clear!", myButtonStyle))
    		//{
    		//	ClearOutRenderTexture(rt, originalTex);
    		//}
    		///*
    		//if (GUI.Button(new Rect(200, 950, 300, 150), "Gen Icon!", myButtonStyle))
    		//{
    		//	Camera.main.transform.position = new Vector3(0f, 0f, -10.0f);
    		//	Camera.main.orthographicSize = 5.0f;
    		//	SetAllGroupsAlphaExceptOne(1.0f);
    		//	CreateIcon();
    		//}
    		//*/

    		//if (checkPoints != null && totalCheckPoints != null && touchedGroupIdx >= 0)
    		//{
    		//	GUIStyle fontStyle = new GUIStyle(GUI.skin.label);
    		//	fontStyle.normal.textColor = Color.black;
    		//	fontStyle.fontSize = 40;
    		//	float percentageDone = 1.0f - checkPoints[touchedGroupIdx].Count / (float)totalCheckPoints[touchedGroupIdx];
    		//	GUI.Label(new Rect(350f, 50.0f, 100.0f, 50.0f), (percentageDone * 100.0f).ToString("0.00") + "%", fontStyle);
    		//}
    	}
    #endif
    	private void OnDrawGizmos()
    	{
    		if (touchedGroupIdx < 0)
    			return;
    		for (int i = 0; i < checkPoints[touchedGroupIdx].Count; i++)
    		{
    			Gizmos.DrawCube(checkPoints[touchedGroupIdx][i], Vector3.one * 0.01f);
    		}
    	} 
    }


}