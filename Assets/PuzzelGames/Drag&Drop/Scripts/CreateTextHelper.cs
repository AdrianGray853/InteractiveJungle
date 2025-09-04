using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive.DRagDrop
{
    public class CreateTextHelper : MonoBehaviour
    {
        public TextConfig Config;
        public string Text;
        public bool Generate;
        public float SpaceSize = 2.0f;
        public float LetterSpacing = 0.5f;
        public float DashHeight = 2.0f;

        [Header("Target Placement Rules")]
        public Transform character;         // player / character ref
        public float minCharacterDistance = 3f;
        public float safeDistance = 2f;
        public int maxAttempts = 20;

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
            // Destroy old children
            List<Transform> toDestroy = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                toDestroy.Add(transform.GetChild(i));
            }
            for (int i = 0; i < toDestroy.Count; i++)
                DestroyImmediate(toDestroy[i].gameObject);

            // Drag Controller
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
                if (!charMapping.ContainsKey(Config.Letters[i].character))
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
                        makeTargets.Add(lde);

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
                        go.transform.localPosition += Vector3.up * DashHeight;

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
                targetPos.AddComponent<SpriteRenderer>().sprite = target.GetComponent<SpriteRenderer>().sprite;
                targetPos.transform.parent = dragLetters.transform;

                // 🔽 Integrated Safe Placement
                targetPos.transform.position = FindSafePosition(target.transform.position, dragLetters.transform);

                letter.TargetPosition = targetPos.transform;
                counter++;
                targetLetters.Add(target.GetComponent<LetterDefinition>());
            }

            dragController.Letters = allLeters.ToArray();
            dragController.TargetLetters = targetLetters.ToArray();

            mainLetters.transform.localScale = Vector3.one;
            dragLetters.transform.localScale = Vector3.one;
        }

        [ContextMenu("turnOff")]
        public void TurnOff()
        {
            SpriteRenderer[] render = transform.GetChild(1).GetComponentsInChildren<SpriteRenderer>();

            foreach (var r in render)
            {
                r.enabled = false; // turn off visibility
            }
        }
        [ContextMenu("turnON")]
        public void TurnON()
        {
            SpriteRenderer[] render = transform.GetChild(1).GetComponentsInChildren<SpriteRenderer>();

            foreach (var r in render)
            {
                r.enabled = true; // turn off visibility
            }
        }
        // New helper: finds a safe random pos
        Vector3 FindSafePosition(Vector3 basePos, Transform allTargetsRoot)
        {
            for (int i = 0; i < maxAttempts; i++)
            {
                Vector3 offset = Vector3.zero;
                int choice = Random.Range(0, 4); // 👈 6 variations abhi
                switch (choice)
                {
                    case 0: // Top Right (farther)
                        offset = new Vector3(Random.Range(6.5f, 9f), Random.Range(5.5f, 8f), 0);
                        break;

                    case 1: // Bottom Left
                        offset = new Vector3(Random.Range(-9f, -6.5f), Random.Range(-8f, -5.5f), 0);
                        break;

                    case 2: // Top Left
                        offset = new Vector3(Random.Range(-9f, -6.5f), Random.Range(5.5f, 8f), 0);
                        break;

                    case 3: // Bottom Right
                        offset = new Vector3(Random.Range(6.5f, 9f), Random.Range(-8f, -5.5f), 0);
                        break;

                        //case 4: // Far Up (direct vertical)
                        //    offset = new Vector3(Random.Range(-2f, 2f), Random.Range(8f, 10f), 0);
                        //    break;

                        //case 5: // Far Side (direct horizontal)
                        //    offset = new Vector3(Random.Range(10f, 12f), Random.Range(-2f, 2f), 0);
                        //    break;
                }
                Vector3 candidate = basePos + offset;

                // Check distance from character
                if (character != null && Vector3.Distance(candidate, character.position) < minCharacterDistance)
                    continue;

                // Check distance from other targets
                bool tooClose = false;
                foreach (Transform t in allTargetsRoot)
                {
                    if (Vector3.Distance(candidate, t.position) < safeDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;

                return candidate; // ✅ Found safe spot
            }

            Debug.LogWarning("No safe target position found, fallback used");
            return basePos + new Vector3(8, 6, 0); // fallback
        }
    }
}
