using Windows.Foundation;

namespace CommonLibrary
{
    public struct FrameProperties
    {
        public readonly Rect Rect;
        public readonly double DelayMilliseconds;
        public readonly bool ShouldDispose;
        public readonly int Index;

        public FrameProperties(Rect rect, double delayMilliseconds, bool shouldDispose, int index)
        {
            Index = index;
            Rect = rect;
            DelayMilliseconds = delayMilliseconds;
            ShouldDispose = shouldDispose;
        }
    }
}
