using UnityEngine;

namespace Interactive.PuzzelShape
{
using UnityEngine;

    public interface IDropZone
    {
        public bool CanDrop(Collider2D collider);
    }

}