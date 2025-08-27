using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineSubtrackMuterTest : MonoBehaviour
{
    public PlayableDirector director;
    // Start is called before the first frame update
    void Start()
    {
        /*
        // Get the playable graph from the director
        PlayableGraph graph = director.playableGraph;

        // Find the AnimationMixerPlayable within the graph
        AnimationLayerMixerPlayable mixerPlayable = FindMixerPlayable(graph);
        mixerPlayable.SetInputWeight(0, 0f);
        Debug.Log(mixerPlayable);
        if (mixerPlayable.IsValid())
        {
            Debug.Log("Valid");
            mixerPlayable.SetInputWeight(0, 0f);
            // Access the tracks in the AnimationMixerPlayable
            int trackCount = mixerPlayable.GetInputCount();
            for (int i = 0; i < trackCount; i++)
            {
                Playable intput = mixerPlayable.GetInput(i);
                
                // Do something with the track
                Debug.Log("Track " + i + ": " + intput.GetPlayableType().ToString());
            }
        }
        */
    }

    float slide = 1.0f;

    private void OnGUI()
    {

        if (GUI.Button(new Rect(0, 300, 300, 300), "Pause"))
            director.Pause();
        if (GUI.Button(new Rect(0, 600, 300, 300), "Resume"))
            director.Resume();
        if (GUI.Button(new Rect(0, 0, 300, 300), "Mute"))
        {
            //director.Pause();
            //director.time = 0f;
            for (int i = 0; i < director.playableGraph.GetRootPlayableCount(); i++)
                Process(director.playableGraph.GetRootPlayable(i));
            director.RebindPlayableGraphOutputs();
            //director.Evaluate();
            //director.Play();
            //director.Resume();
        }

        slide = GUI.HorizontalSlider(new Rect(0, 900, 300, 100), slide, 0f, 1.0f);
        if (mixer.IsValid())
        {
            mixer.GetInput(1).GetInput(0).SetInputWeight(0, slide);
            //mixer.SetInputWeight(0, slide);
            //mixer.SetInputWeight(1, 1.0f - slide);
        }
    }

    AnimationLayerMixerPlayable mixer;

    void Process(Playable playable, int depth = 0)
    {
        if (playable.GetPlayableType() == typeof(AnimationLayerMixerPlayable))
        {
            mixer = (AnimationLayerMixerPlayable)playable;
            Debug.Log("Found Mixer!" + mixer.GetInputCount());
            Debug.Log(mixer.CanSetWeights());
            //mixer.SetInputCount(1);
            //mixer.SetInputWeight(0, 2f);

            //mixer.SetInputWeight(1, 0f);
            //mixer.GetInput(1).SetInputWeight(0, 0f);
            mixer.GetInput(1).GetInput(0).SetInputWeight(0, 0f);
            //mixer.GetInput(1).GetInput(0).GetInput(0).SetInputWeight(0, 0f);
            //mixer.DisconnectInput(1);

            /*
             // Disable automatic track binding
timelineGraph.trackBindingOptions = TrackBindingOptions.None;

// Set the input weights
yourTrack.SetInputWeight(index, weight); // Call SetInputWeight for your specific track and input

// Re-enable automatic track binding
timelineGraph.trackBindingOptions = TrackBindingOptions.Auto;
             */

            return;
        }

        Debug.Log(new string('>', depth + 1) + playable.GetPlayableType().ToString());
        for (int i = 0; i < playable.GetInputCount(); i++)
        {
            Process(playable.GetInput(i), depth + 1);
        }
    }

    /*

    private AnimationLayerMixerPlayable FindMixerPlayable(PlayableGraph graph)
    {
        // Iterate over the outputs to find the AnimationMixerPlayable
        int outputCount = graph.GetOutputCount();
        for (int i = 0; i < outputCount; i++)
        {
            PlayableOutput output = graph.GetOutput(i);
            Playable root = graph.GetRootPlayable(0);
            Playable input = root.GetInput(0); ;
            input.
            if (output.GetSourcePlayable().GetType() == typeof(AnimationLayerMixerPlayable))
            {
                return (AnimationLayerMixerPlayable)output.GetSourcePlayable();
            }
        }

        return AnimationLayerMixerPlayable.Null;
    }
    */
    // Update is called once per frame
    void Update()
    {
        
    }

}
