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

    public class TouchAndFillController : MonoBehaviour
    {
    	public string StartSound;
    	public Material BlitMaterial;
    	public float Scale = 0.1f;
    	public float ScalePower = 0.1f;
    	[Range(0f, 1f)]
    	public float Hardness = 0f;
    	public float FillSpeed = 1.0f;
    	Animator animator;

    	RenderTexture rt;
    	Texture2D originalTex;
    	Rect drawingBoundings;

    	class FillDesc
        {
    		public Vector3 touchPos; // Worldpos
    		public SpriteGroups.SpriteGroup group;
    		public float time; // 0 - 1
    		public Color color;
        }

    	List<FillDesc> touchList = new List<FillDesc>(); // WorldSpace;
    	SpriteGroups spriteGroups;
    	HashSet<SpriteGroups.SpriteGroup> elementsDone = new HashSet<SpriteGroups.SpriteGroup>();

    	bool inputActive = true;

    	// Start is called before the first frame update
    	void Start()
    	{
    		inputActive = true;
    		spriteGroups = GetComponent<SpriteGroups>();
    		GameDataTouch.Instance.GameType = GameDataTouch.eGameType.TouchAndFill;

    		//CreateIcon();

    		BlitMaterial = Instantiate(BlitMaterial);
    		animator = GetComponentInChildren<Animator>();
    		animator.keepAnimatorStateOnDisable = true;
    		animator.enabled = false;
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

    	// Update is called once per frame
    	void Update()
    	{
    		if (inputActive && Input.touchCount > 0)
    		{
    			Touch touch = Input.GetTouch(0);
    			Vector3 worldPos = DragManagerTouch.GetWorldSpacePos(touch.position);
    			worldPos.z = 0f;
    			if (touch.phase == TouchPhase.Began)
    			{
    				SpriteGroups.SpriteGroup spriteGroup = spriteGroups.GetOverlapGroup(worldPos);
    				if (spriteGroup != null)
                    {
    					touchList.Add(
    						new FillDesc() { 
    							group = spriteGroup, 
    							time = 0f, 
    							touchPos = worldPos, 
    							color = GameManagerTouch.Instance.SelectedTool == GameManagerTouch.eTool.Fill ? GameManagerTouch.Instance.SelectedColor : Color.white
    						});
    					elementsDone.Add(spriteGroup);

    					if (elementsDone.Count >= spriteGroups.RendererGroups.Count)
                        {
    						inputActive = false;
    						StartCoroutine(ShowDoneButton());
                        }
                    }
    			}
    		}

    		for (int i = 0; i < touchList.Count; i++)
    		{
    			FillDesc fill = touchList[i];
    			fill.time += Time.deltaTime * FillSpeed;
    			SpriteGroups.SpriteGroup spriteGroup = fill.group;
    			if (fill.time > 1.0f)
    			{
    				fill.time = Mathf.Clamp01(fill.time);
    				Debug.Log("Circle Finished!");
    			}

    			foreach (var sr in spriteGroup.Group)
    			{
    				Sprite sprite = sr.sprite;
    				//Texture2D tex = sprite.texture;
    				//if (touch.phase == TouchPhase.Began)
    				{
    					drawingBoundings = sprite.rect;
    					drawingBoundings.x /= sprite.texture.width;
    					drawingBoundings.y /= sprite.texture.height;
    					drawingBoundings.width /= sprite.texture.width;
    					drawingBoundings.height /= sprite.texture.height;
    					Debug.Log(drawingBoundings);
    				}
    				//Debug.Log("WorldPos: " + worldPos + "; Sprite Bounds: " + sprite.bounds + " : " + sr.bounds + "; Sprite Rect: " + sprite.rect + "; Texture Size: " + new Vector2(sprite.texture.width, sprite.texture.height));
    				Vector2 texPos = new Vector2(
    					(sprite.rect.min.x + (fill.touchPos.x - sr.bounds.min.x) / sr.bounds.size.x * sprite.rect.width),
    					(sprite.rect.min.y + (fill.touchPos.y - sr.bounds.min.y) / sr.bounds.size.y * sprite.rect.height));

    				PaintAtTexPos(texPos, sr, Mathf.Lerp(Scale, 0.0001f, Mathf.Pow(fill.time, ScalePower)), fill.color);
    				Debug.Log(sr.gameObject.name);
    			}
    		}

    		touchList.RemoveAll(x => x.time >= 1.0f);
    	}

    	void PaintAtTexPos(Vector2 position, SpriteRenderer sr, float radius, Color color)
    	{
    		Debug.Log(position);
    		//Vector4 transform = new Vector4(position.x, position.y, Scale, 0f);
    		Vector4 brushPos = new Vector4(position.x, position.y, 0f, 0f); // Merge these!
    		Vector4 settings = new Vector4(radius, Hardness, 0f, sr.sprite.texture.height / (float)sr.sprite.texture.width);
    		BlitMaterial.SetColor("_BrushColor", color);
    		BlitMaterial.SetVector("_BrushSettings", settings); // for Pattern
    		BlitMaterial.SetVector("_BrushPos", brushPos);
    		BlitMaterial.SetVector("_Boundings", new Vector4(drawingBoundings.min.x, drawingBoundings.min.y, drawingBoundings.max.x, drawingBoundings.max.y));
    		//Graphics.Blit(BrushTexture, rt, BlitMaterial); // pentru brush stickers

    		//Graphics.Blit(null, rt, BlitMaterial); // Pentru majoritatea
    		Graphics.Blit(originalTex, rt, BlitMaterial); // Pentru Outline
    	}

    	public IEnumerator ShowDoneButton()
    	{
    		yield return new WaitUntil(() => touchList.Count == 0);

    		yield return CreateIcon();

    		animator.enabled = true;
    		animator.Play("Play");

    		GameManagerTouch.Instance.ShowDoneButton();
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
    		return CaptureManager.Instance.CreateIcon(bounds, new GameObject[] { GameManagerTouch.Instance.LeftBoundObject.parent.gameObject });
    	}

    #if UNITY_EDITOR || DEVELOPMENT_BUILD
    	private void OnGUI()
    	{
    		//if (CaptureManager.Instance.ScreenshotIsRunning)
    		//	return;

    		//GUIStyle myButtonStyle = new GUIStyle(GUI.skin.button);
    		//myButtonStyle.fontSize = 50;
    		//if (GUI.Button(new Rect(200, 500, 300, 150), "Animate!", myButtonStyle))
    		//{
    		//	animator.enabled = true;
    		//	animator.Play("Play");
    		//}
    		//if (GUI.Button(new Rect(200, 650, 300, 150), "TestMenu!", myButtonStyle))
    		//{
    		//	SceneLoader.Instance.LoadScene("TESTING");
    		//}

    		//if (GUI.Button(new Rect(200, 800, 300, 150), "Clear!", myButtonStyle))
    		//{
    		//	ClearOutRenderTexture(rt, originalTex);
    		//}
    	}
    #endif
    }


}