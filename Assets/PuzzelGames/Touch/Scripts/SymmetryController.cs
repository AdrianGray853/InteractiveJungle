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

    public class SymmetryController : MonoBehaviour
    {
    	public Material BlitMaterial;
    	public SpriteRenderer CanvasSprite;
    	public float Scale = 0.075f;
    	[Range(0f, 1f)]
    	public float Hardness = 0.9f;

    	RenderTexture rt;
    	Texture2D originalTex;

    	Vector3 previousTouch; // WorldSpace;
    	int fingerId = -1;
    	bool inputActive = true;

    	float travelDistance = 0f; // Used for Rainbow Color
    	float lastTravelDistance = 0f; // ^

    	// Start is called before the first frame update
    	void Start()
    	{
    		GameDataTouch.Instance.GameType = GameDataTouch.eGameType.Symmetry;
    		//GameManagerTouch.Instance.OnToolSelection(GameManagerTouch.eTool.Symmetry2x);

    		BlitMaterial = Instantiate(BlitMaterial);
    		originalTex = CanvasSprite.sprite.texture;
    		rt = new RenderTexture(originalTex.width, originalTex.height, 0, RenderTextureFormat.ARGB32);
    		rt.wrapMode = TextureWrapMode.Clamp;
    		ClearOutRenderTexture();
    		UpdateTextures();
    	}

        private void OnDestroy()
        {
    		Destroy(rt);
        }

        public void ClearOutRenderTexture()
    	{
    		RenderTexture lastRT = RenderTexture.active;
    		RenderTexture.active = rt;
    		GL.Clear(true, true, Color.clear);
    		//if (tex != null)
    		//	Graphics.Blit(originalTexture, rt);
    		RenderTexture.active = lastRT;
    	}

    	void UpdateTextures()
    	{
    		CanvasSprite.material.SetTexture("_RTTex", rt);
    	}

    	// Update is called once per frame
    	void Update()
    	{
    		if (inputActive && Input.touchCount > 0)
    		{
    			Touch touch = Input.GetTouch(0);
    			if (touch.phase != TouchPhase.Began && touch.phase != TouchPhase.Moved)
    			{
    				if (touch.fingerId == fingerId && (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended))
    				{
    					fingerId = -1;
    				}
    				return; // Only react to these
    			}

    			Vector3 worldPos = DragManagerTouch.GetWorldSpacePos(touch.position);
    			worldPos.z = 0f;
    			if (touch.phase == TouchPhase.Began)
    			{
    				if (CanvasSprite.GetComponent<Collider2D>().OverlapPoint(worldPos))
                    {
    					previousTouch = worldPos;
    					fingerId = touch.fingerId;
    				}
    			}

    			if (touch.fingerId != fingerId)
    				return; // Only accept one finger for drawing!

    			float pressure = (Input.touchPressureSupported && touch.maximumPossiblePressure > 0.1f) ? touch.pressure / touch.maximumPossiblePressure : 1.0f;
    			if (GameManagerTouch.Instance.SelectedTool == GameManagerTouch.eTool.Symmetry6x)
    			{
    				PaintAtWorldPos(GetSymmetryPosition(worldPos, 0, 6), GetSymmetryPosition(previousTouch, 0, 6), pressure);
    				PaintAtWorldPos(GetSymmetryPosition(worldPos, 1, 6), GetSymmetryPosition(previousTouch, 1, 6), pressure);
    				PaintAtWorldPos(GetSymmetryPosition(worldPos, 2, 6), GetSymmetryPosition(previousTouch, 2, 6), pressure);
    				PaintAtWorldPos(GetSymmetryPosition(worldPos, 3, 6), GetSymmetryPosition(previousTouch, 3, 6), pressure);
    				PaintAtWorldPos(GetSymmetryPosition(worldPos, 4, 6), GetSymmetryPosition(previousTouch, 4, 6), pressure);
    				PaintAtWorldPos(GetSymmetryPosition(worldPos, 5, 6), GetSymmetryPosition(previousTouch, 5, 6), pressure);
    			}
    			else if (GameManagerTouch.Instance.SelectedTool == GameManagerTouch.eTool.Symmetry2x)
    			{
    				PaintAtWorldPos(worldPos, previousTouch, pressure);
    				PaintAtWorldPos(GetMirrorPosition(worldPos), GetMirrorPosition(previousTouch), pressure);
    			}
    			else if (GameManagerTouch.Instance.SelectedTool == GameManagerTouch.eTool.Eraser)
    			{
    				PaintAtWorldPos(worldPos, previousTouch, pressure);
    			}

    			lastTravelDistance = travelDistance;
    			travelDistance += worldPos.Distance(previousTouch);
    			previousTouch = worldPos;
    		}
    	}

    	Vector3 GetSymmetryPosition(Vector3 position, int positionIdx, int numReflections)
        {
    		if (positionIdx == 0)
    			return position;

    		Vector3 displacement = position - CanvasSprite.transform.position;
    		float angle = Mathf.Atan2(displacement.y, displacement.x) * Mathf.Rad2Deg;
    		float angleBetweenReflections = 360f / numReflections;
    		float reflectionAngle = angle + positionIdx * angleBetweenReflections;
    		float reflectionAngleRadians = reflectionAngle * Mathf.Deg2Rad;

    		Vector3 reflectedPosition = CanvasSprite.transform.position + new Vector3(
    			displacement.magnitude * Mathf.Cos(reflectionAngleRadians),
    			displacement.magnitude * Mathf.Sin(reflectionAngleRadians),
    			position.z
    		);

    		return reflectedPosition;
    	}

    	Vector3 GetMirrorPosition(Vector3 position)
        {
    		Vector3 diff = position - CanvasSprite.transform.position;
    		diff.x *= -1.0f;
    		return CanvasSprite.transform.position + diff;
    	}

    	void PaintAtWorldPos(Vector3 worldPos, Vector3 prevTouch, float pressure)
        {
    		Sprite sprite = CanvasSprite.sprite;

    		Rect drawingBoundings = sprite.rect;
    		drawingBoundings.x /= sprite.texture.width;
    		drawingBoundings.y /= sprite.texture.height;
    		drawingBoundings.width /= sprite.texture.width;
    		drawingBoundings.height /= sprite.texture.height;
    		Debug.Log(drawingBoundings);

    		//Debug.Log("WorldPos: " + worldPos + "; Sprite Bounds: " + sprite.bounds + " : " + sr.bounds + "; Sprite Rect: " + sprite.rect + "; Texture Size: " + new Vector2(sprite.texture.width, sprite.texture.height));

    		Vector2 currentTexPos = GetTexturePosition(worldPos, CanvasSprite);
    		Vector2 lastTexPos = GetTexturePosition(prevTouch, CanvasSprite);

    		PaintAtTexPos(currentTexPos, lastTexPos, CanvasSprite, drawingBoundings, pressure);
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
    		Vector4 brushPos = new Vector4(currentPosition.x, currentPosition.y, lastPosition.x, lastPosition.y);
    		float finalScale = Mathf.Lerp(300.0f, Scale, pressure);
    		// Scale, Hardness, Eraser (0-1), Ratio
    		Vector4 settings = new Vector4(finalScale, Hardness, 1f, sr.sprite.texture.height / (float)sr.sprite.texture.width);
    		if (GameManagerTouch.Instance.RainbowColor && !isEraser)
    		{
    			BlitMaterial.SetColor("_BrushStartColor", Color.HSVToRGB(Mathf.Repeat(lastTravelDistance * 0.1f, 1.0f), 1.0f, 1.0f));
    			BlitMaterial.SetColor("_BrushEndColor", Color.HSVToRGB(Mathf.Repeat(travelDistance * 0.1f, 1.0f), 1.0f, 1.0f));
    		}
    		else
    		{
    			Color color = isEraser ? Color.white : GameManagerTouch.Instance.SelectedColor;
    			BlitMaterial.SetColor("_BrushStartColor", color);
    			BlitMaterial.SetColor("_BrushEndColor", color);
    		}
    		BlitMaterial.SetColor("_BrushColor", isEraser ? Color.white : GameManagerTouch.Instance.SelectedColor);
    		BlitMaterial.SetVector("_BrushSettings", settings); 
    		BlitMaterial.SetVector("_BrushPosition", brushPos);
    		BlitMaterial.SetVector("_Boundings", new Vector4(drawingBoundings.min.x, drawingBoundings.min.y, drawingBoundings.max.x, drawingBoundings.max.y));
    		//Graphics.Blit(BrushTexture, rt, BlitMaterial); // pentru brush stickers

    		//Graphics.Blit(null, rt, BlitMaterial); // Pentru majoritatea
    		Graphics.Blit(originalTex, rt, BlitMaterial); // Pentru Outline
    	}

    #if UNITY_EDITOR || DEVELOPMENT_BUILD

    	private void OnGUI()
    	{
    		GUIStyle myButtonStyle = new GUIStyle(GUI.skin.button);
    		myButtonStyle.fontSize = 50;

    		if (GUI.Button(new Rect(200, 650, 300, 150), "TestMenu!", myButtonStyle))
    		{
    			SceneLoader.Instance.LoadScene("TESTING");
    		}
    		if (GUI.Button(new Rect(200, 800, 300, 150), "Clear!", myButtonStyle))
    		{
    			ClearOutRenderTexture();
    		}
    	}
    #endif
    }


}