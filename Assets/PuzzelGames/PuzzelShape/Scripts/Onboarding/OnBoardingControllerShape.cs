using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Interactive.PuzzelShape
{
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

    public class OnBoardingControllerShape : MonoBehaviour
    {

    	public static OnBoardingControllerShape Instance { get; private set; }
    	private void Awake()
    	{
    		Instance = this;

    		PopUpInitialScale = PopUpPanel.localScale;
    		BirthdayInitialPosition = BirthdayPanel.position;
    		if (UtilsShape.IsIPad())
    			VerticalBirthdayInitialPosition = BirthdayVerticalPanel.position;
    		else
    			VerticalBirthdayInitialPosition = new Vector3(BirthdayVerticalPanel.position.y, BirthdayVerticalPanel.position.x); // Swap X/Y because of screen rotation
    		BackdropInitialAlphaInGame = InGameBackdrop.color.a;
    		VerticalBackdropInitialAlpha = VerticalBackdrop.color.a;
    		LoadingBackdropInitialAlpha = LoadingBackdrop.color.a;
    	}

    	// In Game
    	public Transform PopUpPanel;
    	public Transform BirthdayPanel;
    	public RawImage  InGameBackdrop;
    	public GameObject SplashScreens;
    	public RawImage NoInternetBackdrop;
    	public Transform NoInternetPanel;

    	// Vertical
    	public Transform AdPanel;
    	public Transform BirthdayVerticalPanel;
    	public Transform SelectAgePanel;	
    	public RawImage VerticalBackdrop;
    	public Transform LoadingPanel;
    	public RawImage LoadingBackdrop;

    	public Transform TrialPanel;

    	[HideInInspector]
    	public bool IsOnBoardingActive = false;
    	public enum ButtonType { Button1, Button2 }
    	private ButtonType buttonPressed;


    	Vector3 PopUpInitialScale;
    	Vector3 BirthdayInitialPosition;
    	Vector3 VerticalBirthdayInitialPosition;
    	float BackdropInitialAlphaInGame;
    	float VerticalBackdropInitialAlpha;
    	float LoadingBackdropInitialAlpha;
    	Tween PopupTween;
    	bool InGame = false;

    	float RotationFailSafeTimer = 0f; // Used to make sure we're not stuck forever in a rotation wait even, who knows what weird stuff might happend during that transition

    	public void AcceptAge()
    	{
    		//Good, show vertical layout page
    		if (InGame)
    		{
    			RotationFailSafeTimer = 0.5f; // Wait 5 sec max

    			TransitionManagerShape.Instance.ShowFade(Color.white, 1.0f, () =>
    			{
    				if (!UtilsShape.IsIPad())
    					Screen.orientation = ScreenOrientation.Portrait;
    				BirthdayPanel.gameObject.SetActive(false);
    				InGameBackdrop.gameObject.SetActive(false);
    				ShowStartTrial();
    			}, null, () =>
    			{
    				if (UtilsShape.IsIPad())
    					return false;
    				RotationFailSafeTimer -= Time.deltaTime;
    					return (RotationFailSafeTimer > 0);
    					//return false;
    				//return (Screen.orientation != ScreenOrientation.Portrait);
    			});
    		} 
    		else
    		{
    			Subscribe();
    		}
    	}

    	public void AcceptAgeForExternalLink()
    	{
    		//Good, show vertical layout page
    			RotationFailSafeTimer = 0.5f; // Wait 5 sec max

    			TransitionManagerShape.Instance.ShowFade(Color.white, 1.0f, () =>
    			{
    				if (!UtilsShape.IsIPad())
    					Screen.orientation = ScreenOrientation.Portrait;
    				BirthdayPanel.gameObject.SetActive(false);
    				InGameBackdrop.gameObject.SetActive(false);

    				// Dup? ce popup-ul a fost ar?tat, apel?m metoda corect?
    				if (buttonPressed == ButtonType.Button1)
    				{
    					OnPrivacyClick(); // Apel?m OnPrivacyClick dac? a fost ap?sat butonul 1
    				}
    				else if (buttonPressed == ButtonType.Button2)
    				{
    					OnTermsClick(); // Apel?m OnTermsClick dac? a fost ap?sat butonul 2
    				}

    				BirthdayVerticalPanel.gameObject.SetActive(false);
    				VerticalBackdrop.gameObject.SetActive(false);
    				//CloseTrialWindow ();
				
    			}, null, () =>
    			{
    				if (UtilsShape.IsIPad())
    					return false;
    				RotationFailSafeTimer -= Time.deltaTime;
    				return (RotationFailSafeTimer > 0);
    				//return false;
    				//return (Screen.orientation != ScreenOrientation.Portrait);
    			});
    	}
    	public void ShowInGame()
    	{
    		if (PopupTween != null)
    			PopupTween.Kill(true);

    		IsOnBoardingActive = true;
    		InGame = true;

    		SplashScreens.SetActive(true);
    		PopUpPanel.gameObject.SetActive(true);
    		PopUpPanel.localScale = Vector3.zero;
    		InGameBackdrop.gameObject.SetActive(true);
    		InGameBackdrop.SetAlpha(0f);
    		PopupTween = DOTween.Sequence()
    			.Append(PopUpPanel.DOScale(PopUpInitialScale, 0.75f).SetEase(Ease.OutBack))
    			.Join(InGameBackdrop.DOFade(BackdropInitialAlphaInGame, 0.75f));
    	}
    	public void CloseInGame()
    	{
    		if (PopupTween != null)
    			PopupTween.Kill(true);
    		PopupTween = DOTween.Sequence()
    			.Append(PopUpPanel.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack))
    			.Join(InGameBackdrop.DOFade(0f, 0.3f))
    			.OnComplete(() =>
    				{
    					PopUpPanel.gameObject.SetActive(false);
    					InGameBackdrop.gameObject.SetActive(false);
    					IsOnBoardingActive = false;
    				});
    	}

    	bool isClosing = false;
    	public void BackdropClicked()
    	{
    		if (isClosing)
    			return; // Avoid multiple clicks

    		if (PopupTween != null)
    			PopupTween.Kill(true);

    		if (PopUpPanel.gameObject.activeSelf || BirthdayPanel.gameObject.activeSelf)
    		{
    			isClosing = true;
    			Sequence s = DOTween.Sequence();
    			if (BirthdayPanel.gameObject.activeSelf)
    			{
    				s.Append(BirthdayPanel.DOMove(BirthdayInitialPosition + Vector3.down * 1000.0f, 0.75f).SetEase(Ease.InBack));
    				s.AppendCallback(() => BirthdayPanel.gameObject.SetActive(false));
    			}
    			if (NoInternetPanel.gameObject.activeSelf)
    			{
    				s.Append(NoInternetPanel.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack));
    				s.AppendCallback(() => NoInternetPanel.gameObject.SetActive(false));
    			}
    			if (NoInternetBackdrop.gameObject.activeSelf)
    			{
    				s.Join(NoInternetBackdrop.DOFade(0f, 0.5f));
    				s.AppendCallback(() => NoInternetBackdrop.gameObject.SetActive(false));
    			}
    			if (PopUpPanel.gameObject.activeSelf)
    			{
    				s.Append(PopUpPanel.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack));
    				s.AppendCallback(() => PopUpPanel.gameObject.SetActive(false));
    			}
    			if (InGameBackdrop.gameObject.activeSelf)
    			{
    				s.Join(InGameBackdrop.DOFade(0f, 0.5f));
    				s.AppendCallback(() => InGameBackdrop.gameObject.SetActive(false));
    			}
    			s.AppendCallback(() =>
    			{
    				isClosing = false;
    				IsOnBoardingActive = false;
    			});
    		}
    	}


    	bool isVerticalClosing = false;
    	public void VerticalBackdropClicked()
    	{
    		if (isVerticalClosing)
    			return;

    		if (PopupTween != null)
    			PopupTween.Kill(true);

    		isVerticalClosing = true;
    		PopupTween = DOTween.Sequence()
    			.Append(BirthdayVerticalPanel.DOMove(VerticalBirthdayInitialPosition + Vector3.down * 4000.0f, 1.0f).SetEase(Ease.OutBack))
    			.Join(VerticalBackdrop.DOFade(0f, 0.5f))
    			.OnComplete(() =>
    				{
    					BirthdayVerticalPanel.gameObject.SetActive(false);
    					VerticalBackdrop.gameObject.SetActive(false);
    					isVerticalClosing = false;
    				});
    	}

    	public void ShowBirthdayPopup()
    	{
    		if (ProductManagerShape.Instance.State == ProductManagerShape.eState.Initiating || ProductManagerShape.Instance.State == ProductManagerShape.eState.FailedToInitialize)
            {
    			ShowNoInternet();
    			return;
            }

    		if (PopupTween != null)
    			PopupTween.Kill(true);

    		BirthdayPanel.transform.position = BirthdayInitialPosition + Vector3.down * 1000.0f;
    		PopupTween = DOTween.Sequence()
    			.Append(PopUpPanel.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack))
    			.AppendCallback(() =>
    			{
    				PopUpPanel.gameObject.SetActive(false);
    				BirthdayPanel.gameObject.SetActive(true);
    			})
    			.Append(BirthdayPanel.DOMove(BirthdayInitialPosition, 1.0f).SetEase(Ease.OutBack));
    	}

    	public void ShowNoInternet()
        {
    		if (PopupTween != null)
    			PopupTween.Kill(true);

    		NoInternetPanel.gameObject.SetActive(true);
    		NoInternetPanel.localScale = Vector3.zero;
    		NoInternetBackdrop.gameObject.SetActive(true);
    		NoInternetBackdrop.SetAlpha(0f);
    		PopupTween = DOTween.Sequence()
    			.Append(NoInternetPanel.DOScale(Vector3.one * 1.5f, 0.75f).SetEase(Ease.OutBack))	
    			.Join(NoInternetBackdrop.DOFade(0.5f, 0.75f));
    	}

    	bool noInternetClosing = false;
    	public void CloseNoInternet()
        {
    		if (noInternetClosing)
    			return;

    		noInternetClosing = true;
    		if (PopupTween != null)
    			PopupTween.Kill(true);

    		PopupTween = DOTween.Sequence()
    			.Append(NoInternetPanel.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack))
    			.Join(NoInternetBackdrop.DOFade(0f, 0.5f))
    			.AppendCallback(() =>
    			{
    				NoInternetPanel.gameObject.SetActive(false);
    				NoInternetBackdrop.gameObject.SetActive(false);
    				noInternetClosing = false;
    			});
    	}

    	public void ShowOnBoarding()
    	{
    		IsOnBoardingActive = true;
    		// Make sure to disable everything from InGame! as This can be triggered during internet connection
    		if (PopupTween != null)
    			PopupTween.Kill(true);
    		InGameBackdrop.gameObject.SetActive(false);
    		PopUpPanel.gameObject.SetActive(false);
    		BirthdayPanel.gameObject.SetActive(false);
    		NoInternetPanel.gameObject.SetActive(false);
    		NoInternetBackdrop.gameObject.SetActive(false);

    		RotationFailSafeTimer = 0.5f;
    		TransitionManagerShape.Instance.ShowFade(Color.white, 1.0f, () =>
    		{
    			if (!UtilsShape.IsIPad())
    				Screen.orientation = ScreenOrientation.Portrait;
    			ShowAd();
    		}, null, () =>
    		{
    			if (UtilsShape.IsIPad())
    				return false;
    			RotationFailSafeTimer -= Time.deltaTime;
    				return (RotationFailSafeTimer > 0);
    			//return (Screen.orientation != ScreenOrientation.Portrait);
    		});
    	}

    	void ShowAd()
    	{
    		InGame = false;
    		AdPanel.gameObject.SetActive(true);
    	}
    	public void ShowVerticalBirthday()
    	{
    		if (PopupTween != null)
    			PopupTween.Kill(true);

            //if (InGame)
            //{
                Subscribe();
                return;
            //}

      //      //AdPanel.gameObject.SetActive(false);
      //      VerticalBackdrop.gameObject.SetActive(true);
    		//VerticalBackdrop.SetAlpha(0f);
    		//BirthdayVerticalPanel.gameObject.SetActive(true);
    		//BirthdayVerticalPanel.transform.position = VerticalBirthdayInitialPosition + Vector3.down * 4000.0f;
    		//PopupTween = DOTween.Sequence()
    		//	.Append(BirthdayVerticalPanel.DOMove(VerticalBirthdayInitialPosition, 1.0f).SetEase(Ease.OutBack))
    		//	.Join(VerticalBackdrop.DOFade(VerticalBackdropInitialAlpha, 0.75f));
    	}

    	public void ShowVerticalBirthdayForExternalLink()
    	{
    		if (PopupTween != null)
    			PopupTween.Kill(true);

    		//if (InGame)
    		//{
    		//	Subscribe();
    		//	return;
    		//}

    		//AdPanel.gameObject.SetActive(false);
    		VerticalBackdrop.gameObject.SetActive(true);
    		VerticalBackdrop.SetAlpha(0f);
    		BirthdayVerticalPanel.gameObject.SetActive(true);
    		BirthdayVerticalPanel.transform.position = VerticalBirthdayInitialPosition + Vector3.down * 4000.0f;
    		PopupTween = DOTween.Sequence()
    			.Append(BirthdayVerticalPanel.DOMove(VerticalBirthdayInitialPosition, 1.0f).SetEase(Ease.OutBack))
    			.Join(VerticalBackdrop.DOFade(VerticalBackdropInitialAlpha, 0.75f));
    	}
    	public void ShowAgeSelection()
    	{
    		AdPanel.gameObject.SetActive(false);
            SelectAgePanel.gameObject.SetActive(true);
    	}
    	public void SelectAgeRange(int range)
    	{
    		ProgressManagerShape.Instance.SetSelectedAgeRange(range);
    		SelectAgePanel.gameObject.SetActive(false);
    		ShowStartTrial();
    	}
    	public void ShowStartTrial()
    	{
    		TrialPanel.gameObject.SetActive(true);
    	}
    	public void Subscribe()
    	{
    		Debug.Log("Subscribe Called!");
    		if (PopupTween != null)
    			PopupTween.Kill(true);

    		Sequence s = DOTween.Sequence();
    		LoadingBackdrop.gameObject.SetActive(true);
    		LoadingBackdrop.SetAlpha(0f);
    		if (!InGame)
    		{
    			s.Append(BirthdayVerticalPanel.DOMove(VerticalBirthdayInitialPosition + Vector3.down * 4000.0f, 1.0f).SetEase(Ease.OutBack))
    			.Join(VerticalBackdrop.DOFade(0f, 0.5f))
    			.AppendCallback(() =>
    			{
    				BirthdayVerticalPanel.gameObject.SetActive(false);
    				VerticalBackdrop.gameObject.SetActive(false);
    				isVerticalClosing = false;
    			});
    		}
    		s.Insert(0f, LoadingBackdrop.DOFade(LoadingBackdropInitialAlpha, 0.3f))
    			.AppendCallback(() =>
    				{
    					LoadingPanel.gameObject.SetActive(true);
    					ProductManagerShape.Instance.BuySubscription();
    				});

    		PopupTween = s;
    	}

    	public void CloseTrialWindow()
    	{
    		RotationFailSafeTimer = 0.5f;
    		TransitionManagerShape.Instance.ShowFade(Color.white, 1.0f, () =>
    		{
    			if (!UtilsShape.IsIPad())
    				Screen.orientation = ScreenOrientation.AutoRotation;
    			VerticalBackdrop.gameObject.SetActive(false);
    			BirthdayVerticalPanel.gameObject.SetActive(false);
    			TrialPanel.gameObject.SetActive(false);
    			IsOnBoardingActive = false;
    		}, null, () =>
    		{
    			if (UtilsShape.IsIPad())
    				return false;
    			RotationFailSafeTimer -= Time.deltaTime;
    			return (RotationFailSafeTimer > 0); 
    			//return (Screen.orientation != ScreenOrientation.AutoRotation);
    		});
    	}

    	private void OnEnable()
    	{
    		ProductManagerShape.Instance.OnStoreResults += OnStoreResults;
    		ProductManagerShape.Instance.OnRestoreDone += OnRestoreDone;
    	}

    	private void OnDisable()
    	{
    		ProductManagerShape.Instance.OnStoreResults -= OnStoreResults;
    		ProductManagerShape.Instance.OnRestoreDone -= OnRestoreDone;
    	}

    	private void OnStoreResults(bool success)
    	{
    		Debug.LogError("Purchase State: " + success);
    		if (!LoadingPanel.gameObject.activeSelf && !TrialPanel.gameObject.activeSelf && Screen.orientation != ScreenOrientation.Portrait)
    			return; // Don't show transitions if there are none

    		LoadingPanel.gameObject.SetActive(false);
    		LoadingBackdrop.gameObject.SetActive(false);

    		if (success)
    		{
    			CloseTrialWindow();
    		}
    	}

    	private void OnRestoreDone()
        {
    		Debug.LogError("Restore Done!");
    		if (!LoadingPanel.gameObject.activeSelf && !TrialPanel.gameObject.activeSelf && Screen.orientation != ScreenOrientation.Portrait)
    			return; // Don't show transitions if there are none

    		LoadingPanel.gameObject.SetActive(false);
    		LoadingBackdrop.gameObject.SetActive(false);
    	}

    	public void OnRestoreClick()
    	{
    		if (PopupTween != null)
    			PopupTween.Kill(true);

    		Sequence s = DOTween.Sequence();
    		LoadingBackdrop.gameObject.SetActive(true);
    		LoadingBackdrop.SetAlpha(0f);
    		s.Insert(0f, LoadingBackdrop.DOFade(LoadingBackdropInitialAlpha, 0.3f))
    			.AppendCallback(() =>
    			{
    				LoadingPanel.gameObject.SetActive(true);
    				ProductManagerShape.Instance.RestoreSubscription();
    			});

    		PopupTween = s;
    	}

    	public void OnPrivacyClick()
    	{
    		Application.OpenURL("https://kidsapp.ai/privacy");
    	}

    	public void OnTermsClick()
    	{
    		Application.OpenURL("https://kidsapp.ai/terms");
    	}

    	public void OnPrivacyButton()
    	{
    		buttonPressed = ButtonType.Button1;
    		ShowVerticalBirthdayForExternalLink(); // Apeleaz? func?ia pentru a ar?ta popup-ul
    	}

    	public void OnTermsButton()
    	{
    		buttonPressed = ButtonType.Button2;
    		ShowVerticalBirthdayForExternalLink(); // Apeleaz? func?ia pentru a ar?ta popup-ul
    	}
    }


}