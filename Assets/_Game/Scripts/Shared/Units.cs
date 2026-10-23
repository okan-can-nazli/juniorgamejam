namespace LeafBrawl.Shared
{
    /// <summary>balance.json'daki uzunluklar dünya px'tir; 40 px = 1 Unity birimi (SRS 3.13).</summary>
    public static class Units
    {
        public const float PxPerUnit = 40f;
        public static float ToUnits(float px) => px / PxPerUnit;
        public static float ToPx(float units) => units * PxPerUnit;
    }
}
