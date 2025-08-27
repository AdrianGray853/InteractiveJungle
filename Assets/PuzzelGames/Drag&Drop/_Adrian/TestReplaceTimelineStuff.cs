using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

public class TestReplaceTimelineStuff : MonoBehaviour
{
	public PlayableDirector director;
	public GameObject ReplacementLetterPrefab;
	public GameObject ReplacementAnimalPrefab;

	// Start is called before the first frame update
	void Start()
	{
		Object track = director.playableAsset.outputs.Where(x => x.streamName == "LetterAnimTrack").First().sourceObject;
		if (track != null)
		{
			GameObject go = Instantiate(ReplacementLetterPrefab);
			Animator animator = go.transform.GetChild(0).GetComponent<Animator>();

			director.SetGenericBinding(track, animator);
		}


		track = director.playableAsset.outputs.Where(x => x.streamName == "AnimalAnimTrack").First().sourceObject;
		if (track != null)
		{
			GameObject go = Instantiate(ReplacementAnimalPrefab);
			Animator animator = go.transform.GetChild(0).GetComponent<Animator>();

			director.SetGenericBinding(track, animator);
		}
	}

	// Update is called once per frame
	void Update()
	{
		
	}
		
}
