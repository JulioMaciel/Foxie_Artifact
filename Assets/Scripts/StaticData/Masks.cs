using UnityEngine;

namespace StaticData
{
    public static class Masks
    {
        public static int Default = LayerMask.GetMask("Default");
        public static int TransparentFX = LayerMask.GetMask("TransparentFX");
        public static int IgnoreRaycast = LayerMask.GetMask("Ignore Raycast");
        public static int Terrain = LayerMask.GetMask("Terrain");
        public static int Water = LayerMask.GetMask("Water");
        public static int UI = LayerMask.GetMask("UI");
        public static int Obstacle = LayerMask.GetMask("Obstacle");
        public static int GrassTerrain = LayerMask.GetMask("GrassTerrain");
        public static int SandTerrain = LayerMask.GetMask("SandTerrain");
    }
}