using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace CommonLibrary.Util
{
    public static class PlatformIndependent
    {
        /// <summary>
        /// Indicates whether the running device is windows phone device.
        /// </summary>
        public static bool IsWindowsPhoneDevice
        {
            get
            {
                if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// [Platform safe] Determines whether the specified element is in visual tree.
        /// </summary>
        /// <param name="elem">The framework element.</param>
        /// <returns>
        /// 	<c>true</c> if the specified element is in visual tree; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInVisualTree(this FrameworkElement elem)
        {
            var current = elem;
            FrameworkElement temp = null; // <<IP>> take parent once
            while ((temp = (current.Parent ?? VisualTreeHelper.GetParent(current)) as FrameworkElement) != null)
            {
                if (Windows.UI.Xaml.Window.Current.Content == temp)
                {
                    return true;
                }
                current = temp;
            }

            try
            {
                return current == Windows.UI.Xaml.Window.Current.Content ||
                    (current is Popup && ((Popup)current).IsOpen);
            }
            catch
            {
                return false;
            }

        }
    }
}
