using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

namespace Interactive.PuzzelShape
{
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

    public class ShapeGameSetup : MonoBehaviour
    {
        public enum eShape
        {
            None = -1,
            Cerc = 0,
            Dreptunghi = 1,
            Oval = 2,
            Patrat = 3,
            Romb = 4,
            Stea = 5,
            Triunghi = 6,
            Trapez = 7,
            Hexagon = 8,
            Octogon = 9,
            Paralelogram = 10,
            Pentagon = 11,
            Diamant = 12,
            Inima = 13,
            Luna = 14,
            Plus = 15,
            Sageata = 16,
            Semicerc = 17
        }

        string[] shapeSounds = { "Circle", "Rectangle", "Ellipse", "Square", "Romb", "Star", "Triangle","Trapezoid", "Hexagon", "Octagon", "Parallelogram","Pentagon","Diamond","Hearth","Crescent","Cross","Arrow","Semicircle"};
    	string[] colorSounds = new string[] {
    	    "Beige",
    	    "Blue",
    	    "Brown",
    	    "Green",
    	    "Grey",
    	    "Orange",
    	    "Pink",
    	    "Purple",
    	    "Red",
    	    "Yellow",
            "Cyan"
        };
    	Color[] colors = new Color[] {
    	    new Color(0.96f, 0.96f, 0.86f),    // Beige
            new Color(0.0f, 0.5f, 1.0f),       // Blue
            new Color(0.65f, 0.16f, 0.16f),    // Brown
            new Color(0.0f, 1.0f, 0.0f),       // Green
            new Color(0.5f, 0.5f, 0.5f),       // Grey
            new Color(1.0f, 0.65f, 0.0f),      // Orange
            new Color(1.0f, 0.75f, 0.8f),      // Pink
            new Color(0.5f, 0.2f, 1.0f),       // Purple
            new Color(1.0f, 0.0f, 0.0f),       // Red
            new Color(1.0f, 1.0f, 0.0f),       // Yellow
            new Color(0.0f, 1.0f, 1.0f)        // Cyan
        };

    	[System.Serializable]
        public class ShapePairDesc
        {
            public SpawnZone Geometry;
            public SpawnZone Hole;
            public eShape[] AllowedShapes;

            public eShape GetRandomShape(List<eShape> BannedShapes)
            {
                UtilsShape.Shuffle(AllowedShapes);
                for (int i = 0; i < AllowedShapes.Length; i++)
                {
                    if (!BannedShapes.Contains(AllowedShapes[i]))
                        return AllowedShapes[i];
                }
                return eShape.None;
            }

            public eShape GetRandomShape()
            {
                UtilsShape.Shuffle(AllowedShapes);
                return AllowedShapes[0];
            }
        }

        public bool RandomColor = false;
        public bool AllowSameShapes = false;
        public float MinimumScale = 0.5f;
        public float MaximumScale = 1.5f;
        [Range(1,10)]
        public int ScaleDivisors = 1;

        public ShapePairDesc[] ShapePairs;

        CustomRGBColors.ColorSampler ColorSampler;
        List<Collider2D> DraggableGeo = new List<Collider2D>();
        List<Collider2D> TargetGeo = new List<Collider2D>();
        List<eShape> ShapesSpawned = new List<eShape>(); // Will hold the shapes from DraggableGeo...

        HashSet<int> spawnedFigures = new HashSet<int>(); // Info about all the spawned shapes and their colors and scales
        int sortOrder = 100;

        int GenerateHash(eShape shape, int colorIdx, int scaleIdx)
        {
            int result = colorIdx << 16 + scaleIdx << 8 + (int)shape;
            return result;
        }

    	string FindClosestColorName(Color color)
    	{
    		int closestIndex = -1;
    		float closestDistance = float.MaxValue;

    		for (int i = 0; i < colors.Length; i++)
    		{
    			float distance = ColorDistance(colors[i], color);
    			if (distance < closestDistance)
    			{
    				closestDistance = distance;
    				closestIndex = i;
    			}
    		}

    		return colorSounds[closestIndex];
    	}

