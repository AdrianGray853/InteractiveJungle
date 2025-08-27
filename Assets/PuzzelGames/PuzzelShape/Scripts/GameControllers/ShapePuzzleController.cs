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

    public class ShapePuzzleController : MonoBehaviour, IDropZone
    {
        [System.Serializable]
        public class PuzzleGroup
    	{
            public Collider2D[] Pieces;
            [HideInInspector]
            public List<Vector3> CachedPositions;

            public void Init()
    		{
                CachedPositions = new List<Vector3>(Pieces.Length);
                CachedPositions.AddRange(Pieces.Select(x => x.transform.position));
    		}
            public bool HasPiece(Collider2D piece)
    		{
                return Pieces.Contains(piece);
    		}
            public bool CheckAndTryGetNewPosition(Collider2D piece, out Vector3 position, float snapPosition)
    		{
                int index = System.Array.IndexOf(Pieces, piece);
                if (index >= 0)
    			{
                    for (int i = 0; i < CachedPositions.Count; i++)
                    {
                        if (CachedPositions[i].Distance(piece.transform.position) < snapPosition)
    					{
                            position = CachedPositions[i];
                            CachedPositions.RemoveAt(i);
                            return true;
    					}
                    }
    			}

                position = Vector3.zero;
                return false;
            }
    	}

        public Collider2D[] PuzzlePieces;
        public PuzzleGroup[] Groups;
        public GameObject Target;
        public string StartingSound = "";
        float SnapDistance = 1.5f;
        Vector3[] originalPositions;
        int piecesInPlace = 0;

        // Start is called before the first frame update
        void Start()
        {
    		foreach (var group in Groups)
    		{
    			group.Init();
    		}

    		originalPositions = new Vector3[PuzzlePieces.Length];

            for (int i = 0; i < PuzzlePieces.Length; i++)
            {
                originalPositions[i] = PuzzlePieces[i].transform.position;
                GameManagerShape.Instance.ScrollPanelShape.AddItem(PuzzlePieces[i].GetComponent<SpriteRenderer>());
    		}
    		GameManagerShape.Instance.ScrollPanelShape.UpdateItemsPosition();

    		if (StartingSound != "")
    		{
    			SoundManagerShape.Instance.PlaySFX(StartingSound);
    		}
    	}
  
        private void NotifyOnTarget(Transform transform)
        {
            transform.GetComponent<SpriteRenderer>().sortingOrder = 0;
            SoundManagerShape.Instance.PlaySFX("PuzzlePlace");

            if (piecesInPlace == PuzzlePieces.Length)
            {
                SoundManagerShape.Instance.PlaySFX("PuzzleDone");
                SoundManagerShape.Instance.PlayCongratsVoice();
                Target.SetActive(false);
                Sequence s = DOTween.Sequence().AppendInterval(0.6f);
                for (int i = 0; i < PuzzlePieces.Length; i++)
    			{
                    OutlineControllerShape outlineController = PuzzlePieces[i].GetComponent<OutlineControllerShape>();
                    if (i == 0)
                        s.Append(DOTween.ToAlpha(() => outlineController.OffsetColor, (x) => outlineController.SetOffsetColor(x), 0.3f, 1.0f).SetEase(Ease.Flash, 4.0f, 0.0f));
                    else
                        s.Join(DOTween.ToAlpha(() => outlineController.OffsetColor, (x) => outlineController.SetOffsetColor(x), 0.3f, 1.0f).SetEase(Ease.Flash, 4.0f, 0.0f));
                }
                //s.AppendCallback(() => GameManagerShape.Instance.ShowNextLevel());
                s.AppendInterval(1.5f);
                s.AppendCallback(() => GameManagerShape.Instance.NextLevel());
            }
        }

    	public bool CanDrop(Collider2D collider)
    	{
    		Collider2D puzzleCollider = PuzzlePieces.FirstOrDefault(x => x == collider);
    		if (puzzleCollider != null)
    		{
    			foreach (var group in Groups)
    			{
                    if (group.HasPiece(puzzleCollider))
                    {
                        Vector3 newPosition;
                        if (group.CheckAndTryGetNewPosition(puzzleCollider, out newPosition, SnapDistance))
                        {
                            MoveToPosition(puzzleCollider, newPosition);
                            return true;
                        }
                        return false; // Ignore the rest, as the groups have their own position system (if a piece is part of a group originalPosition is irrelevant and bad to use!)
                    }
    			}

    			int index = System.Array.IndexOf(PuzzlePieces, puzzleCollider);
    			if (puzzleCollider.transform.position.Distance(originalPositions[index]) < SnapDistance)
    			{
                    MoveToPosition(puzzleCollider, originalPositions[index]);
                    return true;
    			} 
    		}
            return false;
    	}

        private void MoveToPosition(Collider2D piece, Vector3 position)
    	{
    		OutlineControllerShape outlineController = piece.GetComponent<OutlineControllerShape>();
    		DOTween.Sequence()
    			.Append(piece.transform.DOMove(position, 0.3f))
    			.AppendCallback(() => NotifyOnTarget(piece.transform))
    			.Append(DOTween.ToAlpha(() => outlineController.OffsetColor, (x) => outlineController.SetOffsetColor(x), 0.3f, 0.5f).SetEase(Ease.Flash, 2.0f, 0.0f));

            piece.enabled = false;
    		piecesInPlace++;
    	}

    	public Color? GetPanelColor()
    	{
            return null;
    	}
    }


}