using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Interactive.Touch
{
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

    public class SpriteGroups : MonoBehaviour
    {
        [System.Serializable]
        public class SpriteGroup
        {
            public List<SpriteRenderer> Group = new List<SpriteRenderer>(); // ✅ initialized
            [HideInInspector]
            public List<Collider2D> Colliders = new List<Collider2D>();     // ✅ initialized

            public SpriteRenderer GetOverlapRenderer(Vector3 touchWorldPos)
            {
                int maxIndex = -10000;
                SpriteRenderer returnRenderer = null;

                for (int i = 0; i < Group.Count; i++)
                {
                    if (Colliders[i].OverlapPoint(touchWorldPos) && Group[i].sortingOrder > maxIndex)
                    {
                        returnRenderer = Group[i];
                        maxIndex = Group[i].sortingOrder;
                    }
                }

                return returnRenderer;
            }
        }
        public Transform trss;
        public List<SpriteGroup> RendererGroups;
        public Material material;
        private void Awake()
        {
            foreach (var group in RendererGroups)
            {
                group.Colliders = new List<Collider2D>();
                foreach (var g in group.Group)
                {
                    group.Colliders.Add(g.GetComponent<Collider2D>());
                }
            }
        }
        [ContextMenu("AddInList")]
        public void AddSprites()
        {
            RendererGroups.Clear(); // Pehle clear karo
            gameObject.name = trss.parent.name;
            Transform[] childs = trss.GetComponentsInChildren<Transform>();
            Debug.Log($"child Count {childs.Length}");
            trss.gameObject.AddComponent<Animator>();
            foreach (Transform child in childs)
            {
                if (child.TryGetComponent<SpriteRenderer>(out SpriteRenderer sr))
                {
                    Debug.Log($"child Name {child.name}");
                    sr.material = material;

                    // PolygonCollider2D ensure
                    var poly = child.GetComponent<PolygonCollider2D>();
                    if (poly == null)
                        child.gameObject.AddComponent<PolygonCollider2D>();

                    // New group add
                    SpriteGroup group = new SpriteGroup();
                    group.Group.Add(sr);
                    RendererGroups.Add(group);
                }
                else
                {
                    Debug.LogWarning($"Child {child.name} has NO SpriteRenderer, skipped!");
                }
            }
        }

        public List<SpriteRenderer> GetAllRenderers()
        {
            return RendererGroups.SelectMany(x => x.Group).ToList();
        }

        // Returns null if can't find any collisions
        public SpriteGroup GetOverlapGroup(Vector3 touchWorldPos)
        {
            SpriteGroup returnGroup = null;
            int maxIndex = -10000;
            for (int i = 0; i < RendererGroups.Count; i++)
            {
                for (int j = 0; j < RendererGroups[i].Group.Count; j++)
                {
                    if (RendererGroups[i].Colliders[j].OverlapPoint(touchWorldPos) && RendererGroups[i].Group[j].sortingOrder > maxIndex)
                    {
                        returnGroup = RendererGroups[i];
                        maxIndex = RendererGroups[i].Group[j].sortingOrder;
                    }
                }
            }

            return returnGroup;
        }
    }


}