    	float ColorDistance(Color a, Color b)
    	{
    		return Mathf.Sqrt(Mathf.Pow(a.r - b.r, 2) + Mathf.Pow(a.g - b.g, 2) + Mathf.Pow(a.b - b.b, 2));
    	}

    	// Start is called before the first frame update
    	void Start()
        {
            List<eShape> bannedShapes = new List<eShape>(); // This is for actual shapes (not full info about shapes, like colors, scale, ...)
            CustomRGBColors colors = GameManagerShape.Instance.RGBColors;
            ColorSampler = colors.GetNewSampler();
            CustomRGBColors.ColorDef color = ColorSampler.GetNextColor();
            for (int i = 0; i < ShapePairs.Length; i++)
            {
                ShapePairDesc desc = ShapePairs[i];
                eShape shape = desc.GetRandomShape(bannedShapes);
                if (shape == eShape.None)
                    shape = desc.GetRandomShape();

                RGBMaskController geo = Instantiate(GameManagerShape.Instance.Geometries[(int)shape], transform).GetComponent<RGBMaskController>();
                RGBMaskController hole = Instantiate(GameManagerShape.Instance.GeometryHoles[(int)shape], transform).GetComponent<RGBMaskController>();

                int scaleIndex = Random.Range(0, ScaleDivisors + 1);

                // Avoid same shape/scale/color
                int tries = 100;
                while (spawnedFigures.Contains(GenerateHash(shape, ColorSampler.CurrentColorIndex, scaleIndex)) && tries-- > 0)
                {
                    Debug.Log("FoundCollision!");
                    scaleIndex = Random.Range(0, ScaleDivisors + 1);
                }

                float scaleIntervals = (MaximumScale - MinimumScale) / ScaleDivisors;
                float scale = MinimumScale + scaleIntervals * scaleIndex;
                //float scale = Random.Range(MinimumScale, MaximumScale);

                geo.transform.position = desc.Geometry.GetRandomPositionInside();
                hole.transform.position = desc.Hole.GetRandomPositionInside();
                geo.transform.localScale = hole.transform.localScale = Vector3.zero;

                geo.transform.DOScale(Vector3.one * scale, 0.5f).SetEase(Ease.OutBack).SetDelay(i * 0.2f);
                hole.transform.DOScale(Vector3.one * scale, 0.5f).SetEase(Ease.OutBack).SetDelay(0.2f + i * 0.2f);

                geo.SetRedColor(color.RedColor);
                geo.SetGreenColor(color.GreenColor);
                geo.SetBlueColor(color.BlueColor);

                hole.SetGreenColor(color.RedColor);
                hole.SetBlueColor(color.GreenColor);

                if (!AllowSameShapes)
                    bannedShapes.Add(shape);

                int hash = GenerateHash(shape, ColorSampler.CurrentColorIndex, scaleIndex);
                spawnedFigures.Add(hash);

                if (RandomColor)
                    color = ColorSampler.GetNextColor();

                DraggableGeo.Add(geo.GetComponent<Collider2D>());
                TargetGeo.Add(hole.GetComponent<Collider2D>());
                ShapesSpawned.Add(shape);
            }
        }

        private void Update()
        {
            if (DragManagerShape.Instance.IsDragging())
            {
                const float DropRadius = 2.0f;
                for (int i = 0; i < DraggableGeo.Count; i++)
                {
                    if (DraggableGeo[i].enabled && TargetGeo[i].transform.position.Distance(DraggableGeo[i].transform.position) < DropRadius)
                    {
                        DragManagerShape.Instance.RemoveDrag(DraggableGeo[i].transform);
                        AnimateToPosition(DraggableGeo[i], TargetGeo[i]);
                    }
                }
            }
        }

        Tween holeShake;
        Tween pieceTouch;

