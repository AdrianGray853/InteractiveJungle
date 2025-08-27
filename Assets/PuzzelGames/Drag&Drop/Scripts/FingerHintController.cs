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

    public class FingerHintController : MonoBehaviour
    {
        public bool Shared = false;

        public Animator MainAnimator;
        public SpriteRenderer FingerSprite;
        public SpriteRenderer HorizontalSprite;
        public SpriteRenderer VerticalSprite;
        public System.Action OnStartDragAnimationFinished;
        public Tween SharedTweener;
        public static FingerHintController Instance { get; private set; } // I hate to do this but sometimes you got to do what you got to do... ./. 
        private void Awake()
        {
            if (Shared)
                Instance = this; // This can be set multiple times... as there can be multiple finger hints in the scene... and... it's ok... kind of... well it's not... but... kiss my ass
        }


        private float animDragTime = 1.0f;
        private Vector3 animDragTo = Vector3.zero;
        private Vector3[] animDragPath;

        public void OnStartDragAnimationFinishedEvent()
        {
            if (OnStartDragAnimationFinished != null)
                OnStartDragAnimationFinished();
        }

        public void Clear()
        {
            Vector3 previousPos = transform.position; // We'll use this for tweeners as we don't want to jump to the end positions!

            if (OnStartDragAnimationFinished != null)
                OnStartDragAnimationFinished();
            OnStartDragAnimationFinished = null;

            if (SharedTweener != null)
                SharedTweener.Kill(true);
            SharedTweener = null;

            animDragPath = null;

            transform.position = previousPos;
        }

        public void Hide()
        {
            Clear();
            //MainAnimator.StopPlayback(); // Doesn't work
            MainAnimator.enabled = false;
            Sequence s = DOTween.Sequence().Append(FingerSprite.DOFade(0f, 0.3f));
            if (VerticalSprite.gameObject.activeSelf)
                s.Join(VerticalSprite.DOFade(0f, 0.3f));
            if (HorizontalSprite.gameObject.activeSelf)
                s.Join(HorizontalSprite.DOFade(0f, 0.3f));

            SharedTweener = s;
        }

        public void PlayAnimation(string animationName, bool forced = true)
        {
            MainAnimator.enabled = true;

            if (forced)
                MainAnimator.Play(animationName, -1, 0f);
            else
                MainAnimator.Play(animationName);
        }

        public void ShowDrag(Vector3 from, Vector3 to, float dragTime = 1.0f)
        {
            Clear();
            animDragTime = dragTime;
            animDragTo = to;
            transform.position = from;
            OnStartDragAnimationFinished += OnStartDragFinished;
            PlayAnimation("StartDrag");
        }

        public void ShowDrag(Vector3[] path, float dragTime = 1.0f)
        {
            Clear();
            animDragTime = dragTime;
            animDragPath = path;
            transform.position = path[0];
            OnStartDragAnimationFinished += OnStartDragFinished;
            PlayAnimation("StartDrag");
        }

        private void OnStartDragFinished()
        {
            OnStartDragAnimationFinished -= OnStartDragFinished;
            if (SharedTweener != null)
                SharedTweener.Kill(true);
            if (animDragPath != null)
            { // Animate path
                SharedTweener = transform.DOPath(animDragPath, animDragTime)
                .OnComplete(() =>
                {
                    PlayAnimation("EndDrag");
                });
            }
            else
            { // Animate segment
                SharedTweener = transform.DOMove(animDragTo, animDragTime)
                .OnComplete(() =>
                {
                    PlayAnimation("EndDrag");
                });
            }
        }

        public void ShowTapHint(Vector3 position)
        {
            Clear();
            transform.position = position;
            PlayAnimation("ShowTap");
        }

        public void ShowTapHint() => ShowTapHint(transform.position);

        public void ShowRightSlideHint(Vector3 position)
        {
            Clear();
            transform.position = position;
            PlayAnimation("ShowHorizontalToRight");
        }

        public void ShowRightSlideHint() => ShowRightSlideHint(transform.position);

        public void ShowLeftSlideHint(Vector3 position)
        {
            Clear();
            transform.position = position;
            PlayAnimation("ShowHorizontalToLeft");
        }

        public void ShowLeftSlideHint() => ShowLeftSlideHint(transform.position);

        public void ShowUpSlideHint(Vector3 position)
        {
            Clear();
            transform.position = position;
            PlayAnimation("ShowVerticalToUp");
        }

        public void ShowUpSlideHint() => ShowUpSlideHint(transform.position);

        public void ShowDownSlideHint(Vector3 position)
        {
            Clear();
            transform.position = position;
            PlayAnimation("ShowVerticalToDown");
        }

        public void ShowDownSlideHint() => ShowDownSlideHint(transform.position);
    }


}