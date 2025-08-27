using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

    public class TracingManager : MonoBehaviour
    {
    	public char Letter;
    	[Tooltip("Tracing Controller\nif null the local controller will be picked")]
    	public TracingController Controller;
    	public float MaxHintCooldown = 7.0f;
    	float hintCooldown;
    	[Header(".: Dot Info")]
    	public GameObject DotPrefab;
    	public GameObject DotCapPrefab;
    	public float ScaleFactor;
    	public float RotationOffset;
    	public int[] NrDots = new int[] { 5 };

    	[Header(".: Spaceships Info")]
    	public SpaceshipsConfiguration SpaceshipsConfig;

    	public TweenCallback FinishCallback; // This is called after finish animation
    	public TweenCallback DoneCallback; // This is called before the finish animation

    	List<GameObject>[] spawnedDots;
    	GameObject spawnedShip;
    	int spaceshipIdx;
    	Vector3 spaceshipLookAtTarget = Vector3.zero;
    	List<GameObject> spawnedTargets = new List<GameObject>();

    	TracingMesh tracingMesh;

    	bool initiated = false;

    	// Start is called before the first frame update
    	public void Init(bool doFinishAnim = true)
    	{
    		hintCooldown = MaxHintCooldown;
    		finishIt = doFinishAnim;
    		initiated = true;
    		toUpdate = false;
    		if (Controller == null)
    			Controller = GetComponent<TracingController>();

    		tracingMesh = GetComponent<TracingMesh>();

    		Controller.Init();

    		spawnedDots = new List<GameObject>[Controller.paths.Length];
    		for (int i = 0; i < Controller.paths.Length; i++)
    			spawnedDots[i] = new List<GameObject>();

    		SpawnDots();
    		SpawnShips();
    	}

    	bool toUpdate = false;

    #if UNITY_EDITOR
    	private void OnValidate()
    	{
    		toUpdate = true;
    	}
    #endif

    	int currentPath = 0;
    	int currentPoint = -1;
    	int shipPoint = 0;
    	int shipPath = 0;

    	float MinFingerDistance = 1.0f;
    	float shipSpeed = 1.5f;
    	float shipSpeedAddPerDot = 1.5f;

    	bool isDone = false; // This is a hack! Because if j's dot, ending is called two times, this will prevent it, UGLY! JUST LIKE YO MOMMA!
    	bool finishIt = true; // Sets if it should do a finish animation or wait for external call

    	float inputCooldownTimer = -1f;

    	void Update()
    	{
    		if (!initiated)
    			return;

    		if (isDone)
    			return;

    		if (inputCooldownTimer > 0f)
    			inputCooldownTimer -= Time.deltaTime;

    		if (Input.touchCount > 0 && inputCooldownTimer <= 0f)
            {
    			for (int i = 0; i < Input.touchCount; i++)
                {
    				Vector3 worldPos = DragManager.GetWorldSpacePos(Input.GetTouch(i).position);
    				/* // This is using segments...
    				for (int j = currentPoint; j < currentPoint + 2 && j < spawnedDots[currentPath].Count - 1; j++)
                    {
    					Vector3 firstPt = spawnedDots[currentPath][j].transform.position;
    					Vector3 secondPt = spawnedDots[currentPath][j + 1].transform.position;
    					float distance = Utils.DistancePointSegment(worldPos, firstPt, secondPt);
    					if (distance < MinFingerDistance)
                        {
    						for (int k = currentPoint; k <= j; k++)
    							spawnedDots[currentPath][k].transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;
    						currentPoint = j;
                        }
                    }
    				*/
    				// This is using point radius
    				int start = currentPoint + 1;
    				int end = currentPoint + 2; // TODO: expose 2?
    				for (int j = start; j < end && j < spawnedDots[currentPath].Count; j++)
    				{
    					Vector3 dotPos = spawnedDots[currentPath][j].transform.position;
    					if (worldPos.Distance(dotPos) < MinFingerDistance)
    					{
    						hintCooldown = -1.0f;
    						FingerHintController.Instance?.Hide();
    						for (int k = start; k <= j; k++)
    						{
    							spawnedDots[currentPath][k].transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;
    							spawnedDots[currentPath][k].transform.DOScale(spawnedDots[currentPath][k].transform.localScale * 1.7f, 0.3f)
    								.SetEase(Ease.Flash, 2)
    								.SetDelay(0.5f);
    						}
    						currentPoint = j;
    					}
    				}
    			}
            }

    		// Update ship
    		if (shipPoint <= currentPoint && shipPoint < spawnedDots[currentPath].Count || shipPath != currentPath)
            {
    			int targetPoint = Mathf.Clamp(currentPoint, 0, spawnedDots[currentPath].Count - 1);
    			int diff;
    			Vector3 targetPos;
    			if (shipPath == currentPath)
    			{ // Mathf.Min is for the case when there is a dot, like in i, or j.
    				targetPos = spawnedDots[currentPath][Mathf.Min(targetPoint, shipPoint + 1)].transform.position;
    				diff = targetPoint - shipPoint;
    			} 
    			else
                {
    				targetPos = spawnedDots[currentPath][0].transform.position;
    				diff = 10; // Arbitrary speed multiplier... expose?
    			}

    			spaceshipLookAtTarget = targetPos;

    			spawnedShip.transform.position = Vector3.MoveTowards(spawnedShip.transform.position, targetPos, 
    				Time.deltaTime * (shipSpeed + shipSpeedAddPerDot * diff));
    			if (spawnedShip.transform.position.DistanceSq(targetPos) < 0.0001f)
                {
    				if (shipPath == currentPath)
    				{
    					shipPoint++;
    				}
    				else
    				{
    					shipPath = currentPath;

    					// Update target orientation
    					if (spawnedDots[currentPath].Count > 1)
    						spaceshipLookAtTarget = spawnedDots[currentPath][1].transform.position;
    					else
    						spaceshipLookAtTarget = spawnedShip.transform.position + Vector3.up;
    				}

    				// We need to check also the currentPoint because in case of dot (i, j) it will have only one point, and shipPoint will be at the end but currentPoint -1 which will wait for a touch
    				if (shipPoint >= spawnedDots[currentPath].Count - 1 && currentPoint == spawnedDots[currentPath].Count - 1)
                    {
    					SoundManager.Instance.PlaySFX("TracingSpaceshipTarget");
    					if (currentPath < spawnedDots.Length - 1)
    					{
    						inputCooldownTimer = 0.5f;
    						spawnedShip.GetComponent<SpriteRenderer>().DOFade(0.2f, 1.0f).SetEase(Ease.Flash, 8.0f); //DOColor(Color.green, 1.0f).SetEase(Ease.Flash, 8.0f);
    						// Old path
    						spawnedTargets[currentPath].SetActive(false);
    						//foreach (var dot in spawnedDots[currentPath])
    						//	dot.SetActive(false);

    						currentPath++;
    						currentPoint = -1;
    						shipPoint = 0;

    						// New path
    						spawnedTargets[currentPath].SetActive(true);
    						for (int j = 0; j < spawnedDots[currentPath].Count; j++)
    						{
    							GameObject dot = spawnedDots[currentPath][j];
    							dot.transform.DOScale(Vector3.zero, 0.3f).From().SetEase(Ease.OutBack).SetDelay(j * 0.1f);
    							dot.SetActive(true);
    						}
    					}
    					else
                        {
    						isDone = true;
    						Debug.Log("Done tracing!");
    						SoundManager.Instance.PlaySFX("WooshSpaceshipTracing");
    						// Hide dots
    						Sequence hideDots = DOTween.Sequence();
    						// Hide ship
    						if (currentPath < spawnedDots.Length)
    							spawnedTargets[currentPath].SetActive(false);
    						spawnedShip.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);

    						foreach (var path in spawnedDots)
                            {
    							float durationPerDot = 1.0f / path.Count;
    							foreach (var dot in path)
                                {
    								hideDots.Append(dot.transform.DOScale(Vector3.zero, durationPerDot));
                                }
                            }

    						// Animate mesh
    						if (tracingMesh != null)
    						{
    							Sequence s = DOTween.Sequence();
    							for (int i = 0; i < Controller.paths.Length; i++)
    							{
    								int idx = i;
    								s.Append(DOTween.To(() => tracingMesh.Fills[idx], (value) => tracingMesh.Fills[idx] = value, 1.0f, 1.0f));
    							}
    							if (DoneCallback != null)
    							{
    								s.AppendCallback(DoneCallback);
    							}

    							if (finishIt)
                                {
    								DoFinishAnimation(s);
                                }
    						}
						
    						if (GameManager.Instance != null && Letter != '\0')
    						{
    							SoundManager.SFXStack stack;
    							if (char.IsUpper(Letter))
    								stack = SoundManager.Instance.PlaySFXStack("trace_uppercase", 1.0f, "voiceover", 2);
    							else
    								stack = SoundManager.Instance.PlaySFXStack("trace_lowercase", 1.0f, "voiceover", 2);
    							stack.AddSFXStack(char.ToLower(Letter).ToString());
    						}
                        }
    				}
                }
            }

    		if (spawnedShip != null)
    		{
    			UpdateSpaceshipOrientation();
    		}

    		// This is for Debug
    		if (toUpdate)
    		{
    			SpawnDots();

    			toUpdate = false;
    		}

    		if (hintCooldown > 0)
            {
    			hintCooldown -= Time.deltaTime;
    			if (hintCooldown < 0)
                {
    				hintCooldown = MaxHintCooldown;
    				FingerHintController.Instance?.ShowDrag(Controller.paths[currentPath].points, 2.0f);
    				string[] sfxx = new string[] { "follow_blue", "move_spaceship_where"};
    				SoundManager.Instance.PlaySFX(sfxx.GetRandomElement(), 1.0f, "voiceover", 1);
    				//SoundManager.Instance.AddSFXToQueue(letter); 
    			}
            }
    	}

    	public Sequence DoFinishAnimation(Sequence s = null)
        {
    		if (s == null)
    			s = DOTween.Sequence();
    		s.AppendInterval(1.5f);
    		s.Append(transform.parent.DOMove(transform.parent.position + Vector3.down * 20.0f, 1.0f));
    		if (FinishCallback != null)
    			s.AppendCallback(FinishCallback);
    		s.AppendCallback(() => Destroy(transform.parent.gameObject));
    		return s;
    	}

    	// TODO: Check world pos!
	
    	void SpawnDots()
    	{
    		for (int pathIdx = 0; pathIdx < Controller.paths.Length; pathIdx++)
    		{
    			// This is for changes, remove when possible...
    			foreach (var obj in spawnedDots[pathIdx])
    			{
    				Destroy(obj);
    			}
    			spawnedDots[pathIdx].Clear();
    			// End Changes

    			Vector3[] path = Controller.paths[pathIdx].points;

    			float length = Controller.paths[pathIdx].length;
    			int nrDots = NrDots[Mathf.Min(pathIdx, NrDots.Length - 1)];
    			float dotDist = length / (nrDots - 1);
    			List<Vector3> spawnPositions = new List<Vector3>(nrDots);
    			int startIndex = 0;
    			float sumDistance = 0f;
    			for (int j = 0; j < nrDots; j++)
    			{
    				Vector3 pos = GetDotPosition(path, j * dotDist, Controller.paths[pathIdx].cachedDistances, ref startIndex, ref sumDistance);
    				spawnPositions.Add(pos);
    			}

    			for (int j = 0; j < spawnPositions.Count; j++)
    			{
    				//GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    				bool isCap = (j == 0 || j == spawnPositions.Count - 1);
    				GameObject go = Instantiate(isCap ? DotCapPrefab : DotPrefab, transform);
    				go.transform.position = spawnPositions[j];
    				go.transform.localScale *= ScaleFactor;
    				//float zRotation = Mathf.Atan2(path[i + 1].y - path[i].y, path[i + 1].x - path[i].x) * Mathf.Rad2Deg + RotationOffset;
    				Vector3 prevPos = j > 0 ? spawnPositions[j - 1] : spawnPositions[j];
    				Vector3 nextPos = j < spawnPositions.Count - 1 ? spawnPositions[j + 1] : spawnPositions[j];
    				float zRotation = Mathf.Atan2(nextPos.y - prevPos.y, nextPos.x - prevPos.x) * Mathf.Rad2Deg + RotationOffset;
    				go.transform.rotation = Quaternion.Euler(0f, 0f, zRotation);

    				if (pathIdx > 0)
    					go.SetActive(false);
    				else
    					go.transform.DOScale(Vector3.zero, 0.3f).From().SetEase(Ease.OutBack).SetDelay(j * 0.1f);

    				spawnedDots[pathIdx].Add(go);
    			}
    		}
    	}

    	// TODO: This is ugly, unwrap maybe?
    	private Vector3 GetDotPosition(Vector3[] path, float position, float[] distances, ref int startIndex, ref float sumDistance)
    	{
    		for (int i = startIndex; i < path.Length - 1; i++)
    		{
    			float oldDistance = sumDistance;
    			sumDistance += distances[i];
    			if (position <= sumDistance)
    			{
    				float t = (position - oldDistance) / (sumDistance - oldDistance);
    				Vector3 spawnPos = Vector3.Lerp(path[i], path[i + 1], t);
    				startIndex = i;
    				sumDistance = oldDistance; // revert old distance as we incremented it already...
    				return spawnPos;
    			}
    		}

    		return path[path.Length - 1];
    	}

    	private void SpawnShips()
        {
    		spaceshipIdx = Random.Range(0, SpaceshipsConfig.Spaceships.Length);
    		Debug.Log("Selected ship " + spaceshipIdx);
    		spawnedShip = Instantiate(SpaceshipsConfig.Spaceships[spaceshipIdx], transform);
    		spawnedShip.transform.position = spawnedDots[0][0].transform.position;
    		if (spawnedDots[0].Count > 1)
    			spaceshipLookAtTarget = spawnedDots[0][1].transform.position;
    		else // A dot? look up
    			spaceshipLookAtTarget = spawnedDots[0][0].transform.position + Vector3.up;
    		UpdateSpaceshipOrientation(true);
    		for (int i = 0; i < spawnedDots.Length; i++)
            {
    			GameObject target = Instantiate(SpaceshipsConfig.Targets[spaceshipIdx], transform);
    			target.transform.position = spawnedDots[i][spawnedDots[i].Count - 1].transform.position;
    			if (spawnedDots[i].Count > 1)
    			{
    				UpdateTargetOrientation(target.transform, target.transform.position - spawnedDots[i][spawnedDots[i].Count - 2].transform.position, true);
    			}
    			else
    			{ // Look up!
    				UpdateTargetOrientation(target.transform, Vector3.up, true);
    			}

    			if (i > 0)
    				target.SetActive(false);
    			spawnedTargets.Add(target);
    		}
        }

    	// TODO: Duplicate, refactor! Or.... ignore like you always do.... WE NEED TO BUILD FASTER FASTER FASTER CAPTAIN!!! HALTURA++#

    	void UpdateTargetOrientation(Transform target, Vector3 lookAtDiff, bool forced = false)
    	{
    		bool flipSpaceship = Utils.In(spaceshipIdx, 1);
    		bool rotateTowards = Utils.In(spaceshipIdx, 2, 3, 4, 6, 7);
    		bool needsVerticalFlip = Utils.In(spaceshipIdx, 2, 3, 4);

    		if (flipSpaceship && (Mathf.Abs(lookAtDiff.x) > 0.1f || forced))
    		{
    			Vector3 scale = target.localScale;
    			scale.x = -Mathf.Sign(lookAtDiff.x);
    			target.localScale = scale;
    		}
    		if (rotateTowards)
    		{
    			float offset = 0f;
    			if (SpaceshipsConfig.RotationOffsets != null && SpaceshipsConfig.RotationOffsets.Length > 0)
    			{
    				offset = SpaceshipsConfig.RotationOffsets[Mathf.Min(SpaceshipsConfig.RotationOffsets.Length - 1, spaceshipIdx)];
    			}

    			target.rotation = Quaternion.Euler(0f, 0f, Utils.GetAngle(lookAtDiff) + offset);


    			if (needsVerticalFlip && (Mathf.Abs(lookAtDiff.x) > 0.1f || forced))
    			{
    				Vector3 scale = target.localScale;
    				if (Mathf.Abs(Utils.RotationTo180(target.rotation.eulerAngles.z)) > 91.0f)
    					scale.y = -1f;
    				else
    					scale.y = 1.0f;
    				target.localScale = scale;
    			}

    		}
    	}

    	void UpdateSpaceshipOrientation(bool snapOrientation = false)
    	{
    		if (shipPath >= 0 && shipPath < Controller.paths.Length)
    		{
    			if (shipPoint > spawnedDots[shipPath].Count - 2)
    				snapOrientation = true;
    		}

    		// Compute Spaceship orientation
    		Vector3 spaceshipDiff = spaceshipLookAtTarget - spawnedShip.transform.position;
    		if (spaceshipDiff.sqrMagnitude > 0.001f || snapOrientation)
    		{
    			bool flipSpaceship = Utils.In(spaceshipIdx, 1);
    			bool rotateTowards = Utils.In(spaceshipIdx, 2, 3, 4, 6, 7);
    			bool needsVerticalFlip = Utils.In(spaceshipIdx, 2, 3, 4);

    			if (flipSpaceship && (Mathf.Abs(spaceshipDiff.x) > 0.1f || snapOrientation))
    			{
    				Vector3 scale = spawnedShip.transform.localScale;
    				scale.x = -Mathf.Sign(spaceshipDiff.x);
    				spawnedShip.transform.localScale = scale;
    			}
    			if (rotateTowards)
    			{
    				float offset = 0f;
    				if (SpaceshipsConfig.RotationOffsets != null && SpaceshipsConfig.RotationOffsets.Length > 0)
    				{
    					offset = SpaceshipsConfig.RotationOffsets[Mathf.Min(SpaceshipsConfig.RotationOffsets.Length - 1, spaceshipIdx)];
    				}

    				if (snapOrientation)
    				{
    					spawnedShip.transform.rotation = Quaternion.Euler(0f, 0f, Utils.GetAngle(spaceshipDiff) + offset);
    				}
    				else
    				{
    					Quaternion targetAngle = Quaternion.Euler(0f, 0f, Utils.GetAngle(spaceshipDiff) + offset);
    					float angleDiff = Mathf.Max(1.0f, Quaternion.Angle(spawnedShip.transform.rotation, targetAngle));
    					spawnedShip.transform.rotation = Quaternion.RotateTowards(
    						spawnedShip.transform.rotation,
    						targetAngle,
    						angleDiff * 10.0f * Time.deltaTime);
    				}


    				if (needsVerticalFlip)// && (Mathf.Abs(spaceshipDiff.x) > 0.1f || snapOrientation))
    				{
    					Vector3 scale = spawnedShip.transform.localScale;
    					if (Mathf.Abs(Utils.RotationTo180(spawnedShip.transform.rotation.eulerAngles.z)) > 95.0f)
    						scale.y = -1f;
    					else
    						scale.y = 1.0f;
    					spawnedShip.transform.localScale = scale;
    				}

    			}
    		}
    	}

        private void OnDrawGizmosSelected()
        {
            if (spawnedDots != null)
            {
    			Gizmos.color = Color.green;
    			for (int i = 0; i < spawnedDots.Length; i++)
                {
    				if (spawnedDots[i] != null)
                    {
    					for (int j = 0; j < spawnedDots[i].Count; j++)
                        {
    						Gizmos.DrawWireSphere(spawnedDots[i][j].transform.position, MinFingerDistance);
                        }
                    }
                }
            }
        }

    //#if UNITY_EDITOR
    	//private void OnGUI()
    	//{
    	//	if (GUI.Button(new Rect(0, 0, 300, 300), "Start") && !initiated)
    		//	Init();
    	//}
    //#endif
    }


}