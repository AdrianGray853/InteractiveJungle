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

    [CreateAssetMenu(fileName = "ObjectCategoryConfig", menuName = "ScriptableObjects/ObjectCategoryConfig", order = 1)]
    public class CategoryRangeConfigSO : ScriptableObject
    {
    	public enum eCategory 
        { 
            None = 0,
            Farm,
            Sea,
            Birds,
            Zoo,
            Dinosaurs,
            Others,
            Transport
        }

    	[System.Serializable]
        public class CategoryRange
        {
            [HideInInspector]
            public string name = "category";
            //[HideInInspector]
            public eCategory category = eCategory.None;
            //[HideInInspector]
            public int Start = 0; // We need this as there is no guaranties that the order will always be correct?!.
            //public int End = 0;

            public int Count { get { return Stickers == null ? 0 : Stickers.Length; } }

            public Sprite[] Stickers;
        }

        public CategoryRange[] Ranges;

        public int GetStartLevel(int level)
        {
            foreach (var range in Ranges)
            {
                if (level >= range.Start && level < range.Start + range.Stickers.Length)
                    return range.Start;
            }

            return -1;
        }

        public int GetLastLevel(int level)
    	{
            foreach (var range in Ranges)
    		{
                if (level >= range.Start && level < range.Start + range.Stickers.Length)
                    return range.Start + range.Stickers.Length - 1;
    		}

            return -1;
    	}

        void Reset()
        {
            Ranges = new CategoryRange[]
            {
                new CategoryRange() { name = "Farm", category = eCategory.Farm },
                new CategoryRange() { name = "Sea", category = eCategory.Sea },
                new CategoryRange() { name = "Birds", category = eCategory.Birds },
                new CategoryRange() { name = "Zoo", category = eCategory.Zoo },
                new CategoryRange() { name = "Dinosaurs", category = eCategory.Dinosaurs },
                new CategoryRange() { name = "Others", category = eCategory.Others },
                new CategoryRange() { name = "Transport", category = eCategory.Transport }
            };
        }
    	private void OnValidate()
    	{
    		foreach (var range in Ranges)
    		{
                range.name = range.category.ToString();
    		}
    	}

    	public Sprite GetStickerByGlobalIdx(int idx)
    	{
            CategoryRange range = Ranges.FirstOrDefault(x => idx >= x.Start && idx < x.Start + x.Stickers.Length);
            if (range != null)
    		{
                return range.Stickers[idx - range.Start];
    		}

            return null;
    	}

        public Sprite GetStickerByCategoryIdx(eCategory category, int idx)
    	{
            CategoryRange range = Ranges.FirstOrDefault(x => x.category == category);
            if (range != null)
    		{
                return range.Stickers[idx - range.Start];
            }

            return null;
    	}
    }


}