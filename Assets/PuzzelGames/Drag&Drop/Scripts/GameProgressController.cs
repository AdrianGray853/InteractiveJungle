using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class GameProgressController : MonoBehaviour
    {
        public Transform Fill;
        public Transform Spaceship;
        public float SpaceshipMinX = -10.0f;
        public float SpaceshipMaxX = 10.0f;
        public System.Action OnProgressDone;

        int maxProgressSteps = 100;
        int progressSteps = 0;
        float progress = 0f;

        float animSpeed = 0.2f;

        public static GameProgressController Instance { get; private set; }

        void OnEnable()
        {
            Instance = this;
        }

        void OnDisable()
        {
            Instance = null;
        }

        private void Start()
        {
            UpdateProgressVisuals(true);
        }

        private void Update()
        {
            UpdateProgressVisuals();
        }

        public void SetProgress(float value)
        {
            progress = value; // Override
            UpdateProgressVisuals();
        }

        public void SetMaxProgressSteps(int value)
        {
            maxProgressSteps = value;
            CalculateProgress();
        }

        public void SetProgressStep(int value)
        {
            progressSteps = value;
            CalculateProgress();
        }

        public void AddProgressSteps(int steps = 1)
        {
            progressSteps++;
            CalculateProgress();
            if (progressSteps >= maxProgressSteps && OnProgressDone != null)
                OnProgressDone();
        }

        public void CalculateProgress()
        {
            progress = (float)progressSteps / maxProgressSteps;
            UpdateProgressVisuals();
        }

        void UpdateProgressVisuals(bool snap = false)
        {
            Vector3 fillScale = Fill.transform.localScale;
            float tmpProgress = snap ? progress : Mathf.MoveTowards(fillScale.x, progress, animSpeed * Time.deltaTime);
            fillScale.x = tmpProgress;
            Fill.transform.localScale = fillScale;
            Vector3 pos = Spaceship.localPosition;
            pos.x = Mathf.Lerp(SpaceshipMinX, SpaceshipMaxX, tmpProgress);
            Spaceship.localPosition = pos;
        }
    }


}