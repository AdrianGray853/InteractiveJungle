using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.Touch
{
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class CaptureManager : MonoBehaviour
    {
        public bool ScreenshotIsRunning { get; private set; } = false;
        public NativeGallery.Permission ScreenshotPermission { get; private set; } = NativeGallery.Permission.ShouldAsk;

        static CaptureManager _instance = null;
        public static CaptureManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    Instantiate(Resources.Load("Managers") as GameObject);
                }
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        private void Awake()
    	{
            _instance = this;
            ScreenshotPermission = NativeGallery.CheckPermission(NativeGallery.PermissionType.Write, NativeGallery.MediaType.Image);
        }

    	public void Screenshot(GameObject[] ExcludeFromPhoto = null, Bounds? bounds = null, SpriteRenderer photoFrame = null, GameObject photoIcon = null)
        {
            if (ScreenshotIsRunning)
                return;

    #if !UNITY_EDITOR
            if (ScreenshotPermission == NativeGallery.Permission.Denied)
                return;

            if (ScreenshotPermission == NativeGallery.Permission.ShouldAsk)
            {
                ScreenshotPermission = NativeGallery.RequestPermission(NativeGallery.PermissionType.Write, NativeGallery.MediaType.Image);
                if (ScreenshotPermission == NativeGallery.Permission.Denied)
                    return; // STUPID KIDO! YOU'VE PRESSED THE BUTTON!
            }
    #endif

            StartCoroutine(DelayedScreenshot(ExcludeFromPhoto, bounds, photoFrame, photoIcon));
            //StartCoroutine(DelayedIcon(eDelayedAction.NextLevel));
        }

        Sequence photoFrameSequence = null;

        IEnumerator DelayedScreenshot(GameObject[] ExcludeFromPhoto, Bounds? bounds, SpriteRenderer photoFrame, GameObject photoIcon)
        {
            ScreenshotIsRunning = true;
            List<GameObject> returnActive = new List<GameObject>();
            if (ExcludeFromPhoto != null)
            {
                for (int i = 0; i < ExcludeFromPhoto.Length; i++)
                {
                    if (!ExcludeFromPhoto[i].activeInHierarchy)
                        continue;
                    ExcludeFromPhoto[i].SetActive(false);
                    returnActive.Add(ExcludeFromPhoto[i]);
                }
            }
            Debug.Log(Time.time);
            yield return new WaitForEndOfFrame();
            Debug.Log(Time.time);
            //var texture = ScreenCapture.CaptureScreenshotAsTexture();

            //Debug.Log(bounds.Value);
            Vector3 bMin = bounds.HasValue ? Camera.main.WorldToScreenPoint(bounds.Value.min) : Vector3.zero;
            Vector3 bMax = bounds.HasValue ? Camera.main.WorldToScreenPoint(bounds.Value.max) : new Vector3(Screen.width, Screen.height, 0f);
            Debug.Log(bMin);
            Debug.Log(bMax);
            Vector3 size = bMax - bMin;
            Debug.Log(size);
            Texture2D texture = new Texture2D(Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y), TextureFormat.ARGB32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            Rect region = new Rect(Mathf.Round(bMin.x), Mathf.Round(bMin.y), texture.width, texture.height);
            Debug.Log(region);
            // Clamp the values so we never read outside the screen! (otherise ERRORR!) But... this should not happen, call Bety if happens!
            region.x = Mathf.Clamp(region.x, 0f, Screen.width);
            region.y = Mathf.Clamp(region.y, 0f, Screen.height);
            region.width = Mathf.Min(region.width, Screen.width - region.x);
            region.height = Mathf.Min(region.height, Screen.height - region.y);

            Debug.Log(region);
            Debug.Log(Screen.width + " " + Screen.height);

            texture.ReadPixels(region, 0, 0);
            texture.Apply();

            //byte[] bytes = texture.EncodeToPNG();
            //System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/Screenshots/Coloring");
            //System.IO.File.WriteAllBytes(Application.persistentDataPath + "/Screenshots/Coloring/Screenshot" + ProgressManagerTouch.Instance.GetScreenShotId() + ".png", bytes);
            //ProgressManagerTouch.Instance.SetScreenshotId(ProgressManagerTouch.Instance.GetScreenShotId() + 1);
            // cleanup
            //Destroy(texture);
            foreach (var go in returnActive)
                go.SetActive(true);

            Camera.main.Render();
            yield return null;
            Debug.Log(Time.time);

            string screenshotFileName = "Screenshot" + ProgressManagerTouch.Instance.GetScreenShotId() + System.DateTime.Now.ToString("yyyyMMddhhmmss") + ".png";
    #if UNITY_EDITOR
            byte[] bytes = texture.EncodeToPNG();
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/Screenshots");
            System.IO.File.WriteAllBytes(Application.persistentDataPath + "/Screenshots/" + screenshotFileName, bytes);
            ProgressManagerTouch.Instance.SetScreenshotId(ProgressManagerTouch.Instance.GetScreenShotId() + 1);
            // cleanup
            //Destroy(texture);
    #else
            NativeGallery.SaveImageToGallery(texture, Application.productName, screenshotFileName);
    #endif
            ProgressManagerTouch.Instance.SetScreenshotId(ProgressManagerTouch.Instance.GetScreenShotId() + 1);

            if (photoFrame != null && photoIcon != null)
            {
                if (photoFrameSequence != null)
                    photoFrameSequence.Kill(true);
                Sprite releaseSprite = photoFrame.sprite; // Fix Memory leaks
                Sprite screenSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f, 100.0f);
                photoFrame.sprite = screenSprite;
                if (bounds == null)
                {
                    photoFrame.transform.localScale = Vector3.one * (100.0f * Camera.main.orthographicSize * 2.0f / Screen.height);
                    photoFrame.transform.position = Camera.main.transform.position.SetZ(0);
                }
                else
    			{
                    photoFrame.transform.localScale = Vector3.one * (100.0f * bounds.Value.size.y / Screen.height);
                    photoFrame.transform.position = bounds.Value.center.SetZ(0);
                }
                photoFrame.gameObject.SetActive(true);


                if (releaseSprite != null)
                {
                    Destroy(releaseSprite.texture);
                    Destroy(releaseSprite);
                }

                SetSpriteColorTintOffset controller = photoFrame.GetComponent<SetSpriteColorTintOffset>();
                Vector3 iconPosition = Camera.main.ScreenToWorldPoint(photoIcon.transform.position).SetZ(0);
                photoFrameSequence = DOTween.Sequence()
                    .Append(DOTween.ToAlpha(() => controller.OffsetColor, (color) => controller.SetOffsetColor(color), 1.0f, 0.5f).SetEase(Ease.Flash, 2))
                    .Join(photoFrame.transform.DOScale(photoFrame.transform.localScale * 0.5f, 0.5f).SetEase(Ease.OutSine))
                    .AppendInterval(0.5f)
                    .Append(photoFrame.transform.DOScale(0f, 0.5f).SetEase(Ease.InSine))
                    .Join(photoFrame.transform.DOMove(iconPosition, 0.5f).SetEase(Ease.InSine))
                    .AppendCallback(() => photoFrame.gameObject.SetActive(false));
            }
            else
    		{
                Destroy(texture);
    		}

            SoundManagerTouch.Instance.PlaySFX("CameraCapture");
        
            ScreenshotIsRunning = false;
        }

        public Coroutine CreateIcon(Bounds bounds, GameObject[] ExcludeFromIcon = null)
    	{
            if (ScreenshotIsRunning)
                return null;

            return StartCoroutine(DelayedIcon(bounds, ExcludeFromIcon));
    	}

        IEnumerator DelayedIcon(Bounds bounds, GameObject[] ExcludeFromIcon)
        {
            ScreenshotIsRunning = true;
            //yield return null;
            List<GameObject> returnActive = new List<GameObject>();
            if (ExcludeFromIcon != null)
            {
                for (int i = 0; i < ExcludeFromIcon.Length; i++)
                {
                    if (!ExcludeFromIcon[i].activeInHierarchy)
                        continue;
                    ExcludeFromIcon[i].SetActive(false);
                    returnActive.Add(ExcludeFromIcon[i]);
                }
            }
            yield return new WaitForEndOfFrame();
            //var texture_tmp = ScreenCapture.CaptureScreenshotAsTexture();

            //SpriteRenderer sr = CurrentLevel.GetComponent<FillColorOnTouch>().Base;
            Vector3 bMin = Camera.main.WorldToScreenPoint(bounds.min);
            Vector3 bMax = Camera.main.WorldToScreenPoint(bounds.max);
            Vector3 size = bMax - bMin;
            Texture2D screenshotTexture = new Texture2D(Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y), TextureFormat.ARGB32, false);
            screenshotTexture.wrapMode = TextureWrapMode.Clamp;
            Rect region = new Rect(Mathf.Round(bMin.x), Mathf.Round(bMin.y), screenshotTexture.width, screenshotTexture.height);
            // Clamp the values so we never read outside the screen! (otherise ERRORR!) But... this should not happen, call Bety if happens!
            region.x = Mathf.Clamp(region.x, 0f, Screen.width);
            region.y = Mathf.Clamp(region.y, 0f, Screen.height);
            region.width = Mathf.Min(region.width, Screen.width - region.x);
            region.height = Mathf.Min(region.height, Screen.height - region.y);

            Debug.Log(region);
            Debug.Log(Screen.width + " " + Screen.height);
            screenshotTexture.ReadPixels(region, 0, 0);
            screenshotTexture.Apply();
            //Vector2 screenshotSize = new Vector2(screenshotTexture.width, screenshotTexture.height);

            /*
            Texture frame = ColoringIconMaterial.GetTexture("_FrameTex");
            */

            //screenshotSize *= UtilsTouch.FitToSizeScale(screenshotSize, new Vector2(frame.width, frame.height));
            //var resizedTexture = ScaleTexture(screenshotTexture, Mathf.CeilToInt(screenshotSize.x), Mathf.CeilToInt(screenshotSize.y));
            //Destroy(screenshotTexture);

            float scale = UtilsTouch.FitToSizeScale(size, new Vector2(464.0f, 544.0f));
            Vector2 targetSize = size * scale;

            RenderTexture rt = RenderTexture.GetTemporary(Mathf.CeilToInt(targetSize.x), Mathf.CeilToInt(targetSize.y));
            rt.filterMode = FilterMode.Bilinear;
            RenderTexture last = RenderTexture.active;
            RenderTexture.active = rt; // No need to set as Blit sets it for us, Adrian from the future: No we need it because iOS is fucked up and it doesn't clear the texture right...
            GL.Clear(true, true, Color.clear);

            Graphics.Blit(screenshotTexture, rt);
            Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
            texture.ReadPixels(new Rect(0f, 0f, rt.width, rt.height), 0, 0);

            RenderTexture.active = last;
            RenderTexture.ReleaseTemporary(rt);

            //var texture = ScaleTexture(texture_tmp, 100, 100);
            // do something with texture
            byte[] bytes = texture.EncodeToPNG();
            string dirPath = Application.persistentDataPath + "/GeneratedLevelIcons/" + GameDataTouch.Instance.GameType.ToString();
            System.IO.Directory.CreateDirectory(dirPath);
            System.IO.File.WriteAllBytes(dirPath + "/Level" + GameDataTouch.Instance.SelectedLevel + ".png", bytes);
            // cleanup
            Destroy(texture);
            Destroy(screenshotTexture);
            foreach (var go in returnActive)
                go.SetActive(true);

            Camera.main.Render();
            ScreenshotIsRunning = false;
        }

        public Sprite GetExistingLevelSprite(GameDataTouch.eGameType gameType, int levelIdx, Sprite altSprite)
        {
            string path = Application.persistentDataPath + "/GeneratedLevelIcons/" + gameType.ToString() + "/Level" + levelIdx + ".png";
            if (System.IO.File.Exists(path))
            {
                byte[] fileData = System.IO.File.ReadAllBytes(path);
                Texture2D tex2D = new Texture2D(2, 2); // Temp texture
                if (tex2D.LoadImage(fileData))
                {
                    tex2D.filterMode = FilterMode.Bilinear;
                    tex2D.wrapMode = TextureWrapMode.Clamp;
                    //tex2D.alphaIsTransparency = true;
                    tex2D.hideFlags = HideFlags.DontSave;
                    const float PixelsPerUnit = 100.0f;
                    Sprite sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0.5f, 0.5f), PixelsPerUnit, 0, SpriteMeshType.FullRect);
                    return sprite;
                }
                Destroy(tex2D);
            }

            return altSprite;
        }
    }


}