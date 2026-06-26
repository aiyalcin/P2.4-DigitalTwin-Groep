using UnityEngine;
public static class ProductIdentityEnums
{
    /// <summary>
    /// Types of products
    /// </summary>
    public enum Type
    {
        Apples = 0,
        Pears = 1,
    }

    public enum State
    {
        SearchingForBox,
        CarryingBox
    }
}