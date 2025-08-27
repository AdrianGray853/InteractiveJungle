using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class MapMenuController : MonoBehaviour
    {
    	public float MinDrag = -100.0f;
    	public float MaxDrag = 100.0f;
    	public float DragMultiplier = 20.0f;

    	public float CenterAddOffset = 0f;

    	public LevelDefinition[] Levels;
    	public LineRenderer[] Lines;
    	public string TargetScene;
    	public GameData.eGameType GameType;   

    	Vector3 initialPosition;
    	float xOffset = 0f;
    	Vector2 touchPosStart;

    	float targetOffset = 0f;
    	bool goToTarget = false;
    	bool blockInput = false;

    	int unlockAnimIdx = -1;

    	float scrollSpeed = 2.0f;
    	float goToTargetFailSafeTimer = 5.0f; // Safe Timer just in case we somehow don't reach the target, don't block the input!

    	float autoEnterIslandCooldown = 10.0f; // 10 seconds to autoenter if user didn't interact
    	Tween islandShakeTween;

    	float VoiceHintCountdown = 20.0f;

    	// Start is called before the first frame update
    	void Start()
    	{
    		initialPosition = transform.position;
    		goToTarget = true;
    		goToTargetFailSafeTimer = 5.0f;
    		int unlockedLevel = Mathf.Min(Levels.Length - 1, ProgressManager.Instance.GetUnlockLevel(GameType));
    		targetOffset = -Levels[unlockedLevel].transform.position.x + CenterAddOffset;

    		xOffset = targetOffset;
    		for (int i = 0; i < Levels.Length; i++)
    		{
    			if (GameType == GameData.eGameType.UpperCase && ProgressManager.Instance.LastUnlockedLevelBigLetter == i ||
    				GameType == GameData.eGameType.LowerCase && ProgressManager.Instance.LastUnlockedLevelSmallLetter == i)
    			{
    				xOffset = -Levels[Mathf.Max(0, i - 1)].transform.position.x + CenterAddOffset;
    				unlockAnimIdx = i;
    				blockInput = true;
    			}
    			else 
    			{
    				Levels[i].SetEnabled(i <= unlockedLevel);

    				if (i < Levels.Length - 1 &&
    				(GameType == GameData.eGameType.UpperCase && ProgressManager.Instance.LastPlayedLevelBigLetter == i ||
    				 GameType == GameData.eGameType.LowerCase && ProgressManager.Instance.LastPlayedLevelSmallLetter == i))
    				{
    					xOffset = -Levels[i].transform.position.x + CenterAddOffset;
    					targetOffset = -Levels[i + 1].transform.position.x + CenterAddOffset;
    				}
    			}
    		}

    		// Remove the bridges lines that are locked
    		for (int i = unlockedLevel; i < Lines.Length; i++)
            {
    			Lines[i].gameObject.SetActive(false);
            }
    		for (int i = 0; i < unlockedLevel; i++)
            {
    			Lines[i].gameObject.SetActive(i == unlockAnimIdx - 1? false : true);
            }

    		transform.position = initialPosition + Vector3.right * xOffset;
    		//StartCoroutine(AnimateLines(5.0f, unlockedLevel));

    		if (GameType == GameData.eGameType.UpperCase)
            {
    			SoundManager.Instance.CrossFadeMusic("FirstPlanetBgMusic", 1.0f);
    			if (!GameData.Instance.GetFlag("UpperCaseWelcome"))
    			{
    				GameData.Instance.SetFlag("UpperCaseWelcome");
    				SoundManager.Instance.AddSFXToQueue("welcome_uppercase");
    			}
    			SoundManager.Instance.AddSFXToQueue("select_letter_island");
    		}
    		else if (GameType == GameData.eGameType.LowerCase)
            {
    			SoundManager.Instance.CrossFadeMusic("SecondPlanetBgMusic", 1.0f);
    			if (!GameData.Instance.GetFlag("LowerCaseWelcome"))
    			{
    				GameData.Instance.SetFlag("LowerCaseWelcome");
    				SoundManager.Instance.AddSFXToQueue("welcome_2");
    			}
    			SoundManager.Instance.AddSFXToQueue("select_letter_island");
    			//SoundManager.Instance.AddSFXToQueue("select_lowercase");
    		}
    	}

    	// Update is called once per frame
    	void Update()
    	{
    		if (!blockInput && Input.touchCount > 0 && !TransitionManager.Instance.InTransition)
    		{
    			float relativeMovementX = Input.GetTouch(0).deltaPosition.x;
    			relativeMovementX /= Screen.width;

    			xOffset += relativeMovementX * DragMultiplier;
    			autoEnterIslandCooldown = -1.0f; // Disable cooldown on touch

    			// Handle module selection
    			if (Input.GetTouch(0).phase == TouchPhase.Began)
    			{
    				touchPosStart = Input.GetTouch(0).position;
    				goToTarget = false;
    				scrollSpeed = 10.0f;
    				SoundManager.Instance.PlaySFX("MovingIslands");
    			}
    			else if (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled)
    			{
    				if ((touchPosStart - Input.GetTouch(0).position).magnitude < 10.0f)
    				{
    					Vector3 pos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
    					for (int i = 0; i < Levels.Length; i++)
    					{
    						if (Levels[i].Collider.OverlapPoint(pos))
    						{
    							if (Levels[i].IsEnabled)
    							{
    								SoundManager.Instance.PlaySFX("LevelDone");

    								GameData.Instance.GameType = GameType;
    								GameData.Instance.SelectedLevel = i;
								
    								TransitionManager.Instance.ShowFade(2.0f, () => SceneLoader.Instance.LoadScene(TargetScene));
    								/*
    								if (GameType == GameData.eGameType.LowerCase)
    								{
    									SoundManager.Instance.AddSFXToQueue("selected_lowercase");
    									SoundManager.Instance.AddSFXToQueue(Levels[i].name);
    								}
    								*/
    							}
    							else
    							{
    								SoundManager.Instance.PlaySFX("WrongSound");
    								if (islandShakeTween != null)
    									islandShakeTween.Kill(true);
    								islandShakeTween = Levels[i].transform.DOShakePosition(0.5f);
    							}
    						}
    					}
    				} 
    				//else
                    {
    					goToTarget = true;
    					goToTargetFailSafeTimer = 5.0f;
    					float minX = float.MaxValue;
    					int levelIdx = 0;
    					for (int i = 0; i < Levels.Length; i++)
                        {
    						LevelDefinition level = Levels[i];
    						if (Mathf.Abs(level.transform.position.x) < minX)
                            {
    							targetOffset = xOffset - level.transform.position.x + CenterAddOffset;
    							minX = Mathf.Abs(level.transform.position.x);
    							levelIdx = i;
                            }
                        }
    					float xDiff = (touchPosStart - Input.GetTouch(0).position).x;
    					if (targetOffset < xOffset && xDiff < 0f)
                        { // We actually drag in a different direction!
    						levelIdx--;
                        }
    					else if (targetOffset > xOffset && xDiff > 0f)
                        {
    						levelIdx++;
    					}

    					// Readjust
    					levelIdx = Mathf.Clamp(levelIdx, 0, Levels.Length - 1);
    					targetOffset = xOffset - Levels[levelIdx].transform.position.x + CenterAddOffset;
    				}
    			}
    		}

    		if (goToTarget)
    		{
    			goToTargetFailSafeTimer -= Time.deltaTime;
    			if (goToTargetFailSafeTimer < 0f)
    				OnTarget();

    			xOffset = Mathf.MoveTowards(xOffset, targetOffset, Mathf.Max(1.0f, Mathf.Abs(xOffset - targetOffset) * scrollSpeed) * Time.deltaTime);
    			if (xOffset == targetOffset)
    				OnTarget();
    		} 
    		else
            {
    			goToTargetFailSafeTimer = 5.0f; // 5 sec to get to target
            }

    		xOffset = Mathf.Clamp(xOffset, MinDrag, MaxDrag);
    		transform.position = initialPosition + Vector3.right * xOffset;

    		if (!blockInput)
            {
    			if (autoEnterIslandCooldown > 0)
                {
    				autoEnterIslandCooldown -= Time.deltaTime;
    				if (autoEnterIslandCooldown < 0)
                    {
    					float minX = float.MaxValue;
    					int targetLevelIdx = -1;
    					for (int i = 0; i < Levels.Length; i++)
    					{
    						float diffX = Mathf.Abs(Levels[i].transform.position.x);
    						if (diffX < minX)
    						{
    							targetLevelIdx = i;
    							minX = diffX;
    						}
    					}

    					if (targetLevelIdx != -1)
                        {
    						SoundManager.Instance.PlaySFX("LevelDone");
    						GameData.Instance.GameType = GameType;
    						GameData.Instance.SelectedLevel = targetLevelIdx;
    						TransitionManager.Instance.ShowFade(2.0f, () => SceneLoader.Instance.LoadScene(TargetScene));
    					}
    				}
                }
            }

    		if (VoiceHintCountdown > 0)
    		{
    			VoiceHintCountdown -= Time.deltaTime;
    			if (VoiceHintCountdown < 0)
    			{
    				SoundManager.Instance.AddSFXToQueue("select_letter_island");
    				VoiceHintCountdown = 20.0f;
    			}
    		}
    	}

    	void OnTarget()
    	{
    		if (unlockAnimIdx >= 0)
    		{
    			Levels[unlockAnimIdx].PlayUnlockAnim();

    			// Play line/bridge animation
    			int lineIdx = Mathf.Max(0, unlockAnimIdx - 1);
    			Vector3[] positions = new Vector3[Lines[lineIdx].positionCount];
    			Lines[lineIdx].GetPositions(positions);
    			Lines[lineIdx].positionCount = 2;
    			Lines[lineIdx].gameObject.SetActive(true);
    			StartCoroutine(AnimateLine(lineIdx, positions, 2.0f));

    			SoundManager.Instance.PlaySFX("BubblePop");
    			/*
    			if (GameType == GameData.eGameType.LowerCase)
    			{
    				SoundManager.Instance.AddSFXToQueue("now_lowercase");
    				SoundManager.Instance.AddSFXToQueue(Levels[unlockAnimIdx].name);
    			}
    			*/

    			unlockAnimIdx = -1;
    		}

    		blockInput = false;
    		scrollSpeed = 10.0f;
    	}

    	IEnumerator AnimateLines(float speed, int unlockedLevel)
        {
    		for (int i = 0; i < unlockedLevel; i++)
    		{
    			Lines[i].gameObject.SetActive(false);
    		}
    		for (int i = 0; i < unlockedLevel; i++)
            {
    			Vector3[] positions = new Vector3[Lines[i].positionCount];
    			Lines[i].GetPositions(positions);
    			Lines[i].positionCount = 2;
    			Lines[i].gameObject.SetActive(true);
    			yield return AnimateLine(i, positions, speed);
            }
        }

    	IEnumerator AnimateLine(int lineIndex, Vector3[] positions, float speed)
        {
    		Debug.Assert(positions.Length > 2, "Please make sure the lines have at least 2 points!");

    		float lineLength = 0f;
    		float[] cachedDistances = new float[positions.Length - 1];
    		for (int i = 0; i < positions.Length - 1; i++)
    		{
    			float dist = positions[i].Distance(positions[i + 1]);
    			cachedDistances[i] = dist;
    			lineLength += dist;
    		}

    		float currentPos = 0f;
    		int lastPos = 0; // optimization
    		float lastSumDistance = 0.0f; // optimization
    		while (currentPos != lineLength)
            {
    			currentPos = Mathf.MoveTowards(currentPos, lineLength, speed * Time.deltaTime);
    			int nrPoints = Lines[lineIndex].positionCount;
    			float sumDistance = lastSumDistance;
    			float interpolation = 0f;
    			for (int i = lastPos; i < cachedDistances.Length; i++)
                {
    				float tmpSumDistance = sumDistance;
    				sumDistance += cachedDistances[i];
    				if (currentPos <= sumDistance + Mathf.Epsilon)
                    {
    					lastSumDistance = tmpSumDistance;
    					lastPos = i;
    					nrPoints = i + 2;
    					interpolation = currentPos - tmpSumDistance;
    					break;
                    }
                }
    			Lines[lineIndex].positionCount = nrPoints;
    			for (int i = 0; i < nrPoints - 1; i++)
    				Lines[lineIndex].SetPosition(i, positions[i]);
    			Lines[lineIndex].SetPosition(nrPoints - 1, positions[nrPoints - 2] + (positions[nrPoints - 1] - positions[nrPoints - 2]).normalized * interpolation);
    			yield return null;
            }
        }

    #if DEVELOPMENT_BUILD
    	private void OnGUI()
        {
    		GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
    		buttonStyle.fontSize = 50;
    		if (GUI.Button(new Rect(1000, 0, 300, 150), "UnlockALL", buttonStyle))
            {
    			for (int i = 0; i < 26; i++)
                {
    				ProgressManager.Instance.UnlockLevel(GameData.eGameType.LowerCase, i);
    				ProgressManager.Instance.UnlockLevel(GameData.eGameType.UpperCase, i);
    			}
    			SceneLoader.Instance.ReloadScene();
            }
    		if (GUI.Button(new Rect(1300, 0, 300, 150), "Lock ALL", buttonStyle))
    		{
    			ProgressManager.Instance.Reset();
    			ProgressManager.Instance.Save();
    			SceneLoader.Instance.ReloadScene();
    		}
    	}
    #endif
    }


}