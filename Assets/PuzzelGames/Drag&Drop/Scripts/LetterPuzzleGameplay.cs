using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Interactive.DRagDrop
{
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

    public class LetterPuzzleGameplay : MonoBehaviour
    {
    	public Sprite[] LowercaseLetters;
    	public Sprite[] UppercaseLetters;
    	public Material SpriteOffsetMaterial;

    	public BoxCollider2D PuzzleArea;
    	public int Rows = 3;
    	public int Columns = 4;
    	public float PieceScale = 0.75f;
    	public float PieceHighlightScale = 0.6f;
    	public int MaxSpawnWaves = 10;
    	public float MaxInGameHintCooldown = 10.0f;
    	float inGameHintCooldown;

    	public RawImage Fader;
    	public GameObject PopUp;
    	public SpriteRenderer ElectricFX;

    	public Transform ProgressionMask;
    	Vector3 ProgressionInitScale;
    	public Transform RocketBooster;
    	public Animator RocketAnimator;

    	[Header("Tutorial")]
    	public RawImage TutorialBackdrop;
    	public Image TutorialFocus;

    	class PuzzlePiece
    	{
    		public SpriteRenderer Sprite;
    		public Collider2D Collider;
    		public char Letter;
    		public Vector2Int GridPosition;

    		LetterPuzzleGameplay puzzle; // Ref
    		Tween tween;
    		bool isRemoved = false;
    		bool highlighted = false;

    		public PuzzlePiece(LetterPuzzleGameplay puzzleRef)
            {
    			puzzle = puzzleRef;
            }

    		public void Appear(float appearDelay = 0f)
            {
    			if (isRemoved || highlighted)
    				return;
    			if (tween != null)
    				tween.Kill(true);
    			Sprite.transform.localScale = Vector3.zero;
    			tween = Sprite.transform.DOScale(Vector3.one * puzzle.PieceScale, 0.3f)
    				.SetEase(Ease.OutBack)
    				.SetDelay(appearDelay)
    				.OnPlay(() => SoundManager.Instance.PlaySFX("BubblePop"));
    		}

    		public void Remove(bool animate = false)
    		{
    			if (tween != null)
    				tween.Kill(true);
    			Sprite.transform.localScale = Vector3.one * puzzle.PieceScale;
    			if (animate)
    			{
    				Sprite.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete( () => Destroy(Sprite.gameObject));
    			}
    			else
    			{
    				Destroy(Sprite.gameObject);
    			}
    			isRemoved = true;
    		}

    		public void Highlight()
            {
    			Debug.Log("Highlight" + Letter);
    			if (isRemoved || highlighted)
    				return;
    			if (tween != null)
    				tween.Kill(true);
    			tween = Sprite.transform.DOScale(Vector3.one * puzzle.PieceHighlightScale, 0.2f).SetEase(Ease.InBack);
    			highlighted = true;
            }

    		public void ResetHighlight()
            {
    			Debug.Log("ResetHighlight" + Letter);
    			if (isRemoved || !highlighted)
    				return;
    			if (tween != null)
    				tween.Kill(true);
    			tween = Sprite.transform.DOScale(Vector3.one * puzzle.PieceScale, 0.2f).SetEase(Ease.OutBack);
    			highlighted = false;
    		}

    		public void Buzz()
            {
    			Debug.Log("Buzz" + Letter);
    			if (isRemoved)
    				return;
    			if (tween != null)
    				tween.Kill(true);
    			if (highlighted)
    			{ // Reset highlight first
    				tween = DOTween.Sequence()
    					.Append(Sprite.transform.DOScale(Vector3.one * puzzle.PieceScale, 0.2f).SetEase(Ease.OutBack))
    					.Append(Sprite.transform.DOShakeRotation(0.3f, Vector3.forward * 20.0f, 10, 5, true, ShakeRandomnessMode.Harmonic));
    			}
    			else
                {
    				tween = Sprite.transform.DOShakeRotation(0.3f, Vector3.forward * 20.0f, 10, 5, true, ShakeRandomnessMode.Harmonic);
    			}
    			highlighted = false;
    		}

    		public void Flash(float strength = 0.5f)
            {
    			if (isRemoved || highlighted)
    				return;
    			if (tween != null)
    				tween.Kill(true);
    			SetSpriteColorTintOffset offset = Sprite.GetComponent<SetSpriteColorTintOffset>();
    			tween = DOTween.To(offset.GetOffsetAlpha, offset.SetOffsetAlpha, strength, 2.0f).SetEase(Ease.Flash, 8.0f);
    		}
    	}

    	class Cell
    	{
    		public PuzzlePiece Piece;
    		public Vector3 Position;
    	}

    	Cell[][] Grid;

    	enum eDirection
    	{
    		UP = 0,
    		RIGHT = 1,
    		DOWN = 2,
    		LEFT = 3,

    		NUM_DIRECTIONS = 4
    	}

    	Vector2Int[] Directions = new Vector2Int[4]
    	{
    		Vector2Int.up,
    		Vector2Int.right,
    		Vector2Int.down,
    		Vector2Int.left
    	};

    	enum eGamePhase
    	{
    		ACTIVE,
    		CONNECTING,
    		FILLING,
    		DONE
    	}

    	eGamePhase gamePhase;

    	PuzzlePiece selectedPiece;
    	Vector3 lastTouchPos;
    	PuzzlePiece lastTouchPiece = null;
    	//float lastTouchTime; // Let's keep it simple for now
    	int spawnWaves = 0;
    	int fingerId = -1;

    	bool isTutorial = false;
    	bool isPaused = false;

    	const float TAP_THRESHOLD = 0.1f;
	
    	// Start is called before the first frame update
    	void Start()
    	{	
    		SoundManager.Instance.CrossFadeMusic("ThirdMiniGameBgSound", 1.0f);
    		inGameHintCooldown = MaxInGameHintCooldown;

    		if (Grid == null)
    			CreateGrid();
    		SpawnInitialPieces();
    		gamePhase = eGamePhase.ACTIVE;

    		// Init progression
    		ProgressionInitScale = ProgressionMask.localScale;
    		Vector3 scale = ProgressionMask.localScale;
    		scale.y = 0f;
    		ProgressionMask.localScale = scale;
    		RocketBooster.localScale = Vector3.zero;

    		isTutorial = !ProgressManager.Instance.IsTutorialShown(2);
    		if (isTutorial)
    		{
    			inGameHintCooldown = -1.0f;
    			StartCoroutine(TutorialCoroutine());
    		}
    		else
            {
    			SoundManager.Instance.AddSFXToQueue("new_game", 1.0f, "voiceover", 1);
    			SoundManager.Instance.AddSFXToQueue("select_upper_lower_near", 1.0f, "voiceover", 1);
    		}
    	}

    	// Update is called once per frame
    	void Update()
    	{
    		if (gamePhase != eGamePhase.ACTIVE)
    			return;

    		if (isPaused)
    			return;

    		if (Input.touchCount > 0)
    		{
    			int touchIdx = 0;

    			if (fingerId == -1)
                { // Search for a finger
    				for (int i = 0; i < Input.touchCount; i++)
                    {
    					if (Input.GetTouch(i).phase != TouchPhase.Began)
    						continue;

    					Vector3 pos = DragManager.GetWorldSpacePos(Input.GetTouch(i).position);
    					if (GetPuzzlePieceAtPosition(pos) != null)
                        {
    						touchIdx = i;
    						break;
                        }
                    }
                } 
    			else
                {
    				for (int i = 0; i < Input.touchCount; i++)
    				{
    					if (Input.GetTouch(i).fingerId == fingerId)
                        {
    						touchIdx = i;
    						break;
                        }
    				}
                }

    			Touch touch = Input.GetTouch(touchIdx);
    			Vector3 worldPos = DragManager.GetWorldSpacePos(touch.position);
    			if (touch.phase == TouchPhase.Began)
    			{
    				inGameHintCooldown = MaxInGameHintCooldown;
    				//lastTouchTime = Time.time;
    				lastTouchPos = worldPos;
    				PuzzlePiece piece = GetPuzzlePieceAtPosition(worldPos);
    				lastTouchPiece = piece;
    				if (piece != null && selectedPiece == null)
    				{
    					fingerId = touch.fingerId;
    					piece.Highlight();
    					Debug.Log("Check1" + piece.Letter);
    				}
    			}
    			else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
    			{
    				inGameHintCooldown = MaxInGameHintCooldown;
    				fingerId = -1;
    				PuzzlePiece startPiece = lastTouchPiece; //GetPuzzlePieceAtPosition(lastTouchPos);
    				PuzzlePiece endPiece = GetPuzzlePieceAtPosition(worldPos);
    				if (lastTouchPos.Distance(worldPos) < TAP_THRESHOLD)
    				{ // Tap
    					Debug.Log("Tap Check" + endPiece?.Letter);
    					if (startPiece == endPiece && startPiece != null)
    					{
    						if (selectedPiece != null)
    						{ // Connection
    							Debug.Log("Connection1" + endPiece.Letter);
    							CheckConnection(selectedPiece, endPiece);
    							DeselectPiece();
    						}
    						else
    						{
    							Debug.Log("Else1" + endPiece.Letter);
    							selectedPiece = endPiece;
    							selectedPiece.Highlight();
    						}
    					}
    					else //if (endPiece == null || startPiece == null)
    					{
    						DeselectPiece();
    					}
    				}
    				else
    				{ // Slide
    					Debug.Log("Slide Check" + endPiece?.Letter);
    					if (startPiece != null && endPiece != null && startPiece != endPiece)
    					{
    						Debug.Log("Slide1" + endPiece.Letter);
    						CheckConnection(startPiece, endPiece);
    					}
    					if (startPiece != null)
    						startPiece.ResetHighlight();
    					DeselectPiece();
    				}
    			}
    			else if (touch.phase == TouchPhase.Moved)
    			{
    				if (lastTouchPos.Distance(worldPos) > TAP_THRESHOLD)
    				{
    					PuzzlePiece startPiece = lastTouchPiece; //GetPuzzlePieceAtPosition(lastTouchPos);
    					PuzzlePiece endPiece = GetPuzzlePieceAtPosition(worldPos);
    					if (startPiece != null && endPiece != null && startPiece != endPiece && CanConnect(startPiece, endPiece))
                        {
    						fingerId = -1;
    						CheckConnection(startPiece, endPiece);
    						DeselectPiece();
    						if (startPiece != null)
    							startPiece.ResetHighlight();
    						lastTouchPiece = null;
    					}
    				}
    			}
    			/*
    			else if (touch.phase == TouchPhase.Moved)
                {
    				if (lastTouchPos.Distance(worldPos) < TAP_THRESHOLD)
    				{ DeselectPiece(); Debug.Log("Deselect2"); }
    			}
    			*/
    		} // touch count > 0

    		if (gamePhase == eGamePhase.ACTIVE && fingerId == -1 && inGameHintCooldown > 0)
            {
    			inGameHintCooldown -= Time.deltaTime;
    			if (inGameHintCooldown < 0)
                {
    				inGameHintCooldown = MaxInGameHintCooldown;
    				PuzzlePiece a, b;
    				FindPair(out a, out b);
    				if (a != null && b != null)
                    {
    					a.Flash(0.3f);
    					b.Flash(0.3f);
                    }
    			}
    		}
    	}

    	void DeselectPiece()
    	{
    		Debug.Log("DeselectPiece " + (selectedPiece == null ? " null " : selectedPiece.Letter));
    		if (selectedPiece != null)
    			selectedPiece.ResetHighlight();
    		selectedPiece = null; // Do check for anims and restore!
    	}

    	Tween ElectricFadeTween = null;

    	bool CanConnect(PuzzlePiece start, PuzzlePiece end)
        {
    		return (Mathf.Abs(start.GridPosition.x - end.GridPosition.x) == 1 && start.GridPosition.y == end.GridPosition.y ||
    			Mathf.Abs(start.GridPosition.y - end.GridPosition.y) == 1 && start.GridPosition.x == end.GridPosition.x) &&
    			IsOppositeLetter(start, end);

    	}
    	void CheckConnection(PuzzlePiece start, PuzzlePiece end)
    	{
    		if (CanConnect(start, end))
            {
    			inGameHintCooldown = MaxInGameHintCooldown;
    			gamePhase = eGamePhase.CONNECTING;
    			if (ElectricFadeTween != null)
    				ElectricFadeTween.Kill(true);
    			SoundManager.Instance.PlaySFX("Electricity");
			
    			string[] sfx = new string[] { "nice_observation", "doing_great", "genius", "so_fast" };
    			SoundManager.Instance.PlaySFX(sfx.GetRandomElement(), 1.0f, "voiceover", 1);
    			ElectricFX.color = Color.white;
    			//ElectricFX.transform.localScale = Vector3.one * 0.8f;
    			ElectricFX.transform.position = (start.Collider.transform.position + end.Collider.transform.position) * 0.5f;
    			ElectricFX.transform.rotation = Quaternion.Euler(0f, 0f, Utils.GetAngle(start.Collider.transform.position, end.Collider.transform.position));
    			ElectricFX.gameObject.SetActive(true);
    			ElectricFadeTween = DOTween.Sequence()
    				.AppendInterval(1.0f)
    				.AppendCallback(() =>
    				{
    					SoundManager.Instance.PlaySFX("MiniGame3FindAPair");
    					// The right way to remove a piece from the board!
    					GridSetPiece(start.GridPosition.x, start.GridPosition.y, null, true);
    					GridSetPiece(end.GridPosition.x, end.GridPosition.y, null, true);
    					DebugGrid();
    				})
    				.Append(ElectricFX.DOFade(0f, 0.3f))
    				.AppendCallback(() =>
    				{
    					//StartCoroutine(SettlePieces());
    					StartCoroutine(CheckAndShuffleBoard());
    				});
    		}
    		else
            {
    			SoundManager.Instance.PlaySFX("WrongSound");
    			string[] sfx = new string[] { "attention_details", "dont_worry", "decide", "try" };
    			SoundManager.Instance.PlaySFX(sfx.GetRandomElement(), 1.0f, "voiceover", 1);
    			start.Buzz();
    			end.Buzz();
            }
    	}

    	bool IsOppositeLetter(PuzzlePiece start, PuzzlePiece end)
        {
    		if (start == null)
    			return false;
    		if (end == null)
    			return false;
    		return start.Letter != end.Letter && char.ToLower(start.Letter) == char.ToLower(end.Letter);
    	}

    	IEnumerator CheckAndShuffleBoard()
        {
    		//yield return new WaitForSeconds(0.5f);
    		yield return null;
    		if (CheckForPairs())
            {
    			gamePhase = eGamePhase.ACTIVE;
    		} 
    		else
            {
    			gamePhase = eGamePhase.FILLING;
    			// Hide and Respawn Letters!
    			for (int i = 0; i < Rows; i++)
    			{
    				for (int j = 0; j < Columns; j++)
    				{
    					if (Grid[i][j].Piece != null)
    						GridSetPiece(i, j, null, true);
    				}
    			}
    			yield return new WaitForSeconds(0.5f);
    			spawnWaves++;

    			// Update progression
    			float progress = (float)Mathf.Min(MaxSpawnWaves, spawnWaves) / MaxSpawnWaves;
    			Vector3 scale = ProgressionMask.localScale;
    			scale.y = progress * ProgressionInitScale.y;
    			ProgressionMask.localScale = scale;
    			RocketBooster.localScale = Vector3.one * progress;

    			if (spawnWaves >= MaxSpawnWaves)
    			{
    				gamePhase = eGamePhase.DONE;
    				ShowWinUI();
    			}
    			else {
    				SpawnInitialPieces();
    				gamePhase = eGamePhase.ACTIVE;
    			}
    		}
        }

    	bool CheckForPairs()
        {
    		for (int i = 0; i < Rows; i++)
    		{
    			for (int j = 0; j < Columns - 1; j++)
    			{
    				// Check Horizontal
    				if (IsOppositeLetter(Grid[i][j].Piece, Grid[i][j + 1].Piece))
    					return true;
    			}
    		}
    		for (int i = 0; i < Rows - 1; i++)
    		{
    			for (int j = 0; j < Columns; j++)
    			{
    				// Check Vertical
    				if (IsOppositeLetter(Grid[i][j].Piece, Grid[i + 1][j].Piece))
    					return true;
    			}
            }

    		return false;
        }

    	void CreateGrid()
    	{
    		if (PuzzleArea == null)
    			return;

    		float cellWidth = PuzzleArea.bounds.size.x / Columns;
    		float cellHeight = PuzzleArea.bounds.size.y / Rows;
    		Vector3 startPos = PuzzleArea.bounds.min + new Vector3(cellWidth * 0.5f, cellHeight * 0.5f, 0f);

    		Grid = new Cell[Rows][];
    		for (int i = 0; i < Rows; i++)
    		{
    			Grid[i] = new Cell[Columns];
    			for (int j = 0; j < Columns; j++)
    			{
    				Grid[i][j] = new Cell();
    				Grid[i][j].Position = startPos + new Vector3(cellWidth * j, cellHeight * i, 0f);
    			}
    		}
    	}

    	PuzzlePiece GetPuzzlePieceAtPosition(Vector2 position)
    	{
    		for (int i = 0; i < Rows; i++)
    		{
    			for (int j = 0; j < Columns; j++)
    			{
    				PuzzlePiece pp = Grid[i][j].Piece;
    				if (pp != null && pp.Collider.OverlapPoint(position))
    				{
    					return pp;
    				}
    			}
    		}
    		return null;
    	}

    	private void OnDrawGizmosSelected()
    	{
    		if (Grid == null)
    			CreateGrid();
    		if (Grid == null)
    			return;
    		Gizmos.color = Color.green;

    		for (int i = 0; i < Rows; i++)
    			for (int j = 0; j < Columns; j++)
    				Gizmos.DrawCube(Grid[i][j].Position, Vector3.one * 0.1f);
    	}

    	void SpawnInitialPieces()
    	{
    		char[][] letterGrid = new char[Rows][];
    		for (int i = 0; i < Rows; i++)
    		{
    			letterGrid[i] = new char[Columns];
    			for (int j = 0; j < Columns; j++)
    			{
    				letterGrid[i][j] = GetRandomLetter();
    			}
    		}

    		// TESTING
    		/*
    		letterGrid = new char[][]
    		{
    			new char[] { 'g', 'b', 'c', 'd' },
    			new char[] { 'a', 's', 'E', 'X' },
    			new char[] { 'y', 'b', 'c', 'd' },
    		};
    		*/
    		// END TESTING


    		bool foundBadWord = true;
    		while (foundBadWord)
    		{
    			foundBadWord = CensorAndFix(letterGrid);
    		}

    		for (int i = 0; i < Rows; i++)
    		{
    			for (int j = 0; j < Columns; j++)
    			{
    				PuzzlePiece pp = CreatePiece(letterGrid[i][j], Grid[i][j].Position, PieceScale);
    				GridSetPiece(i, j, pp);
    				pp.Appear(Random.Range(0f, 0.2f));
    			}
    		}

    		// Force a pair!
    		// Get a random position and look for a valid neighbour, swap our letter with a new one
    		int rndRow = Random.Range(0, Rows);
    		int rndCol = Random.Range(0, Columns);

    		int rndDir = Random.Range(0, 4);
    		for (int i = 0; i < 4; i++)
    		{
    			Vector2Int dir = Directions[(rndDir + i) % 4];
    			PuzzlePiece pp = GetNeighbour(rndRow, rndCol, dir);
    			if (pp != null)
    			{
    				char wantedLetter = GetOtherCaseLetter(pp.Letter);
    				PuzzlePiece newPiece = CreatePiece(wantedLetter, Grid[rndRow][rndCol].Position, PieceScale);
    				GridSetPiece(rndRow, rndCol, newPiece);
    				newPiece.Appear(Random.Range(0f, 0.2f));
    				Debug.Log("Replaced " + wantedLetter + " dir " + dir + " xy " + rndRow + " " + rndCol);
    				break;
    			}
    		}

    	}

        private bool CensorAndFix(char[][] letterGrid)
        {
    		bool hasBadWord = false;
            for (int w = 0; w < BadWords.Length; w++)
            {
    			string badWord = BadWords[w];
    			// Check Rows
    			for (int i = 0; i < Rows; i++)
                {
    				for (int j = 0; j <= Columns - badWord.Length; j++)
                    {
    					bool found = true;
    					for (int k = 0; k < badWord.Length; k++)
                        {
    						if (char.ToLower(letterGrid[i][j + k]) != badWord[k])
                            {
    							found = false;
    							break;
                            }
                        }

    					if (found)
                        {
    						Debug.Log("Found bad word: " + badWord);
    						hasBadWord = true;
    						for (int k = 0; k < badWord.Length; k++)
    							letterGrid[i][j + k] = GetRandomLetter();
    					}
                    }
                }

    			// Check Columns
    			for (int i = 0; i <= Rows - badWord.Length; i++)
    			{
    				for (int j = 0; j < Columns; j++)
    				{
    					bool found = true;
    					for (int k = 0; k < badWord.Length; k++)
    					{
    						if (char.ToLower(letterGrid[i + k][j]) != badWord[k])
    						{
    							found = false;
    							break;
    						}
    					}

    					if (found)
    					{
    						Debug.Log("Found bad word: " + badWord);
    						hasBadWord = true;
    						for (int k = 0; k < badWord.Length; k++)
    							letterGrid[i + k][j] = GetRandomLetter();
    					}
    				}
    			}
    		}
    		return hasBadWord;
        }

        char GetOtherCaseLetter(char letter)
    	{
    		if (char.IsUpper(letter))
    			return char.ToLower(letter);
    		return char.ToUpper(letter);
    	}

    	PuzzlePiece GetNeighbour(int row, int col, Vector2Int direction)
    	{
    		int newRow = row + direction.x;
    		int newCol = col + direction.y;
    		if (!IsValidGridPos(newRow, newCol))
    			return null;

    		return Grid[newRow][newCol].Piece;
    	}

    	void GridSetPiece(int row, int col, PuzzlePiece piece, bool animateRemoval = false)
    	{
    		if (Grid[row][col].Piece != null)
    			Grid[row][col].Piece.Remove(animateRemoval);
    		Grid[row][col].Piece = piece;
    		if (piece != null)
    			piece.GridPosition = new Vector2Int(row, col);
    	}

    	void GridSwapPiece(int originalRow, int originalCol, int targetRow, int targetCol)
        {
    		PuzzlePiece tmp = Grid[targetRow][targetCol].Piece;
    		Grid[targetRow][targetCol].Piece = Grid[originalRow][originalCol].Piece;
    		Grid[originalRow][originalCol].Piece = tmp;
    		if (Grid[originalRow][originalCol].Piece != null)
    		{
    			Grid[originalRow][originalCol].Piece.Sprite.transform.position = Grid[originalRow][originalCol].Position;
    			Grid[originalRow][originalCol].Piece.GridPosition = new Vector2Int(originalRow, originalCol);
    		}
    		if (Grid[targetRow][targetCol].Piece != null)
    		{
    			Grid[targetRow][targetCol].Piece.Sprite.transform.position = Grid[targetRow][targetCol].Position;
    			Grid[targetRow][targetCol].Piece.GridPosition = new Vector2Int(targetRow, targetCol);
    		}
    	}

    	char GetRandomLetter()
    	{
    		bool lower = Random.value < 0.5f;
    		if (lower)
    		{
    			return (char)Random.Range('a', 'z' + 1);
    		} 
    		else
    		{
    			return (char)Random.Range('A', 'Z' + 1);
    		}
    	}

    	PuzzlePiece CreatePiece(char letter, Vector3 position, float scale)
    	{
    		Sprite sprite;
    		if (char.IsLower(letter))
    		{
    			sprite = LowercaseLetters[letter - 'a'];
    		} 
    		else
    		{
    			sprite = UppercaseLetters[letter - 'A'];
    		}
    		GameObject go = new GameObject("Piece");
    		SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
    		sr.sprite = sprite;
    		sr.sharedMaterial = SpriteOffsetMaterial;
    		go.AddComponent<SetSpriteColorTintOffset>();
    		BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
    		collider.size = sr.bounds.size;
    		go.transform.parent = transform;
    		go.transform.position = position;
    		go.transform.localScale = Vector3.one * scale;

    		PuzzlePiece pp = new PuzzlePiece(this)
    		{
    			Collider = collider,
    			Letter = letter,
    			Sprite = sr,
    			GridPosition = new Vector2Int(-1, -1)
    		};

    		return pp;
    	}

    	bool IsValidGridPos(int row, int col)
        {
    		if (row < 0)
    			return false;
    		if (col < 0)
    			return false;
    		if (row >= Rows)
    			return false;
    		if (col >= Columns)
    			return false;

    		return true;
    	}

    	// Not used!
    	IEnumerator SettlePieces()
        {
    		yield return new WaitForSeconds(0.5f);
    		gamePhase = eGamePhase.FILLING;

    		bool hasHoles = true;
    		int emptyCells = 0;
    		// Check verticals
    		while (hasHoles)
            {
    			hasHoles = false;
    			for (int i = 0; i < Rows; i++)
                {
    				for (int j = 0; j < Columns; j++)
                    {
    					if (Grid[i][j].Piece == null)
    					{
    						emptyCells++;
    						continue;
    					}

    					Vector2Int startPos = new Vector2Int(i, j);
    					Vector2Int nextPos = startPos;
    					// Check vertical
    					while (IsValidGridPos(nextPos.x - 1, nextPos.y) && Grid[nextPos.x - 1][nextPos.y].Piece == null)
                        {
    						nextPos.x--;
                        }
    					if (nextPos != startPos)
                        { // We found a vertical gap, animate!
    						PuzzlePiece piece = Grid[startPos.x][startPos.y].Piece;
    						Vector3 originalPosition = piece.Sprite.transform.position;
    						GridSwapPiece(startPos.x, startPos.y, nextPos.x, nextPos.y);
    						piece.Sprite.transform.DOMove(originalPosition, 0.3f).From();
    						hasHoles = true;
                        } 
                    }
                }

    			if (hasHoles)
    				yield return new WaitForSeconds(0.3f);
            }

    		DebugGrid();

    		bool gameDone = emptyCells == (Rows * Columns);
    		hasHoles = true;
    		// Check horisontals
    		while (hasHoles && !gameDone)
    		{
    			hasHoles = false;
    			for (int i = 0; i < Rows; i++)
    			{
    				for (int j = 0; j < Columns; j++)
    				{
    					if (Grid[i][j].Piece == null)
    						continue;

    					Vector2Int startPos = new Vector2Int(i, j);
    					Vector2Int nextPos = startPos;
    					// Check vertical
    					while (IsValidGridPos(nextPos.x, nextPos.y - 1) && Grid[nextPos.x][nextPos.y - 1].Piece == null)
    					{
    						nextPos.y--;
    					}
    					if (nextPos != startPos)
    					{ // We found a vertical gap, animate!
    						PuzzlePiece piece = Grid[startPos.x][startPos.y].Piece;
    						Vector3 originalPosition = piece.Sprite.transform.position;
    						GridSwapPiece(startPos.x, startPos.y, nextPos.x, nextPos.y);
    						piece.Sprite.transform.DOMove(originalPosition, 0.3f).From();
    						hasHoles = true;
    					}
    				}
    			}

    			if (hasHoles)
    				yield return new WaitForSeconds(0.3f);
    		}

    		if (gameDone)
    		{
    			gamePhase = eGamePhase.DONE;
    			ShowWinUI();
    		}
    		else
    		{
    			DebugGrid();
    			SpawnNewPieces();
    		}
    	}

    	// Not used!
    	void SpawnNewPieces()
        {
    		spawnWaves++;
    		int spawnPieces = (spawnWaves > MaxSpawnWaves) ? 1 : 2; // Do a progression
    		Vector2Int[] spawnPoints = new Vector2Int[] { new Vector2Int(-1, -1), new Vector2Int(-1, -1) };
    		for (int k = 0; k < spawnPieces; k++) // We know that there are maximum 2
            {
    			List<Vector2Int> SpawnPositions = new List<Vector2Int>();
    			for (int j = 0; j < Columns; j++)
                {
    				int foundAt = -1;
    				for (int i = 0; i < Rows; i++)
                    {
    					if (Grid[i][j].Piece == null && (i != spawnPoints[0].x || j != spawnPoints[0].y))
                        {
    						Debug.Log("Found at " + i + " " + j);
    						SpawnPositions.Add(new Vector2Int(i, j));
    						foundAt = i;
    						break;
                        }
                    }

    				if (foundAt == 0)
    					break; // we don't have more free spaces
                }
    			Debug.Log("---");
    			SpawnPositions.ForEach((a) => Debug.Log(a.x + " " + a.y));
    			spawnPoints[k] = SpawnPositions.GetRandomElement();
            }

    		Sequence s = DOTween.Sequence();
    		for (int i = 0; i < spawnPieces; i++)
            {
    			char letter = GetRandomLetter();
    			if (i == 0)
                { // Find a nearest letter
    				int rndDir = Random.Range(0, 4);
    				for (int r = 0; r < 4; r++)
    				{
    					Vector2Int dir = Directions[(rndDir + r) % 4];
    					PuzzlePiece piece = GetNeighbour(spawnPoints[i].x, spawnPoints[i].y, dir);
    					if (piece != null)
    					{
    						letter = GetOtherCaseLetter(piece.Letter);
    						break;
    					}
    				}
    			}

    			Debug.Log("Spawning at " + spawnPoints[i].x + " " + spawnPoints[i].y);

    			PuzzlePiece pp = CreatePiece(letter, Grid[spawnPoints[i].x][spawnPoints[i].y].Position, PieceScale);
    			GridSetPiece(spawnPoints[i].x, spawnPoints[i].y, pp);
    			s.Append(pp.Sprite.transform.DOMove(pp.Sprite.transform.position + Vector3.up * 10.0f, 0.5f).From());
    			DebugGrid();
    		}
    		s.OnComplete(() =>
    		{
    			gamePhase = eGamePhase.ACTIVE;
    		});
        }

    	public void ShowWinUI()
    	{
    #if UNITY_IOS
    		if (!ProgressManager.Instance.IsReviewShown(2))
    		{
    			Debug.Log("Asking for review!");
    			//UnityEngine.iOS.Device.RequestStoreReview();
    			ProgressManager.Instance.SetReviewShow(2);
    		}
    #endif

    		SoundManager.Instance.AddSFXToQueue("FinishMiniGame_3", 1.0f, "voiceover", 1);
    		string[] sfx = new string[] { "did_great", "nice_know_letters", "do_again" };
    		SoundManager.Instance.AddSFXToQueue(sfx.GetRandomElement(), 1.0f, "voiceover", 1);
    		RocketAnimator.SetBool("FlyAway", true);
    		DOTween.Sequence()
    			.AppendInterval(1.0f)
    			.AppendCallback(() => Fader.gameObject.SetActive(true))
    			.Append(Fader.DOFade(0.8f, 1.0f))
    			.AppendCallback(() => PopUp.SetActive(true))
    			.Append(PopUp.transform.DOScale(Vector3.zero, 0.4f).From().SetEase(Ease.OutBack));
    	}

    	public void Restart()
    	{
    		SoundManager.Instance.PlaySFX("ClickButton");
    		SceneLoader.Instance.ReloadScene();
    	}

    	public void GoBack()
    	{
    		SoundManager.Instance.PlaySFX("ClickButton");
    		TransitionManager.Instance.ShowFade(2.0f, () => SceneLoader.Instance.LoadScene("MiniGames"));
    	}

    	void DebugGrid()
        {
    #if UNITY_EDITOR || DEVELOPMENT_BUILD
    		for (int i = 0; i < Rows; i++)
            {
    			for (int j = 0; j < Columns; j++)
                {
    				if (Grid[i][j].Piece == null)
    					continue;
    				if (Grid[i][j].Piece.GridPosition.x != i || Grid[i][j].Piece.GridPosition.y != j)
                    {
    					Debug.Log("Found bad grid!");
    					Debug.Break();
    					return;
                    }
                }
            }
    #endif
    	}

    	void FindPair(out PuzzlePiece a, out PuzzlePiece b)
        {
    		a = null;
    		b = null;

    		for (int i = 0; i < Rows - 1 && a == null; i++)
    		{
    			for (int j = 0; j < Columns; j++)
    			{
    				PuzzlePiece first = Grid[i][j].Piece;
    				PuzzlePiece second = Grid[i + 1][j].Piece;
    				if (IsOppositeLetter(first, second))
    				{
    					a = first;
    					b = second;
    					break;
    				}
    			}
    		}
    		for (int i = 0; i < Rows && a == null; i++)
    		{
    			for (int j = 0; j < Columns - 1; j++)
    			{
    				PuzzlePiece first = Grid[i][j].Piece;
    				PuzzlePiece second = Grid[i][j + 1].Piece;
    				if (IsOppositeLetter(first, second))
    				{
    					a = first;
    					b = second;
    					break;
    				}
    			}
            }
        }

    	IEnumerator TutorialCoroutine()
    	{
    		yield return new WaitUntil(() => gamePhase == eGamePhase.ACTIVE);
    		yield return new WaitForSeconds(1.0f);

    		PuzzlePiece a, b;
    		int lastWave = spawnWaves;
    		RectTransform rect = TutorialFocus.GetComponent<RectTransform>();
    		Vector2 originalSize = rect.sizeDelta;
    		Vector2 rotatedSize = new Vector2(originalSize.y, originalSize.x);
    		float initialAlpha = TutorialBackdrop.color.a;

    		while (spawnWaves < lastWave + 3)
    		{
    			yield return new WaitForSeconds(1.0f);
    			FindPair(out a, out b);
    			Debug.Log("Found a " + a?.ToString());
    			if (a != null && b != null)
    			{
    				isPaused = true;
    				TutorialFocus.gameObject.SetActive(true);
    				if (a.GridPosition.x == b.GridPosition.x) // Is Horizontal?
    					rect.sizeDelta = rotatedSize;
    				else
    					rect.sizeDelta = originalSize;
    				TutorialFocus.transform.position = Camera.main.WorldToScreenPoint((a.Collider.transform.position + b.Collider.transform.position) * 0.5f);
    				a.Flash();
    				b.Flash();
    				TutorialBackdrop.gameObject.SetActive(true);
    				TutorialBackdrop.color = Color.clear;

    				SoundManager.Instance.AddSFXToQueue("select_upper_lower_near", 1.0f, "voiceover", 1);

    				Sequence focusSequence = DOTween.Sequence()
    					.Append(TutorialBackdrop.DOFade(initialAlpha, 2.0f))
    					.AppendInterval(0.5f)
    					.Append(TutorialBackdrop.DOFade(0f, 1.0f));
    				yield return new WaitUntil(() => !focusSequence.IsActive() || Input.touchCount > 0);
    				if (focusSequence.IsActive())
    					focusSequence.Kill(true);
    				isPaused = false;
    			}

    			float hintCooldown = -1.0f;
    			while (gamePhase == eGamePhase.ACTIVE)
    			{
    				hintCooldown -= Time.deltaTime;
    				if (hintCooldown < 0)
    				{
    					FindPair(out a, out b);
    					if (a != null && b != null)
    					{
    						FingerHintController.Instance.ShowDrag(a.Collider.transform.position, b.Collider.transform.position, 1.0f);
    						//SoundManager.Instance.AddSFXToQueue("select_upper_lower_near", 1.0f, "voiceover", 1);
    					}
    					hintCooldown = 5.0f;
    				}
    				//yield return new WaitForSeconds(2.0f);
    				yield return null;
    			}
    			Debug.Log("One Pair found!");
    			FingerHintController.Instance.Hide();
    			yield return new WaitUntil(() => gamePhase == eGamePhase.ACTIVE);
    		}


    		isTutorial = false;
    		ProgressManager.Instance.SetTutorialShown(2);
    		TransitionManager.Instance.ShowFade(2.0f, () => Restart());

    	}

    	string[] BadWords = new string[]
    	{
    		"gay",
    		"fuck",
    		"cock",
    		"suck",
    		"dick",
    		"anal",
    		"sex",
    		"butt",
    		"vagi",
    		"rim",
    		"perv",
    		"homo",
    		"cum",
    		"whor",
    		"cunt",
    		"ass",
    		"nigr",
    		"nige",
    		"jerk",
    		"jew",
    		"nut",
    		"twit",
    		"fap",
    		"yid",
    		"yob",
    		"wimp",
    		"hick",
    		"dork",
    		"pee",
    		"shit",
    		"poo",
    		"god",
    		"fag",
    		"arse",
    		"hell",
    		"damn",
    		"crap",
    		"niga",
    		"piss",
    		"slut",
    		"turd",
    		"cack",
    		"gash",
    		"jizz",
    		"mad",
    		"prat",
    		"tart",
    		"tits",
    		"porn"
    	};
    }


}