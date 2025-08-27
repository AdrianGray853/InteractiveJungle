using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.Touch
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class StickerPageDesc : MonoBehaviour
    {
        [System.Serializable]
        public class StickerDesc
    	{
            public SpriteRenderer Locked;
            public SpriteRenderer Unlocked;
            [HideInInspector]
            public int StickerIdx;
    	}

        public StickerDesc[] StickerTargets;
    }


}