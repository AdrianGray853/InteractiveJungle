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

    public class TrialPanelPriceAdjustmentShape : MonoBehaviour
    {
        public TextMeshProUGUI textMesh;

        private void OnEnable()
        {
            textMesh.text = ProductManagerShape.Instance.SubscriptionText;
        }
    }


}