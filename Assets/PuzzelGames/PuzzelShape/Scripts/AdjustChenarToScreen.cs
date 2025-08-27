using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.PuzzelShape
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class AdjustChenarToScreen : MonoBehaviour
    {
        [SerializeField] RectTransform HomeButtonRef;
        Vector2 margins;

        void Start()
        {
            Rect worldRect = UtilsShape.GetWorldRect(HomeButtonRef);

            margins = worldRect.position;
            margins.y += worldRect.size.y;

            margins.x -= 50.0f;
            margins.y += 50.0f;

            Vector3 topLeft = Camera.main.ScreenToWorldPoint(margins);
            Vector3 bottomRight = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height) - margins);

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            Vector2 spriteSize;
            spriteSize.x = bottomRight.x - topLeft.x;
            spriteSize.y = topLeft.y - bottomRight.y;
            sr.size = spriteSize;
            //   ^~~~ some black magic manipulations with sprite size
        }
    }


}