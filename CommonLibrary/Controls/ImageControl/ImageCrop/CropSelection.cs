using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace CommonLibrary
{
    public class CropSelection : INotifyPropertyChanged
    {

        #region Property

        /// <summary>
        /// The minimun size of the seleced region
        /// </summary>
        public double MinSelectRegionSize { get; set; }

        public AspectRatio CropAspectRatio { get; set; }

        private Rect outerRect;

        /// <summary>
        /// The outer rect. The non-selected region can be represented by the 
        /// OuterRect and the SelectedRect.
        /// </summary>
        public Rect OuterRect
        {
            get { return outerRect; }
            set
            {
                if (outerRect != value)
                {
                    outerRect = value;

                    this.OnPropertyChanged("OuterRect");
                }
            }
        }

        private Rect selectedRect;

        /// <summary>
        /// The selected region, which is represented by the four Thumbs.
        /// </summary>
        public Rect SelectedRect
        {
            get { return selectedRect; }
            set
            {
                if (selectedRect != value)
                {
                    selectedRect = value;
                    HorizontalLine1 = new Rect(selectedRect.Left, selectedRect.Top + selectedRect.Height / 3, selectedRect.Width, 0.5);
                    HorizontalLine2 = new Rect(selectedRect.Left, selectedRect.Top + selectedRect.Height / 3 * 2, selectedRect.Width, 0.5);
                    VerticalLine1 = new Rect(selectedRect.Left + selectedRect.Width / 3, selectedRect.Top, 0.5, selectedRect.Height);
                    VerticalLine2 = new Rect(selectedRect.Left + selectedRect.Width / 3 * 2, selectedRect.Top, 0.5, selectedRect.Height);

                    //HorizontalLine1StartPoint = new Point(selectedRect.Left, selectedRect.Top + selectedRect.Height / 3);
                    //HorizontalLine1EndPoint = new Point(selectedRect.Left + selectedRect.Width, selectedRect.Top + selectedRect.Height / 3);

                    //HorizontalLine2StartPoint = new Point(selectedRect.Left, selectedRect.Top + selectedRect.Height / 3 * 2);
                    //HorizontalLine2EndPoint = new Point(selectedRect.Left + selectedRect.Width, selectedRect.Top + selectedRect.Height / 3 * 2);

                    //VerticalLine1StartPoint = new Point(selectedRect.Left + selectedRect.Width / 3, selectedRect.Top);
                    //VerticalLine1EndPoint = new Point(selectedRect.Left + selectedRect.Width / 3, selectedRect.Top + selectedRect.Height);

                    //VerticalLine2StartPoint = new Point(selectedRect.Left + selectedRect.Width / 3 * 2, selectedRect.Top);
                    //VerticalLine2EndPoint = new Point(selectedRect.Left + selectedRect.Width / 3 * 2, selectedRect.Top + selectedRect.Height);

                    this.OnPropertyChanged("SelectedRect");
                    //OnPropertyChanged("HorizontalLineCanvasTop");
                    //OnPropertyChanged("VerticalLineCanvasLeft");
                    //OnPropertyChanged("HorizontalLine1CanvasTop");
                    //OnPropertyChanged("VerticalLine1CanvasLeft");
                }
            }
        }

        private Rect horizontalLine1;
        public Rect HorizontalLine1
        {
            get { return horizontalLine1; }
            set
            {
                if (horizontalLine1 != value)
                {
                    horizontalLine1 = value;

                    OnPropertyChanged("HorizontalLine1");
                }
            }
        }

        private Rect horizontalLine2;
        public Rect HorizontalLine2
        {
            get { return horizontalLine2; }
            set
            {
                if (horizontalLine2 != value)
                {
                    horizontalLine2 = value;

                    OnPropertyChanged("HorizontalLine2");
                }
            }
        }

        private Rect verticalLine1;
        public Rect VerticalLine1
        {
            get { return verticalLine1; }
            set
            {
                if (verticalLine1 != value)
                {
                    verticalLine1 = value;

                    OnPropertyChanged("VerticalLine1");
                }
            }
        }

        private Rect verticalLine2;
        public Rect VerticalLine2
        {
            get { return verticalLine2; }
            set
            {
                if (verticalLine2 != value)
                {
                    verticalLine2 = value;

                    OnPropertyChanged("VerticalLine2");
                }
            }
        }

        //private Point horizontalLine1StartPoint;
        //public Point HorizontalLine1StartPoint
        //{
        //    get { return horizontalLine1StartPoint; }
        //    set
        //    {
        //        if (horizontalLine1StartPoint != value)
        //        {
        //            horizontalLine1StartPoint = value;

        //            OnPropertyChanged("HorizontalLine1StartPoint");
        //        }
        //    }
        //}

        //private Point horizontalLine1EndPoint;
        //public Point HorizontalLine1EndPoint
        //{
        //    get { return horizontalLine1EndPoint; }
        //    set
        //    {
        //        if (horizontalLine1EndPoint != value)
        //        {
        //            horizontalLine1EndPoint = value;

        //            OnPropertyChanged("HorizontalLine1EndPoint");
        //        }
        //    }
        //}

        //private Point horizontalLine2StartPoint;
        //public Point HorizontalLine2StartPoint
        //{
        //    get { return horizontalLine2StartPoint; }
        //    set
        //    {
        //        if (horizontalLine2StartPoint != value)
        //        {
        //            horizontalLine2StartPoint = value;

        //            OnPropertyChanged("HorizontalLine2StartPoint");
        //        }
        //    }
        //}

        //private Point horizontalLine2EndPoint;
        //public Point HorizontalLine2EndPoint
        //{
        //    get { return horizontalLine2EndPoint; }
        //    set
        //    {
        //        if (horizontalLine2EndPoint != value)
        //        {
        //            horizontalLine2EndPoint = value;

        //            OnPropertyChanged("HorizontalLine2EndPoint");
        //        }
        //    }
        //}

        //private Point verticalLine1StartPoint;
        //public Point VerticalLine1StartPoint
        //{
        //    get { return verticalLine1StartPoint; }
        //    set
        //    {
        //        if (verticalLine1StartPoint != value)
        //        {
        //            verticalLine1StartPoint = value;

        //            OnPropertyChanged("VerticalLine1StartPoint");
        //        }
        //    }
        //}

        //private Point verticalLine1EndPoint;
        //public Point VerticalLine1EndPoint
        //{
        //    get { return verticalLine1EndPoint; }
        //    set
        //    {
        //        if (verticalLine1EndPoint != value)
        //        {
        //            verticalLine1EndPoint = value;

        //            OnPropertyChanged("VerticalLine1EndPoint");
        //        }
        //    }
        //}

        //private Point verticalLine2StartPoint;
        //public Point VerticalLine2StartPoint
        //{
        //    get { return verticalLine2StartPoint; }
        //    set
        //    {
        //        if (verticalLine2StartPoint != value)
        //        {
        //            verticalLine2StartPoint = value;

        //            OnPropertyChanged("VerticalLine2StartPoint");
        //        }
        //    }
        //}

        //private Point verticalLine2EndPoint;
        //public Point VerticalLine2EndPoint
        //{
        //    get { return verticalLine2EndPoint; }
        //    set
        //    {
        //        if (verticalLine2EndPoint != value)
        //        {
        //            verticalLine2EndPoint = value;

        //            OnPropertyChanged("VerticalLine2EndPoint");
        //        }
        //    }
        //}

        public double HorizontalLineCanvasTop
        {
            get
            {
                return (SelectedRect.Bottom - SelectedRect.Top) / 3 * 1 + SelectedRect.Top;
            }

        }
        public double HorizontalLine1CanvasTop
        {
            get
            {
                return (SelectedRect.Bottom - SelectedRect.Top) / 3 * 2 + SelectedRect.Top;
            }

        }

        public double VerticalLineCanvasLeft
        {
            get
            {
                return (SelectedRect.Right - SelectedRect.Left) / 3 * 1 + SelectedRect.Left;
            }
        }

        public double VerticalLine1CanvasLeft
        {
            get
            {
                return (SelectedRect.Right - SelectedRect.Left) / 3 * 2 + SelectedRect.Left;
            }
        }


        private Visibility _cropSelectionVisibility=Visibility.Collapsed;

        public Visibility CropSelectionVisibility
        {
            get { return _cropSelectionVisibility; }
            set
            {
                _cropSelectionVisibility = value;
                this.OnPropertyChanged("CropSelectionVisibility");
            }
        }


        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        #endregion

        #region Method
        internal void UpdateSelectedRect(float scale, double x, double y)
        {
            var rect = new Rect() { X = SelectedRect.X + x, Y = SelectedRect.Y + y, Width = SelectedRect.Width * scale, Height = SelectedRect.Height * scale };
            var leftTop = new Point(rect.Left, rect.Top);
            var leftBottom = new Point(rect.Left, rect.Bottom);
            var rightTop = new Point(rect.Right, rect.Top);
            var rightBottom = new Point(rect.Right, rect.Bottom);

            if (OuterRect.Contains(leftTop)
                && OuterRect.Contains(leftBottom)
                && OuterRect.Contains(rightTop)
                && OuterRect.Contains(rightBottom)
                && rect.Width >= 2 * MinSelectRegionSize
                && rect.Height >= 2 * MinSelectRegionSize)
            {
                SelectedRect = rect;
            }

        }

        internal void UpdateSelectedRect(string ThumbName, double xUpdate, double yUpdate,Rect? outerRect=null)
        {


            var left = SelectedRect.Left;
            var top = SelectedRect.Top;

            var right = SelectedRect.Right;
            var bottom = SelectedRect.Bottom;


            if (ThumbName == "topLeftThumb")
            {
                left += xUpdate;
                top += yUpdate;
            }
            else if (ThumbName == "topRightThumb")
            {
                right += xUpdate;
                top += yUpdate;
            }
            else if (ThumbName == "bottomLeftThumb")
            {
                left += xUpdate;
                bottom += yUpdate;
            }
            else if (ThumbName == "bottomRightThumb")
            {
                right += xUpdate;
                bottom += yUpdate;
            }

            var rect = new Rect(new Point(left, top), new Point(right, bottom));
            var leftTop = new Point(rect.Left, rect.Top);
            var leftBottom = new Point(rect.Left, rect.Bottom);
            var rightTop = new Point(rect.Right, rect.Top);
            var rightBottom = new Point(rect.Right, rect.Bottom);

            var outerRect1 = outerRect!=null ? outerRect.Value: OuterRect;

            if (outerRect1.Contains(leftTop)
                && outerRect1.Contains(leftBottom)
                && outerRect1.Contains(rightTop)
                && outerRect1.Contains(rightBottom)
                && rect.Width >= 2 * MinSelectRegionSize
                && rect.Height >= 2 * MinSelectRegionSize)
            {
                SelectedRect = rect;
            }
        }


        internal void ResizeSelectedRect(double scale)
        {
            var rect = new Rect() { X = SelectedRect.X * scale, Y = SelectedRect.Y * scale, Width = SelectedRect.Width * scale, Height = SelectedRect.Height * scale };
            var leftTop = new Point(rect.Left, rect.Top);
            var leftBottom = new Point(rect.Left, rect.Bottom);
            var rightTop = new Point(rect.Right, rect.Top);
            var rightBottom = new Point(rect.Right, rect.Bottom);

            if (OuterRect.Contains(leftTop)
                && OuterRect.Contains(leftBottom)
                && OuterRect.Contains(rightTop)
                && OuterRect.Contains(rightBottom)
                && rect.Width >= 2 * MinSelectRegionSize
                && rect.Height >= 2 * MinSelectRegionSize)
            {
                SelectedRect = rect;
            }
        }
        #endregion
    }
}
