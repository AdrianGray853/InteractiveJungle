using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    [CreateAssetMenu(fileName = "SpaceshipsConfiguration", menuName = "ScriptableObjects/SpaceshipsConfiguration", order = 1)]
    public class SpaceshipsConfiguration : ScriptableObject
    {
    	public GameObject[] Spaceships;
    	public GameObject[] Targets;
    	public float[] RotationOffsets;
    }


}