        private void OnTouch(Touch touch)
        {
            Vector3 worldPos = DragManagerShape.GetWorldSpacePos(touch.position);
            bool found = false;
            foreach (var geo in DraggableGeo)
            {
                if (geo.enabled && geo.OverlapPoint(worldPos))
                {
                    geo.GetComponent<SpriteRenderer>().sortingOrder = sortOrder++;
                    DragManagerShape.Instance.AddDrag(touch.fingerId, geo.transform, worldPos, Vector3.zero);
                    if (pieceTouch != null)
                        pieceTouch.Kill(true);
                    pieceTouch = geo.transform.DOScale(geo.transform.localScale.x * 1.1f, 0.2f).SetEase(Ease.Flash, 2.0f, 0.5f);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                foreach (var hole in TargetGeo)
                {
                    if (hole.enabled && hole.OverlapPoint(worldPos))
                    {
                        if (holeShake != null)
                            holeShake.Kill(true);
                        holeShake = hole.transform.DOShakeScale(0.5f, 0.1f, 10, 45.0f, true/*, ShakeRandomnessMode.Harmonic*/);
                        SoundManagerShape.Instance.PlaySFX("Denied");
                        break;
                    }
                }
            }
        }
        /*
        private void OnRelease(Touch touch, DragHelperShape helper)
        {
            const float DropRadius = 1.0f;
            Collider2D collider = helper.DraggingTransform.GetComponent<Collider2D>();
            int idx = DraggableGeo.IndexOf(collider);
            if (idx >= 0)
            {
                if (TargetGeo[idx].transform.position.Distance(helper.DraggingTransform.position) < DropRadius)
                {
                    AnimateToPosition(collider, TargetGeo[idx]);
                }
            }
        }
        */

        private void AnimateToPosition(Collider2D from, Collider2D to)
        {
            from.enabled = false;
            to.enabled = false;
    		if (holeShake != null)
    			holeShake.Kill(true);
    		if (pieceTouch != null)
    			pieceTouch.Kill(true);
    		from.transform.DOMove(to.transform.position, 0.2f).SetEase(Ease.InSine).OnComplete(() => NotifyOnTarget(from, to));
        }

        private void NotifyOnTarget(Collider2D from, Collider2D to)
        {
            SoundManagerShape.Instance.PlaySFX("SuckIn", 0.5f);
            ParticleSystem ps = Instantiate<ParticleSystem>(GameManagerShape.Instance.SuckInFX);
            ps.transform.position = from.transform.position;
            var mainModule = ps.main;
            RGBMaskController rgbMask = from.GetComponent<RGBMaskController>();
            Color color = Color.Lerp(rgbMask.GreenColor, rgbMask.RedColor, 0.5f);
            color.a = Random.Range(0.5f, 0.9f);
            mainModule.startColor = color;
            var spriteModule = ps.textureSheetAnimation;
            int idx = DraggableGeo.IndexOf(from);
            spriteModule.startFrame = (float)ShapesSpawned[idx] / spriteModule.spriteCount;
            ps.gameObject.SetActive(true);
            Destroy(ps.gameObject, mainModule.duration + mainModule.startLifetime.constantMax);

    		// Sound
    		string sfxName = FindClosestColorName(rgbMask.RedColor);
    		float sfxDuration = SoundManagerShape.Instance.GetClip(sfxName).length;
    		DOTween.Sequence()
    			.AppendCallback(() => SoundManagerShape.Instance.PlaySFX(sfxName))
    			.AppendInterval(sfxDuration)
    			.AppendCallback(() => SoundManagerShape.Instance.PlaySFX(shapeSounds[(int)ShapesSpawned[idx]]));

    		// Animation
    		DOTween.Sequence()
            .Append(from.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack))
            .Join(to.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack))
            .OnComplete(() => {
                from.gameObject.SetActive(false);
                to.gameObject.SetActive(false);
                CheckDone();
            });
        }

        private void CheckDone()
    	{
            foreach (var geo in DraggableGeo)
    		{
                if (geo.gameObject.activeSelf)
                    return;
    		}
            DOTween.Sequence()
                .AppendInterval(1.0f)
                .AppendCallback(() => GameManagerShape.Instance.NextLevel());
        }

        private void OnEnable()
        {
            DragManagerShape.Instance.AddOnTouchCallback(OnTouch);
            //DragManagerShape.Instance.AddOnReleaseCallback(OnRelease);
        }

        private void OnDisable()
        {
    		if (DragManagerShape.Instance == null)
    			return;
    		DragManagerShape.Instance.RemoveOnTouchCallback(OnTouch);
            //DragManagerShape.Instance.RemoveOnReleaseCallback(OnRelease);
        }
    }


}