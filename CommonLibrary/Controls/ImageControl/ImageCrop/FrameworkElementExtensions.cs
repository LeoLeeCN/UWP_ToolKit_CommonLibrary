using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace CommonLibrary.Util
{
    public static class FrameworkElementExtensions
    {
        public static FrameworkElement FindDescendantByName(this FrameworkElement element, string name)
        {
            if (element == null || string.IsNullOrWhiteSpace(name)) { return null; }

            if (name.Equals(element.Name, StringComparison.OrdinalIgnoreCase))
            {
                return element;
            }
            var childCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < childCount; i++)
            {
                var result = (VisualTreeHelper.GetChild(element, i) as FrameworkElement).FindDescendantByName(name);
                if (result != null) { return result; }
            }
            return null;
        }


        public static IEnumerable<object> GetVisibleItems(this ItemsControl itemsControl)
        {
            for (int i = 0; i < itemsControl.Items.Count; i++)
            {
                var obj = itemsControl.ContainerFromIndex(i) as FrameworkElement;
                if (obj != null)
                {
                    GeneralTransform gt = obj.TransformToVisual(itemsControl);
                    var rect = gt.TransformBounds(new Rect(0, 0, obj.ActualWidth, obj.ActualHeight));

                    if (rect.Bottom < 0 || rect.Top > itemsControl.ActualHeight)
                    {
                        continue;
                    }

                    yield return itemsControl.Items[i];
                }
            }
        }
    }
}
