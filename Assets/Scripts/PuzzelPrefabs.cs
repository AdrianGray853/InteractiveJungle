using UnityEngine;
using UnityEngine.Rendering;
using Interactive.DRagDrop;
using System.Collections.Generic;
using Interactive.Touch;
using System.IO;
using static Interactive.Touch.SpriteGroups;


#if UNITY_EDITOR
using UnityEditor; // Needed for saving prefabs in editor
#endif
using Interactive.PuzzelShape;

public class PuzzelPrefabs : MonoBehaviour
{

    public GameObject[] parts;          // PSB prefabs
    public Sprite[] outlineParts;       // Outline sprites
    public GameObject Prefab;           // Base puzzle prefab
    public GameObject OutlinePrefab;    // Outline prefab
    public string key;
    public MixWordGameplay cont;

    public Material material;
    [ContextMenu("Generate Puzzle Prefabs")]
    public void GeneratePuzzlePrefabs()
    {
        for (int i = 0; i < parts.Length; i++)
        {
            // Create parent prefab holder
            GameObject obj = Instantiate(Prefab);

            // Instantiate puzzle part (PSB prefab safe instantiate)
            GameObject part;
#if UNITY_EDITOR
            part = (GameObject)PrefabUtility.InstantiatePrefab(parts[i], obj.transform);
#else
            part = Instantiate(parts[i], obj.transform);
#endif

            // Instantiate outline prefab
            GameObject outline = Instantiate(OutlinePrefab, obj.transform);

            // Naming
            obj.name = $"{key} {i + 1}";
            part.name = $"{key}PuzzelParts {i + 1}";
            outline.name = $"{key}PuzzelOutline {i + 1}";

            // Assign outline sprite
            var sr = outline.GetComponent<SpriteRenderer>();
            if (sr != null && i < outlineParts.Length)
                sr.sprite = outlineParts[i];

            // Sorting group
            part.AddComponent<SortingGroup>().sortingOrder = 100;

            // Offset outline
            outline.transform.localPosition = new Vector3(0, 10.25f, 0);

            // Add colliders + outline controller to children
            Transform[] partsChildren = part.GetComponentsInChildren<Transform>();
            foreach (Transform child in partsChildren)
            {
                if (child == part.transform) continue; // Skip root

                if (child.GetComponent<PolygonCollider2D>() == null)
                    child.gameObject.AddComponent<PolygonCollider2D>();

                var outlineCtrl = child.gameObject.AddComponent<OutlineControllerShape>();
                outlineCtrl.Antialiased = true;
            }

            // Assign to controller
            ShapePuzzleController controller = obj.GetComponent<ShapePuzzleController>();
            controller.Target = outline;

            // Assign puzzle pieces
            PolygonCollider2D[] colliders = part.GetComponentsInChildren<PolygonCollider2D>();
            controller.PuzzlePieces = new Collider2D[colliders.Length];
            for (int j = 0; j < colliders.Length; j++)
            {
                controller.PuzzlePieces[j] = colliders[j];
            }

#if UNITY_EDITOR
            // Optional: Save as prefab asset
            string folderPath = "Assets/GeneratedPrefabs/";
            if (!System.IO.Directory.Exists(folderPath))
                System.IO.Directory.CreateDirectory(folderPath);

            string localPath = folderPath + parts[i].name + ".prefab";
            localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);

            PrefabUtility.SaveAsPrefabAsset(obj, localPath);
            DestroyImmediate(obj); // Clean up scene
#endif
        }

#if UNITY_EDITOR
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif
    }


    [ContextMenu("Generate Coloring Prefabs")]
    public void AddSprites()
    {
#if UNITY_EDITOR
        if (Prefab == null)
        {
            Debug.LogError("❌ Prefab not assigned!");
            return;
        }
        int count = 1;
        foreach (var item in parts)
        {
            // Create parent prefab holder
            GameObject obj = Instantiate(Prefab);

            // Instantiate puzzle part (PSB prefab safe instantiate)
            GameObject part;
#if UNITY_EDITOR
            part = (GameObject)PrefabUtility.InstantiatePrefab(item, obj.transform);
#else
            //part = Instantiate(parts[i], obj.transform);
#endif
            obj.name = $"Animal {count}";
            count++;
            Transform trss = obj.transform.GetChild(0);

            // Ensure SpriteGroups
            SpriteGroups gr = obj.GetComponent<SpriteGroups>();
            if (gr == null)
                gr = obj.AddComponent<SpriteGroups>();

            gr.RendererGroups.Clear();

            if (trss.GetComponent<Animator>() == null)
                trss.gameObject.AddComponent<Animator>();

            Transform[] childs = trss.GetComponentsInChildren<Transform>();
            foreach (Transform child in childs)
            {
                if (child.TryGetComponent<SpriteRenderer>(out SpriteRenderer sr))
                {
                    if (material != null)
                        sr.material = material;

                    if (child.GetComponent<PolygonCollider2D>() == null)
                        child.gameObject.AddComponent<PolygonCollider2D>();

                    SpriteGroup group = new SpriteGroup();
                    group.Group.Add(sr);
                    group.Colliders.Add(sr.GetComponent<Collider2D>());
                    gr.RendererGroups.Add(group);
                }
            }

            // Save as new prefab in folder
            string folderPath = "Assets/GeneratedPrefabs/";
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string localPath = folderPath + obj.name + ".prefab";
            localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);

            PrefabUtility.SaveAsPrefabAsset(obj, localPath);
            DestroyImmediate(obj); // Clean up scene
                                   // cleanup
            Debug.Log($"✅ Saved prefab at: {localPath}");
        }
