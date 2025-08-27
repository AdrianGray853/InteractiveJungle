using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.Touch
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class OnEnableSetParticlePosition : MonoBehaviour
    {
        public ParticleSystem PSObject;

    	private void Update()
    	{
    		PSObject.transform.position = Camera.main.ScreenToWorldPoint(transform.position).SetZ(0);
    		PSObject.transform.localScale = Vector3.one * (Camera.main.orthographicSize / 5.0f);
    	}
    }


}