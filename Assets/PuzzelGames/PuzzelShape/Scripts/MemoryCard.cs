using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Interactive.PuzzelShape
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

    public class MemoryCard : MonoBehaviour
    {
        public bool ShowFront;

        public Transform Front;
        public Transform Back;
        public SpriteRenderer Content;

    	public BoxCollider2D FitBox;

    	//[HideInInspector]
    	public int Tag;
    	public Collider2D Collider { get; private set; }
    	[HideInInspector]
    	public float Scale = 1.0f; //Config scale
    	[HideInInspector]
    	public bool Revealed = false; // This is only set from outside when the card is fully uncovered after selection (animation is done, it's not an intro show)

    	private float finalScale = 1.0f; // Computed scale

        void Awake()
        {
    		Front.gameObject.SetActive(ShowFront);
    		Back.gameObject.SetActive(!ShowFront);
    		Content.gameObject.SetActive(ShowFront);
    		Collider = GetComponent<Collider2D>();
    	}

    	public void SetContent(Sprite content, float scale = 1.0f)
    	{
    		Scale = scale;
    		float fitScale = UtilsShape.FitToSizeScale(content.bounds.size, FitBox.size);
    		//Debug.Log("Setting Content scale of scale = " + scale + " fitscale = " + fitScale);
    		finalScale = scale * fitScale;
    		Content.sprite = content;
    		Content.transform.localScale = Vector3.one * finalScale;
    	}

    	public Sequence AnimateState(bool showFront, float delay = 0f, bool setRevealed = false)
    	{
    		Sequence s = DOTween.Sequence();
    		if (showFront == ShowFront)
    			return s;

    		s.AppendInterval(delay);
    		if (ShowFront)
    		{ // Already visible
    			Back.localScale = new Vector3(0f, 1f, 1f);
    			s.AppendCallback(() => SoundManagerShape.Instance.PlaySFX("CardFlip", 0.3f));
    			s.Append(Front.DOScaleX(0f, 0.3f).SetEase(Ease.InSine));
    			s.Join(Content.transform.DOScaleX(0f, 0.3f).SetEase(Ease.InSine));
    			s.AppendCallback(() => {
    				Content.gameObject.SetActive(false);
    				Front.gameObject.SetActive(false);
    				Back.gameObject.SetActive(true);
    			});
    			s.Append(Back.DOScaleX(1f, 0.3f).SetEase(Ease.OutSine));
    		} 
    		else
    		{
    			Front.localScale = new Vector3(0f, 1f, 1f);
    			Vector3 tmpScale = Content.transform.localScale;
    			tmpScale.x = 0f;
    			Content.transform.localScale = tmpScale;
    			s.AppendCallback(() => SoundManagerShape.Instance.PlaySFX("CardFlip", 0.3f));
    			s.Append(Back.DOScaleX(0f, 0.3f).SetEase(Ease.InSine));
    			s.AppendCallback(() =>
    			{
    				Content.gameObject.SetActive(true);
    				Front.gameObject.SetActive(true);
    				Back.gameObject.SetActive(false);
    			});
    			s.Append(Front.DOScaleX(1f, 0.3f).SetEase(Ease.OutSine));
    			s.Join(Content.transform.DOScaleX(finalScale, 0.3f).SetEase(Ease.OutSine));
    			if (setRevealed)
    				s.AppendCallback(() => Revealed = true);
    		}

    		ShowFront = showFront;
    		return s;
    	}

    	Tween shakeAnimation;

    	public void Shake()
    	{
    		if (shakeAnimation != null)
    			shakeAnimation.Kill(true);
    		shakeAnimation = transform.DOShakeRotation(0.5f, Vector3.forward * 15.0f, 20, 90.0f, true/*, ShakeRandomnessMode.Harmonic*/);
    	}

    	/* Always with errors ....
    #if UNITY_EDITOR
    	private void OnValidate()
    	{
    		UnityEditor.EditorApplication.delayCall += () =>
    		{
    			Front.gameObject.SetActive(ShowFront);
    			Back.gameObject.SetActive(!ShowFront);
    			Content.gameObject.SetActive(ShowFront);
    		};
    	}
    #endif
    	*/
    }


}