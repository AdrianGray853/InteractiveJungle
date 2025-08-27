using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

namespace Interactive.Touch
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

    public class TracingManagerTouch : MonoBehaviour
    {
    	public string StartSound;
    	public Transform StartObject;
    	public float MinLineDistance = 1.0f; // This is the distance from the line (outside)
    	public float MinFillDistance = 1.0f; // This is the fill distance in units (when you are on the line, but too far)
    	public float MaxHintCooldown = 7.0f;
    	float hintCooldown;

    	public TweenCallback FinishCallback; // This is called after finish animation
    	public TweenCallback DoneCallback; // This is called before the finish animation

    	TracingController tracingController;
    	TracingMesh tracingMesh;

    	int currentPath = 0;
    	int currentPoint = 0;

    	bool isDone = false; // This is a hack! Because if j's dot, ending is called two times, this will prevent it, UGLY! JUST LIKE YO MOMMA!

    	float inputCooldownTimer = -1f;

        private void Start()
        {
    		// DEBUG Icons

    		// Icon
    		//Bounds bounds = new Bounds();
    		//foreach (var rendererGroup in spriteGroups.RendererGroups)
    		//{
    		//	foreach (var renderor in rendererGroup.Group)
    		//	{
    		//		bounds.Encapsulate(renderor.bounds);
    		//	}
    		//}
    		//Bounds bounds = StartObject.GetComponent<SpriteRenderer>().bounds;
    		//CaptureManager.Instance.CreateIcon(bounds);
		

    		hintCooldown = MaxHintCooldown;
		
    		tracingController = GetComponent<TracingController>();

    		tracingMesh = GetComponent<TracingMesh>();

    		tracingController.Init();

    		if (StartSound != null && StartSound != "")
    		{
    			SoundManagerTouch.Instance.AddSFXToQueue(StartSound, 1.0f, "voiceover");
    			GameManagerTouch.Instance.PlayIntroVoice();
    		}
    	}

        void Update()
    	{
    		if (isDone)
    			return;

    		if (inputCooldownTimer > 0f)
    			inputCooldownTimer -= Time.deltaTime;

    		if (Input.touchCount > 0 && inputCooldownTimer <= 0f && tracingController.paths != null && tracingController.paths.Length > 0)
            {
    			for (int i = 0; i < Input.touchCount; i++)
                {
    				Vector3 worldPos = DragManagerTouch.GetWorldSpacePos(Input.GetTouch(i).position);
    				// This is using segments...
    				for (int j = currentPoint; j <= currentPoint + 2 && j < tracingController.paths[currentPath].points.Length - 1; j++)
    				{
    					Vector3 firstPt = tracingController.paths[currentPath].points[j];
    					Vector3 secondPt = tracingController.paths[currentPath].points[j + 1];
    					float distance = UtilsTouch.DistancePointSegment(worldPos, firstPt, secondPt);
    					if (distance < MinLineDistance)
    					{
    						float t = UtilsTouch.GetTFromProjectPointSegment(worldPos, firstPt, secondPt);
    						if (t >= 0f)
    						{
    							float fill = GetFillAmount(j, t);
    							float fillDiff = Mathf.Abs(fill - tracingMesh.Fills[currentPath]);
    							float fillDistance = fillDiff * tracingController.paths[currentPath].length;
    							if (fillDistance < MinFillDistance)
    							{
    								tracingMesh.Fills[currentPath] = Mathf.Max(tracingMesh.Fills[currentPath], fill);
    								currentPoint = j;
    								Debug.Log(fill + " " + j);

    								if (currentPoint == tracingController.paths[currentPath].points.Length - 2 && t >= 1.0f)
    								{
    									OnDone();
    								}
    							}
    						}
    					}
    				}
    			}
            }
    	}

    	float GetFillAmount(int pointIdx, float t)
        {
    		float pathLen = tracingController.paths[currentPath].length;
    		float amount = 0f;
    		for (int i = 0; i <= pointIdx; i++)
            {
    			if (i == pointIdx)
    				amount += tracingController.paths[currentPath].cachedDistances[i] * t;
    			else
    				amount += tracingController.paths[currentPath].cachedDistances[i];
    		}
    		return Mathf.Clamp01(amount / pathLen);
        }

    	void OnDone()
        {
    		isDone = true;

    		DOTween.Sequence()
    			.AppendInterval(1.0f)
    			.Append(StartObject.DOMove(tracingController.paths[currentPath].points[0], 0.5f).SetEase(Ease.InOutSine))
    			.Append(StartObject.DOPath(tracingController.paths[currentPath].points, 3.0f).SetEase(Ease.InOutSine))
    			.OnComplete(() => GameManagerTouch.Instance.ShowDoneButton());
    	}
    }


}