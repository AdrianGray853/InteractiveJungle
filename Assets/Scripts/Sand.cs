using UnityEngine;

[System.Serializable]
public class SandItem : BaseItem
{
    //public GameObject fishPrefab;     // Optional: swimming fish or water elements

    // You can add more aquarium-specific data here
}
public class Sand : Base<SandItem>
{
    protected override void OnItemSelected(SandItem item)
    {
        base.OnItemSelected(item);
        // Add background-specific behavior here
    }

    protected override void OnItemLocked(SandItem item)
    {
        base.OnItemLocked(item);
        // Optional: show popup or sound
    }
    public override SandItem CloneItem(SandItem original)
    {
        return new SandItem
        {
            itemName = original.itemName,
            item = original.item,
            isLocked = original.isLocked,
            isDefault = false,
            postionkey = original.postionkey,
            // Don't assign postionkey or item here; assign later
        };
    }
}
