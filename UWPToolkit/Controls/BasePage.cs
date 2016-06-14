using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace UWPToolkit.Controls
{
    public class BasePage : Page
    {
        private PageStytle _pageStytle = PageStytle.None;
        public PageStytle PageStytle
        {
            get
            {
                return _pageStytle;
            }
            set
            {
                if (_pageStytle != value)
                {
                    _pageStytle = value;
                }
            }
        }

        public int PageID = 0;
        public BasePage(PageStytle style = PageStytle.None)
        {
            this.IsTextScaleFactorEnabled = false;
            this.SizeChanged += BasePage_SizeChanged;
            this.Loaded += BasePage_Loaded;
            this.Unloaded += BasePage_Unloaded;
            PageStytle = style;
            if (PageStytle != PageStytle.Main)
            {
                this.ManipulationMode = ManipulationModes.TranslateX;
                this.ManipulationCompleted += BasePage_ManipulationCompleted;
                this.ManipulationDelta += BasePage_ManipulationDelta;

                _tt = this.RenderTransform as TranslateTransform;
                if (_tt == null) this.RenderTransform = _tt = new TranslateTransform();
            }
        }

        private void BasePage_Loaded(object sender, RoutedEventArgs e)
        {
            OnLoaded(e);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += BasePage_BackRequested;
        }

        private void BasePage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            MasterPage.BackRequest();
        }

        private void BasePage_Unloaded(object sender, RoutedEventArgs e)
        {
            OnUnloaded(e);
        }

        private void BasePage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var eArgs = new PageSizeChangedEventArgs(e.OriginalSource, e.PreviousSize, e.NewSize);
            OnPageSizeChanged(eArgs);
        }

        protected virtual void OnLoaded(RoutedEventArgs e)
        {
        }

        protected virtual void OnUnloaded(RoutedEventArgs e)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        protected virtual void OnPageSizeChanged(PageSizeChangedEventArgs e)
        {

        }

        #region Swipe Gestures 手势滑动
        private TranslateTransform _tt;
        /// <summary>
        /// 0:后退 -1：向左划  1：向右划
        /// </summary>
        private int action = 0;

        private void BasePage_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

            if (_tt.X + e.Delta.Translation.X < 0)
            {
                _tt.X = 0;
                return;
            }
            _tt.X += e.Delta.Translation.X;

        }


        private void BasePage_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {

            double abs_delta = Math.Abs(e.Cumulative.Translation.X);
            double speed = Math.Abs(e.Velocities.Linear.X);
            double delta = e.Cumulative.Translation.X;
            double to = 0;

            if (abs_delta < this.ActualWidth / 3 && speed < 0.5)
            {
                _tt.X = 0;
                return;
            }


            action = 0;
            if (delta > 0)
                to = this.ActualWidth;
            else if (delta < 0)
                return;


            var s = new Storyboard();
            var doubleanimation = new DoubleAnimation() { Duration = new Duration(TimeSpan.FromMilliseconds(120)), From = _tt.X, To = to };
            doubleanimation.Completed += Doubleanimation_Completed;
            Storyboard.SetTarget(doubleanimation, _tt);
            Storyboard.SetTargetProperty(doubleanimation, "X");
            s.Children.Add(doubleanimation);
            s.Begin();
        }

        private void Doubleanimation_Completed(object sender, object e)
        {
            if (action == 0)
            {
                MasterPage.BackRequest();
            }
            else
            {
            }

            _tt = this.RenderTransform as TranslateTransform;
            if (_tt == null) this.RenderTransform = _tt = new TranslateTransform();
            _tt.X = 0;

        }
    }
    #endregion

    public class PageSizeChangedEventArgs : EventArgs
    {
        public PageSizeChangedEventArgs(object originalSource, Size previousSize, Size newSize)
        {
            this.OriginalSource = originalSource;
            this.PreviousSize = previousSize;
            this.NewSize = newSize;
        }

        public Size NewSize { private set; get; }
        public Size PreviousSize { private set; get; }
        public object OriginalSource { private set; get; }
    }

    public enum PopupMode
    {
        H5,
        Native,
        None
    }

    public enum PageStytle
    {
        Main,
        Left,
        Right,
        Detail,
        None
    }
}

