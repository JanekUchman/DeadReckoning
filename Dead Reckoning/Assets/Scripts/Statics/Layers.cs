using UnityEngine;
public static class Layers
{
    [Range(0, 31)] public static int groundLayer = 8, bulletLayer = 10, playerLayer = 12;
    public static int[] walkableLayers = {groundLayer, playerLayer};
    public static LayerMask[] walkableLayersMask = { 1 <<groundLayer, 1 <<  playerLayer, };
    public static LayerMask playerLayerMask = 1 << playerLayer;

}
