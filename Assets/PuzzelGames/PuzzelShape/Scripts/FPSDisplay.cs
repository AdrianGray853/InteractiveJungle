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

    public class FPSDisplay : MonoBehaviour
    {
        public TextMeshProUGUI Text;

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            Text.text = string.Format("FPS: {0:0.00}", 1.0f / Time.deltaTime);
        }
    }


}