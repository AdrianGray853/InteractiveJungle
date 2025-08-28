using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Interactive.PuzzelShape
{
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

    public class JungleItem : MonoBehaviour
    {
        public UnityEngine.UI.Image Image;
        public TextMeshProUGUI Text;
        public UnityEngine.UI.Button Button;

        public GameObject Lock;
        public GameObject Unlock;

        public void SetLockedState(bool Locked)
    	{
            //Lock.SetActive(Locked);
            //Unlock.SetActive(!Locked);
    	}

        public void SetUncompleted()
    	{
            Lock.SetActive(false);
            Unlock.SetActive(false);
    	}
    }


}