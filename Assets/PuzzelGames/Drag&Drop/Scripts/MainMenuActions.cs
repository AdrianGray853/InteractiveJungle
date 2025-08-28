using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

    public class JungleActions : MonoBehaviour
    {
    	public Camera mainCamera;
    	public UpCasePlanetController BigLetterPlanet;
    	public LoCasePlanetController SmallLetterPlanet;
    	public MixCasePlanetController MixedLetterPlanet;
    	public MiniGameCasePlanetController MinigamesPlanet;

    	public GameObject[] LockedGraphics;

    	public FingerHintController[] FingerHints;
    	public float HintMaxCountdown = 5.0f;
    	float hintCountdown = 5.0f;
    	int hintCounter = 0;

    	bool inputActive = false;
    	bool welcomeSoundPlayed = false;

    	public void BigLettersClick()
    	{
    		if (!inputActive)
    			return;

    		if (!ProductManager.Instance.IsSubscribed)
            {
    			OnBoardingController.Instance.ShowInGame(0);
    			return;
            }

    		SoundManager.Instance.PlaySFX("EnterAPlanetSound", 0.75f);
    		BigLetterPlanet.HideLetters();
    		AnimateCameraTo(BigLetterPlanet.transform.position, "UpperCaseMap");
    	}

    	public void SmallLettersClick()
    	{
    		if (!inputActive)
    			return;

    		if (!ProductManager.Instance.IsSubscribed)
    		{
    			OnBoardingController.Instance.ShowInGame(1);
    			return;
    		}

    		SoundManager.Instance.PlaySFX("EnterAPlanetSound", 0.75f);
    		SmallLetterPlanet.HideLetters();
    		AnimateCameraTo(SmallLetterPlanet.transform.position, "LowerCaseMap");
    	}

    	public void MixedLettersClick()
    	{
    		if (!inputActive)
    			return;

    		SoundManager.Instance.PlaySFX("EnterAPlanetSound", 0.75f);
    		MixedLetterPlanet.HideLetters();
    		AnimateCameraTo(MixedLetterPlanet.transform.position, "CombinationMap");
    		//SoundManager.Instance.PlaySFX("alright");
    	}

    	public void MinigamesClick()
    	{
    		if (!inputActive)
    			return;

    		if (!ProductManager.Instance.IsSubscribed)
    		{
    			OnBoardingController.Instance.ShowInGame(2);
    			return;
    		}

    		SoundManager.Instance.PlaySFX("EnterAPlanetSound", 0.75f);
    		MinigamesPlanet.ShowSelectedAnimation();
    		AnimateCameraTo(MinigamesPlanet.transform.position, "MiniGames");
    	}

    	void AnimateCameraTo(Vector3 targetPosition, string targetScene)
    	{
    		hintCountdown = -1.0f;
    		foreach (var hint in FingerHints)
            {
    			//hint.StopPlayback(); // Doesn't work
    			hint.MainAnimator.enabled = false;
    			hint.FingerSprite.DOFade(0f, 0.3f);
            }

    		//TransitionManager.Instance.SetFadeColor(Color.black);
    		DOTween.Sequence()
    			.Append(mainCamera.DOOrthoSize(2.5f, 1.0f).SetEase(Ease.InOutSine))
    			.Join(mainCamera.transform.DOMove(targetPosition.SetZ(mainCamera.transform.position.z), 1.0f).SetEase(Ease.InOutSine))
    			.AppendInterval(1.0f)
    			.AppendCallback(() => TransitionManager.Instance.ShowFade(2.0f, () => ChangeScene(targetScene)));
    	}

    	void ChangeScene(string targetScene)
        {
    		SceneLoader.Instance.LoadScene(targetScene, true);
        }

        private void Update()
        {
    		if (ProductManager.Instance.State == ProductManager.eState.Initiating)
    			return;

    		foreach (var lockGraphic in LockedGraphics)
    		{
    			if (lockGraphic.activeSelf == ProductManager.Instance.IsSubscribed) // Checking as SetActive might do more stuff behind the scenes...
    				lockGraphic.SetActive(!ProductManager.Instance.IsSubscribed);
    		}

    		if (TransitionManager.Instance.InTransition)
    			return;

    		inputActive = true;

    		if (ProductManager.Instance.State == ProductManager.eState.Initialized && !ProductManager.Instance.IsSubscribed && !ProgressManager.Instance.IsOnboardingShown()) // TODO: Check the save file for only one onboarding show!
    		{
    			ProgressManager.Instance.SetOnboardingShown(true);
    			//OnBoardingController.Instance.ShowInGame();
    			OnBoardingController.Instance.ShowOnBoarding();
    		}

    		if (OnBoardingController.Instance.IsOnBoardingActive)
    			return; // Don't do anything bello from here, like hints etc...

    		if (!welcomeSoundPlayed && !GameData.Instance.GetFlag("JungleWelcomePlayed"))
    		{
    			//Sequence s = DOTween.Sequence();
    			//s.AppendInterval(2.5f);
    			//s.AppendCallback(() => SoundManager.Instance.PlaySFX("welcome"));
    			SoundManager.Instance.PlaySFX("welcome");
    			GameData.Instance.SetFlag("JungleWelcomePlayed", true);
    			welcomeSoundPlayed = true;
    		}

    		if (hintCountdown > 0)
            {
    			hintCountdown -= Time.deltaTime;
    			if (hintCountdown < 0)
                {
    				FingerHintController hint = FingerHints.GetRandomElement();
    				hint.MainAnimator.Play("ShowTap", -1, 0f); 
    				hintCountdown = HintMaxCountdown;

    				if (hintCounter % 3 == 0)
    					SoundManager.Instance.PlaySFX("select");

    				hintCounter++;
    				/*
    				Sequence s = DOTween.Sequence();
    				s.AppendInterval(1.5f);
    				s.AppendCallback(() => SoundManager.Instance.PlaySFX("select"));
    				*/
    			}
    		}
    	}

        private void Start()
        {
            if (SoundManager.Instance.GetCurrentMusicName() != "JungleSong")
            {
    			SoundManager.Instance.CrossFadeMusic("JungleSong", 1.0f);
            }

    		hintCountdown = HintMaxCountdown;
    	}

    #if DEVELOPMENT_BUILD
        private void OnGUI()
        {
    		GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
    		buttonStyle.fontSize = 50;
    		if (GUI.Button(new Rect(700, 0, 300, 150), "Fake Purchase", buttonStyle))
    		{
    			ProductManager.Instance.FakePurchase();
    			Screen.orientation = ScreenOrientation.AutoRotation;

    			OnBoardingController.Instance.PopUpPanel.gameObject.SetActive(false);
    			OnBoardingController.Instance.BirthdayPanel.gameObject.SetActive(false);
    			OnBoardingController.Instance.InGameBackdrop.gameObject.SetActive(false);
    			OnBoardingController.Instance.AdPanel.gameObject.SetActive(false);
    			OnBoardingController.Instance.BirthdayVerticalPanel.gameObject.SetActive(false);
    			OnBoardingController.Instance.SelectLetterPanel.gameObject.SetActive(false);
    			OnBoardingController.Instance.SelectAgePanel.gameObject.SetActive(false);
    			OnBoardingController.Instance.FakeBuildLoadingPanel.gameObject.SetActive(false);
    			OnBoardingController.Instance.VerticalBackdrop.gameObject.SetActive(false);
    			OnBoardingController.Instance.LoadingPanel.gameObject.SetActive(false);
    			OnBoardingController.Instance.LoadingBackdrop.gameObject.SetActive(false);
    			OnBoardingController.Instance.TrialPanel.gameObject.SetActive(false);
    		}
    		if (GUI.Button(new Rect(1000, 0, 300, 150), "Restore", buttonStyle))
    		{
    			ProductManager.Instance.RestoreSubscription();
    		}
    		if (GUI.Button(new Rect(1300, 0, 300, 150), "Refresh", buttonStyle))
    		{
    			ProductManager.Instance.RefreshReceipt();
    		}
    	}	
    #endif
    }


}