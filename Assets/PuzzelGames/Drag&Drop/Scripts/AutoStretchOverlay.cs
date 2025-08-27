using UnityEngine;

namespace Interactive.DRagDrop
{
using UnityEngine;

    [ExecuteInEditMode]
    public class AutoStretchOverlay : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;
        public float ScaleX = 0.96f, ScaleY = 0.9f;
        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>(); 
        }

        // Update is called once per frame
        void Update()
        {
            AutoScale();
        }

        private void AutoScale()
        {
            /*
            float spriteWidth = spriteRenderer.bounds.size.x;
            float spriteHeight = spriteRenderer.bounds.size.y;

            float targetWidth = spriteHeight * (4f / 3f);
            float targetHeight = spriteWidth * (3f / 4f);

            float scaleX = spriteWidth / targetWidth;
            float scaleY = spriteHeight / targetHeight;

            float scale = Mathf.Min(scaleX, scaleY);

            transform.localScale = new Vector3(scale, scale, transform.localScale.z);*/
            spriteRenderer = GetComponent<SpriteRenderer>();
            Vector3 bottomLeft = Camera.main.ScreenToWorldPoint(Vector3.zero);
            Vector3 topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));
            float diffX = topRight.x - bottomLeft.x;
            float diffY = topRight.y - bottomLeft.y;
            //Debug.Log(diffX + " " + diffY);

            Vector2 size = spriteRenderer.size;
            size.x = diffX * ScaleX;
            size.y = diffY * ScaleY;
            spriteRenderer.size = size;
        }

   
    }


}