using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor; // Needed for saving prefabs in editor
#endif
using Interactive.PuzzelShape;

public class PuzzelPrefabs : MonoBehaviour
{
    public GameObject[] parts;
    public Sprite[] outlineParts;
    public GameObject Prefab;
    public GameObject OutlinePrefab;

    [ContextMenu("Generate Puzzle Prefabs")]
    public void GeneratePuzzlePrefabs()
    {
        for (int i = 0; i < parts.Length; i++)
        {
            GameObject obj = Instantiate(Prefab);
            GameObject part = Instantiate(parts[i], obj.transform);
            GameObject outline = Instantiate(OutlinePrefab, obj.transform);

            obj.name = $"PuzzelBackGround {i + 1}";
            part.name = $"PuzzelParts {i + 1}";
            outline.name = $"PuzzelOutline {i + 1}";
            outline.GetComponent<SpriteRenderer>().sprite = outlineParts[i];
            part.AddComponent<SortingGroup>().sortingOrder = 100;
            // Get all children of part
            Transform[] partsChildren = part.GetComponentsInChildren<Transform>();
            outline.transform.localPosition= new Vector3(0,10.25f,0);
            foreach (Transform child in partsChildren)
            {
                if (child == part.transform) continue; // Skip root

                var poly = child.gameObject.AddComponent<PolygonCollider2D>();

                var outlineCtrl = child.gameObject.AddComponent<OutlineControllerShape>();
                outlineCtrl.Antialiased = true;
            }

            ShapePuzzleController controller = obj.GetComponent<ShapePuzzleController>();
            controller.Target = outline;

            // Assign puzzle pieces
            PolygonCollider2D[] colliders = part.GetComponentsInChildren<PolygonCollider2D>();
            controller.PuzzlePieces = new Collider2D[colliders.Length];
            for (int j = 0; j < colliders.Length; j++)
            {
                controller.PuzzlePieces[j] = colliders[j];
            }

//#if UNITY_EDITOR
//            // Save prefab in Assets folder (optional)
//            string localPath = "Assets/GeneratedPrefabs/" + parts[i].name + ".prefab";
//            localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
//            PrefabUtility.SaveAsPrefabAsset(obj, localPath);
//            DestroyImmediate(obj);
//#endif
        }
    }
}
