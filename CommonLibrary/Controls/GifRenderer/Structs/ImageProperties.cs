namespace CommonLibrary
{
    public struct ImageProperties
    {
        public readonly int PixelWidth;
        public readonly int PixelHeight;
        public readonly bool IsAnimated;
        public readonly int LoopCount;

        public ImageProperties(int pixelWidth, int pixelHeight, bool isAnimated, int loopCount)
        {
            PixelWidth = pixelWidth;
            PixelHeight = pixelHeight;
            IsAnimated = isAnimated;
            LoopCount = loopCount;
        }
    }
}
