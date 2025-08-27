using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

namespace Interactive.PuzzelShape
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

    public class MemoryGameSetup : MonoBehaviour
    {
        public enum eCategory
        {
            Animale = 0,
            Mancare = 1,
            Obiecte = 2,
            Forme = 3,
            Random = 4
        }

        public Transform MainCardPosition;
        public Transform[] MemoryCardPositions;
        public eCategory Category;
        [Range(0, 6)]
        public int MinCardDifficulty;
    	[Range(0, 6)]
    	public int MaxCardDifficulty;
        public float MemorizeTime = 3.0f;
        public int DuplicatesAllowed = 0;

        private MemoryCard mainCard;
        private List<MemoryCard> memoryCards = new List<MemoryCard>();

        public Vector2 TargetResolution = new Vector2(2778.0f, 1284.0f); // IPhone 13 Pro Max

        enum eState
    	{
            Init,
            ShowAllMemory,
            HideMemory,
            ShowMain,
            GuessMemory,
            ChangeMain,
            Done
    	}
        eState state = eState.Init;
        float stateChangeTimer = 0f;

        // TODO: Make it so it can be random card or can use same card, don't forget about tags after spawning, then use touch and anims...

        // Start is called before the first frame update
        void Start()
        {
            // Scale transform based on resolution
            float targetRatio = TargetResolution.x / TargetResolution.y;
            float currentRatio = (float)Screen.width / Screen.height;
            transform.localScale *= currentRatio / targetRatio;

            /*
            List<GameManagerShape.CardCollection> cardCollection = GameManagerShape.Instance.Cards
                .Where(x => x.Category == Category)
                .ToList();
        
            List<GameManagerShape.CardDesc> CardPool = new List<GameManagerShape.CardDesc>();
            foreach (var collection in cardCollection)
            {
               CardPool.AddRange(collection.Cards.Where(x => x.Difficulty >= MinCardDifficulty && x.Difficulty <= MaxCardDifficulty).ToList());
            }
            */
            List<GameManagerShape.CardDesc> CardPool = GameManagerShape.Instance.Cards
                .Where(x => x.Category == Category)
                .SelectMany(x => x.Cards)
                .Where(x => x.Difficulty >= MinCardDifficulty && x.Difficulty <= MaxCardDifficulty)
                .ToList();
            List<int> Duplicates = new List<int>(new int[CardPool.Count]);
            List<int> Indices = Enumerable.Range(0, CardPool.Count).ToList();

            state = eState.ShowAllMemory;
            stateChangeTimer = MemorizeTime + MemoryCardPositions.Length * 0.2f; // Let the time be set in config!

            // Spawn cards and make sure they satisfy the duplicate condition
            GameManagerShape.CardDesc desc;
            int idx;
            for (int i = 0; i < MemoryCardPositions.Length; i++)
    		{
                MemoryCard card = SpawnCard(MemoryCardPositions[i]);
    			card.ShowFront = false;
                idx = Random.Range(0, CardPool.Count);
                desc = CardPool[idx];
                card.SetContent(desc.CardSprite, desc.Scale);
                card.AnimateState(true, 0.2f + i * 0.2f);
                card.Tag = Indices[idx];
                memoryCards.Add(card);

                Duplicates[idx]++;
                if (Duplicates[idx] > DuplicatesAllowed)
    			{
                    CardPool.RemoveAt(idx);
                    Duplicates.RemoveAt(idx);
                    Indices.RemoveAt(idx);

                }
    		}

            // Span main card
    		mainCard = SpawnCard(MainCardPosition);
    		mainCard.ShowFront = false;
            idx = Random.Range(0, memoryCards.Count);
    		mainCard.SetContent(memoryCards[idx].Content.sprite, memoryCards[idx].Scale);
    		//mainCard.AnimateState(true);
    		mainCard.Tag = memoryCards[idx].Tag;
    	}

    	// Update is called once per frame
    	void Update()
        {
            if (stateChangeTimer > 0)
    		{
                stateChangeTimer -= Time.deltaTime;
                if (stateChangeTimer < 0)
    			{
                    CheckState();
    			}
    		}

            if (state == eState.GuessMemory)
            {
                var inputs = DragManagerShape.Instance.GetNewInputs();
                if (inputs.Count > 0)
                {
                    for (int i = 0; i < memoryCards.Count; i++)
                    {
                        if (memoryCards[i].ShowFront)
                            continue; // Skip frontal ones

                        for (int j = 0; j < inputs.Count; j++)
                        {
                            if (memoryCards[i].Collider.OverlapPoint(DragManagerShape.GetWorldSpacePos(inputs[j].position)))
                            {
                                if (memoryCards[i].Tag == mainCard.Tag)
                                {
                                    memoryCards[i].AnimateState(true, 0f, true).AppendCallback(CheckCards);
                                }
                                else
                                {
                                    SoundManagerShape.Instance.PlaySFX("Denied");
                                    memoryCards[i].Shake();
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        Sequence flipSequence;

        private void CheckCards()
    	{
            if (state == eState.Done)
                return;

            var leftCards = memoryCards.Where(x => !x.Revealed).ToList();
            if (leftCards.Count == 0)
    		{ // Done
                state = eState.Done;

                if (GameManagerShape.Instance.CurrentLevelIdx == 0)
    			{
                    SoundManagerShape.Instance.PlaySFX("YouCanPlayWithThemToo");
                }

                GameManagerShape.Instance.ShowNextLevel();

                // For Fun!?
                for (int i = 0; i < memoryCards.Count; i++)
    			{
                    Rigidbody2D rbd = memoryCards[i].gameObject.AddComponent<Rigidbody2D>();
                    rbd.linearVelocity = new Vector2(Random.Range(-2.0f, 2.0f), 3.0f);
                    rbd.angularVelocity = Random.Range(-360.0f, 360.0f);
                    rbd.gravityScale = 0f;
                    Debug.Log(rbd.transform.lossyScale);
    			}
    		} 
            else
    		{
                int leftCount = leftCards.Count(x => x.Tag == mainCard.Tag);
                if (leftCount == 0)
    			{ // no more similar cards left, change main card
                    state = eState.ChangeMain;
                    if (flipSequence != null)
                        flipSequence.Kill(true);
                    float duration = mainCard.AnimateState(false, 0.5f).Duration();
                    flipSequence = DOTween.Sequence();
                    flipSequence.AppendInterval(duration + 0.01f); // Add a buffer just to avoid any overlap...
                    flipSequence.AppendCallback(() =>
                    {
                        MemoryCard rndCard = leftCards[Random.Range(0, leftCards.Count)];
                        mainCard.SetContent(rndCard.Content.sprite, rndCard.Scale);
                        mainCard.Tag = rndCard.Tag;
                        mainCard.AnimateState(true);
                    });
                
                    stateChangeTimer = flipSequence.Duration(); // last duration is for returning the card back
                }
    		}
        }

        private void CheckState()
    	{
            if (state == eState.ShowAllMemory)
    		{
                for (int i = 0; i < memoryCards.Count; i++)
    			{
                    memoryCards[i].AnimateState(false);
    			}
                mainCard.AnimateState(true, 0.5f);
                state = eState.HideMemory;
                stateChangeTimer = 0.5f;
            } 
            else if (state == eState.HideMemory || state == eState.ChangeMain)
    		{
                state = eState.GuessMemory; // not timed
    		}
    	}

        private MemoryCard SpawnCard(Transform transform)
    	{
            GameObject go = Instantiate(GameManagerShape.Instance.CardPrefab, transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.GetComponent<BoxCollider2D>().edgeRadius *= go.transform.lossyScale.x;
            return go.GetComponent<MemoryCard>();
    	}

    	// --------------------- This is for FUN! ----------------------
    	private void TouchCallback(Touch touch)
    	{
            if (state != eState.Done)
                return;

    		Vector3 worldPos = DragManagerShape.GetWorldSpacePos(touch.position);
    		for (int i = 0; i < memoryCards.Count; i++)
    		{
    			if (DragManagerShape.Instance.HasDrag(memoryCards[i].transform))
    				continue;
    			if (memoryCards[i].Collider.OverlapPoint(worldPos))
    			{
    				DragManagerShape.Instance.AddDrag(touch.fingerId, memoryCards[i].transform, worldPos, Vector3.zero, true);
    				break;
    			}
    		}
    	}

    	private void OnEnable()
    	{
    		DragManagerShape.Instance.AddOnTouchCallback(TouchCallback);
    	}

    	private void OnDisable()
    	{
    		if (DragManagerShape.Instance == null)
    			return;
    		DragManagerShape.Instance.RemoveOnTouchCallback(TouchCallback);
    	}

    	private void FixedUpdate()
    	{
    		if (state != eState.Done)
    			return;

    		for (int i = 0; i < memoryCards.Count; i++)
    		{
    			Rigidbody2D rbd = memoryCards[i].gameObject.GetComponent<Rigidbody2D>();
                rbd.AddForce(Input.acceleration.ToVector2() * 50.0f);
    		}
    	}
    }


}