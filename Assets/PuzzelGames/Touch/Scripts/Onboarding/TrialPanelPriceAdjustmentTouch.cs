using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Interactive.Touch
{
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

    public class TrialPanelPriceAdjustmentTouch : MonoBehaviour
    {
        public TextMeshProUGUI textMesh;

        private void Start()
        {
            textMesh.text = ProductManagerTouch.Instance.SubscriptionText;
        }
    }


}