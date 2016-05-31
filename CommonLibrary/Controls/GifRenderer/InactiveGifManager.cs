using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommonLibrary
{
    public static class InactiveGifManager
    {
        private static int _inactiveCheckDelayInMiliseconds = 200;
        private static List<GifRenderer> _inactiveRenderers;
        private static List<GifRenderer> _inactiveRenderersCleanup;
        private static bool _isInactiveRunning;

        public static void Add(GifRenderer renderer)
        {
            if (_inactiveRenderers == null)
            {
                _inactiveRenderers = new List<GifRenderer>();
                _inactiveRenderersCleanup = new List<GifRenderer>();
            }

            _inactiveRenderers.Add(renderer);

            if (!_isInactiveRunning)
            {
                Start();
            }
        }

        public static void Remove(GifRenderer renderer)
        {
            _inactiveRenderers.Remove(renderer);
        }

        private static async void Start()
        {
            _isInactiveRunning = true;

            while (_isInactiveRunning)
            {
                CheckForInactivesBackOnScreen();
                await Task.Delay(_inactiveCheckDelayInMiliseconds);
            }
        }

        private static void CheckForInactivesBackOnScreen()
        {
            var copy = _inactiveRenderers.ToArray();
            foreach (var item in copy)
            {
                if (!item.IsOffScreen())
                {
                    item.Restart();
                    _inactiveRenderersCleanup.Add(item);
                }
            }

            foreach (var item in _inactiveRenderersCleanup)
            {
                Remove(item);
            }

            _inactiveRenderersCleanup.Clear();

            if (_inactiveRenderers.Count == 0)
            {
                Stop();
            }
        }

        private static void Stop()
        {
            _isInactiveRunning = false;
        }

        internal static void Clear()
        {
            Stop();
            if (_inactiveRenderers != null)
                _inactiveRenderers.Clear();

            if (_inactiveRenderersCleanup != null)
                _inactiveRenderersCleanup.Clear();
        }
    }
}
