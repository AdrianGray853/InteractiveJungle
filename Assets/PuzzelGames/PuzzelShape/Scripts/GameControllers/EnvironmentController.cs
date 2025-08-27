using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Interactive.PuzzelShape
{
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

    public class EnvironmentController : MonoBehaviour, IDropZone
    {
        public Collider2D[] ColorObjects;
        public GameObject[] OutlineObjects;
    	public Color PanelColor = Color.white;
        Vector3[] originalPositions;
    	//Vector3[] originalScale;
    	float SnapDistance = 1.5f;
    	int piecesInPlace = 0;

    	// Start is called before the first frame update
    	void Start()
        {
    		originalPositions = new Vector3[ColorObjects.Length];
    		for (int i = 0; i < ColorObjects.Length; i++)
    		{
    			originalPositions[i] = ColorObjects[i].transform.position;
    			GameManagerShape.Instance.ScrollPanelShape.AddItem(ColorObjects[i].GetComponent<SpriteRenderer>());
    		}
    		GameManagerShape.Instance.ScrollPanelShape.UpdateItemsPosition();
    		GameManagerShape.Instance.ScrollPanelShape.Panel.SetTintColor(PanelColor);
    		GameManagerShape.Instance.ScrollPanelShape.Panel.SetBaseColor(PanelColor * 0.35f);
    	}

    	public bool CanDrop(Collider2D collider)
    	//public bool CanDrop<Collider2D>(Collider2D collider)
    	{
    		Collider2D colorCollider = ColorObjects.FirstOrDefault(x => x == collider);
    		if (colorCollider != null)
    		{
    			int index = System.Array.IndexOf(ColorObjects, colorCollider);
    			if (colorCollider.transform.position.Distance(originalPositions[index]) < SnapDistance)
    			{
    				OutlineControllerShape outlineController = colorCollider.GetComponent<OutlineControllerShape>();
    				DOTween.Sequence()
    					.Append(colorCollider.transform.DOMove(originalPositions[index], 0.3f))
    					//.Join(colorCollider.transform.DOScale(originalScale[index], 0.3f))
    					.AppendCallback(() => NotifyOnTarget(colorCollider.transform, OutlineObjects[index]))
    					.Append(DOTween.ToAlpha(() => outlineController.OffsetColor, (x) => outlineController.SetOffsetColor(x), 0.3f, 0.5f).SetEase(Ease.Flash, 2.0f, 0.0f));

    				colorCollider.enabled = false;
    				piecesInPlace++;
    				return true;
    			}
    		}
    		return false;
    	}
	
    	private void NotifyOnTarget(Transform transform, GameObject Outline)
    	{
    		transform.GetComponent<SpriteRenderer>().sortingOrder = 0;
    		Outline.SetActive(false);
    		SoundManagerShape.Instance.PlaySFX("PuzzlePlace");

    		if (piecesInPlace == ColorObjects.Length)
    		{
    			//GameManagerShape.Instance.ShowNextLevel();
    			GameManagerShape.Instance.ScrollPanelShape.FadeOut();
    			SoundManagerShape.Instance.PlayCongratsVoice();
    			DOTween.Sequence()
    			.AppendInterval(2.0f)
    			.AppendCallback(() => GameManagerShape.Instance.NextLevel());
    		}
    	}
    }


}