#endif



#if UNITY_EDITOR
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif
    }
    [System.Serializable]
    public class PuzzelsPrefab
    {
        public List<string> puzzelString = new List<string>();

    }
    public List<PuzzelsPrefab> puzzell = new List<PuzzelsPrefab>()
{
    new PuzzelsPrefab { puzzelString = new List<string> { FormatWord("Parrot"), FormatWord("RED"), FormatWord("blue") } },
    new PuzzelsPrefab { puzzelString = new List<string> { FormatWord("toucan"), FormatWord("BIG"), FormatWord("Beak") } },
    new PuzzelsPrefab { puzzelString = new List<string> { FormatWord("Hornbill"), FormatWord("Yellow"), FormatWord("BLACK") } },
    new PuzzelsPrefab { puzzelString = new List<string> { FormatWord("Parakeet"), FormatWord("yellow"), FormatWord("Small") } },
    new PuzzelsPrefab { puzzelString = new List<string> { FormatWord("Hornbill"), FormatWord("black"), FormatWord("WHITE") } },
    new PuzzelsPrefab { puzzelString = new List<string> { FormatWord("hummingbird"), FormatWord("Green"), FormatWord("TINY") } },
    new PuzzelsPrefab { puzzelString = new List<string> { FormatWord("sunbird"), FormatWord("Violet"), FormatWord("WINGS") } },
    new PuzzelsPrefab { puzzelString = new List<string> { FormatWord("Quetzal"), FormatWord("LONG"), FormatWord("tail") } },
    new PuzzelsPrefab { puzzelString = new List<string> { FormatWord("PARADISE"), FormatWord("Bird"), FormatWord("feather") } }
};

    private static string FormatWord(string word)
    {
        // Add # after each character
        return string.Join("#", word.ToCharArray()) + "#";
    }
    public GameObject prefabObj;
    [ContextMenu("Generate Words Prefabs")]
    public void GenrateColoringWords()
    {
#if UNITY_EDITOR
        if (prefabObj == null)
        {
            Debug.LogError("❌ Prefab object not assigned!");
            return;
        }

        string folderPath = "Assets/GeneratedPrefabs/";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        int count = 1;

        for (int i = 0; i < puzzell.Count; i++)
        {
            PuzzelsPrefab items = puzzell[i];

            for (int j = 0; j < items.puzzelString.Count; j++)
            {
                string word = items.puzzelString[j];

                // Create instance of prefab
                GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefabObj);
                obj.name = $"{i + 1}.{word}";

                // Example: add a TextMesh to display the word
                //var text = obj.GetComponentInChildren<TextMesh>();
                //if (text == null)
                //    text = obj.AddComponent<TextMesh>();

                //text.text = word;
                CreateTextHelper HELPER = obj.GetComponent<CreateTextHelper>();
                HELPER.Text = word;
                HELPER.Generate = true;
                // Save prefab in folder
                string localPath = folderPath + obj.name + ".prefab";
                localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);

                GameObject obej = PrefabUtility.SaveAsPrefabAsset(obj, localPath);
                //CreateTextHelper HELPER = obej.GetComponent<CreateTextHelper>();
                //HELPER.Text = word;
                //HELPER.Generate = true;
                //DestroyImmediate(obj);

                Debug.Log($"✅ Saved word prefab at: {localPath}");
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif
    }
}