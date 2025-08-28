using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using EModule = GameDataShape.eGameType;

namespace Interactive.PuzzelShape
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EModule = GameDataShape.eGameType;

    public class Jungle : MonoBehaviour
    {
        public GameObject Elements;
        public GameObject HomeButton;
        public ColoringGames ColoringModule;
        public PuzzleGames PuzzleModule;
        public FindObjectGames FindObjectModule;
        public ShapesGames ShapesModule;
        public MemoryGames MemoryModule;
        public ShapesPuzzleGames ShapesPuzzleModule;

        public GameObject[] CompleteModules; // BAD NAMING, it's actually CompleteIcons!
        public GameObject[] LockIcons;

        bool inputActive;
        bool subscriptionChecked;



        private void OnEnable()
        {
            //		ProductManagerShape.Instance.OnStoreResults += OnStoreResults;
        }

        private void OnDisable()
        {
            //	ProductManagerShape.Instance.OnStoreResults -= OnStoreResults;
        }

        private void Start()
        {
            ShowJungle();
    #if PROJECT_X
    		inputActive = true;
    		if (global::GameDataShape.Instance.GameTarget == "Coloring")
    			OnColoringButtonClick();
    		else if (global::GameDataShape.Instance.GameTarget == "Puzzle")
    			OnPuzzleButtonClick();
    		else if (global::GameDataShape.Instance.GameTarget == "Memory")
    			OnMemoryButtonClick();
    		else if (global::GameDataShape.Instance.GameTarget == "ShapesPuzzle")
    			OnShapesPuzzleButtonClick();
    		else if (global::GameDataShape.Instance.GameTarget == "Shapes")
    			OnShapesButtonClick();
    		else if (global::GameDataShape.Instance.GameTarget == "Environment")
    			OnFindObjectButtonClick();
    		inputActive = false;
    #endif
            if (SceneLoaderShape.Instance.LastScene != "Loader")
            {
                SoundManagerShape.Instance.CrossFadeMusic("JungleMusic", 2.0f);
                //            //  AnalyticsManager.Instance.ModuleEnded();
            }

            // Update copleted games
            //for (int i = 0; i < CompleteModules.Length; i++)
            //{
            //	CompleteModules[i].SetActive(ProgressManagerShape.Instance.GetGameDone((EModule)i));
            //}

            SoundManagerShape.Instance.PlaySFX("SelectSound");
            UpdateLockIcons();

            //      if (!GameUnlocked() && ProductManagerShape.Instance.State == ProductManagerShape.eState.Initialized)
            //{
            //          OnBoardingControllerShape.Instance.ShowInGame();
            //          return;
            //      }
        }

        private void Update()
        {
            //if (ProductManagerShape.Instance.State == ProductManagerShape.eState.Initiating)
            //    return;

            //if (TransitionManagerShape.Instance.InTransition)
            //    return;

            //if (ProductManagerShape.Instance.State == ProductManagerShape.eState.Initialized &&
            //    !ProductManagerShape.Instance.IsSubscribed &&
            //    !ProgressManagerShape.Instance.IsOnboardingShown())
            //{
            //    ProgressManagerShape.Instance.SetOnboardingShown(true);
            //    //OnBoardingControllerShape.Instance.ShowInGame();
            //    OnBoardingControllerShape.Instance.ShowOnBoarding();
            //}

            if (OnBoardingControllerShape.Instance.IsOnBoardingActive)
                return;

            if (!subscriptionChecked)
            {
                OnStoreResults(true); // Fake call to refresh states
                subscriptionChecked = true;
            }

            inputActive = true;
        }


        public void OnColoringButtonClick() => ActivateModule(EModule.Coloring);

        public void OnPuzzleButtonClick() => ActivateModule(EModule.Puzzle);

        public void OnFindObjectButtonClick() => ActivateModule(EModule.Environment);

        public void OnShapesButtonClick() => ActivateModule(EModule.Shapes);

        public void OnMemoryButtonClick() => ActivateModule(EModule.Memory);

        public void OnShapesPuzzleButtonClick() => ActivateModule(EModule.ShapePuzzle);


        public void OnHomeMenuButtonClick()
        {
            PlayClickSound();
    #if PROJECT_X
    			global::SceneLoaderShape.Instance.LoadShapesScene("Jungle", "Jungle");
    #else
            ShowJungle();
            // AnalyticsManager.Instance.ModuleEnded();
    #endif
        }

        public void ShowJungle()
        {
            Elements.SetActive(true);
            ColoringModule.gameObject.SetActive(false);
            PuzzleModule.gameObject.SetActive(false);
            FindObjectModule.gameObject.SetActive(false);
            ShapesModule.gameObject.SetActive(false);
            MemoryModule.gameObject.SetActive(false);
            ShapesPuzzleModule.gameObject.SetActive(false);

            HomeButton.SetActive(false);

            FluturiController.Instance.SetTargetItem(null);
        }

        void ActivateModule(EModule type)
        {
            if (!inputActive) return;
            if (!GameUnlocked(/*type*/))
            {
                OnBoardingControllerShape.Instance.ShowInGame();
                return;
            }

            PlayClickSound();
            Elements.SetActive(false);
            HomeButton.SetActive(true);

            IGameModule module = GetModuleByType(type);
            module.Enabled = true;
            module.UpdateFluturiTarget();

            //  AnalyticsManager.Instance.ModuleStarted(type);
        }

        void PlayClickSound()
        {
            SoundManagerShape.Instance.PlaySFX("MenuItemClick");
        }

        void OnStoreResults(bool success)
        {
            UpdateLockIcons();
        }

        void UpdateLockIcons()
        {
            //for (int i = 0; i < LockIcons.Length; i++)
            //{
            //	LockIcons[i].SetActive(!GameUnlocked());
            //}
        }

        bool GameUnlocked(/*EModule gameType*/)
        {
    #if PROJECT_X
    		return true;
    #else
            return true;// (/*gameType == EModule.Shapes || gameType == EModule.Environment ||*/ ProductManagerShape.Instance.IsSubscribed);
    #endif
        }

        float GetCompletionRatio(EModule type)
        {
            int total = GetModuleByType(type).LevelCount;
            int done = ProgressManagerShape.Instance.GetUnlockLevel(type);
            return (float)done / total;
        }

        IGameModule GetModuleByType(EModule type) => type switch
        {
            EModule.Coloring => ColoringModule,
            EModule.ShapePuzzle => ShapesPuzzleModule,
            EModule.Environment => FindObjectModule,
            EModule.Memory => MemoryModule,
            EModule.Shapes => ShapesModule,
            EModule.Puzzle => PuzzleModule,
            _ => throw new System.InvalidOperationException($"unexpected module type '{type}'")
        };
    }

}