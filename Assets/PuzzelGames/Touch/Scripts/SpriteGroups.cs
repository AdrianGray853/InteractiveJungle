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
            public List<SpriteRenderer> Group;
            [HideInInspector]
            public List<Collider2D> Colliders;

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

        public List<SpriteGroup> RendererGroups;

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