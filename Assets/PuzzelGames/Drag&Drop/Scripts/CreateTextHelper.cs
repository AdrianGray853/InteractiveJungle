using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class CreateTextHelper : MonoBehaviour
    {
        public TextConfig Config;
        public string Text;
        public bool Generate;
        public float SpaceSize = 2.0f;
        public float LetterSpacing = 0.5f;
        public float DashHeight = 2.0f;

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void OnValidate()
        {
    #if UNITY_EDITOR
            if (Generate)
            {
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    GenerateText();
                };
                Generate = false;
            }
    #endif
        }

        void GenerateText()
        {
            // Generate text...

            List<Transform> toDestroy = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                toDestroy.Add(transform.GetChild(i));
            }

            for (int i = 0; i < toDestroy.Count; i++)
                DestroyImmediate(toDestroy[i].gameObject);

            TextLetterDragController dragController = gameObject.GetComponent<TextLetterDragController>();
            if (dragController == null)
                dragController = gameObject.AddComponent<TextLetterDragController>();

            GameObject mainLetters = new GameObject("MainLetters");
            GameObject dragLetters = new GameObject("DragLetters");

            mainLetters.transform.parent = transform;
            dragLetters.transform.parent = transform;

            mainLetters.transform.localPosition = Vector3.zero;
            dragLetters.transform.localPosition = Vector3.zero;


            Dictionary<char, TextConfig.LetterDescription> charMapping = new Dictionary<char, TextConfig.LetterDescription>();
            for (int i = 0; i < Config.Letters.Length; i++)
            {
                charMapping.Add(Config.Letters[i].character, Config.Letters[i]);
            }

            bool initiatedBounds = false;
            float offset = 0f;

            List<LetterDefinition> makeTargets = new List<LetterDefinition>();

            Vector3 midPoint = Vector3.zero;

            Vector3 sum = Vector3.zero;
            int nr = 0;

            List<LetterDefinition> allLeters = new List<LetterDefinition>();
            List<LetterDefinition> targetLetters = new List<LetterDefinition>();

            for (int i = 0; i < Text.Length; i++)
            {
                char c = Text[i];
                if (c == ' ')
                    offset += SpaceSize;
                if (c == '#')
                    continue;

                bool isSpecial = false;
                if (i + 1 < Text.Length && Text[i + 1] == '#')
                    isSpecial = true;

                if (charMapping.ContainsKey(c))
                {
                    TextConfig.LetterDescription desc = charMapping[c];
                    GameObject go = new GameObject(c.ToString());
                    go.transform.parent = mainLetters.transform;
                    LetterDefinition lde = go.AddComponent<LetterDefinition>();
                    lde.Letter = c;
                    if (isSpecial)
                    {
                        makeTargets.Add(lde);
                    }
                    BoxCollider2D boxCollider = go.AddComponent<BoxCollider2D>();
                    boxCollider.isTrigger = true;
                    boxCollider.size = desc.sprite.bounds.size;
                    boxCollider.offset = new Vector2(0f, boxCollider.size.y * 0.5f);
                    lde.Collider = boxCollider;

                    SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = desc.sprite;
                    sr.sortingOrder = 1;
                    if (!initiatedBounds)
                    {
                        midPoint = desc.sprite.bounds.center;
                        initiatedBounds = true;
                    }
                
                    nr++;
                    go.transform.localPosition = Vector3.right * (offset + sr.bounds.size.x * 0.5f);
                    sum += go.transform.localPosition;
                    offset = go.transform.localPosition.x + sr.bounds.size.x * 0.5f + LetterSpacing;

                    if (c == '-')
                    {
                        go.transform.localPosition += Vector3.up * DashHeight;
                    }

                    allLeters.Add(lde);
                }
            }

        
            sum = sum * (1.0f / nr) + midPoint;
            for (int i = 0; i < mainLetters.transform.childCount; i++)
            {
                mainLetters.transform.GetChild(i).position -= sum;
            }

            int counter = 1;
            foreach (var letter in makeTargets)
            {
                GameObject target = Instantiate(letter.gameObject);
                target.transform.parent = dragLetters.transform;
                target.transform.position = letter.transform.position;
                target.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0.3176471f);
                target.name = letter.name + counter;
                target.gameObject.SetActive(false);
                GameObject targetPos = new GameObject(letter.name + counter + "Target");
                targetPos.transform.parent = dragLetters.transform;
                targetPos.transform.position = target.transform.position;
                letter.TargetPosition = targetPos.transform;
                counter++;
                targetLetters.Add(target.GetComponent<LetterDefinition>());
            }

            dragController.Letters = allLeters.ToArray();
            dragController.TargetLetters = targetLetters.ToArray();

            //
        }
    }


}