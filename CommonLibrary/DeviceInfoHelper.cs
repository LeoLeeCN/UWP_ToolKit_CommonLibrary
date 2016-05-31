using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Devices.Power;
using Windows.Networking.Connectivity;
using Windows.Storage.Streams;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace CommonLibrary
{
    public class DeviceInfoHelper
    {
        public static bool IsStatusBarPresent = Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");
        public static bool isHWBtnAPIPresent = Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons");

        private static string _ashwid = string.Empty;
        public static string GetASHWID()
        {
            if (string.IsNullOrEmpty(_ashwid))
            {
                IBuffer id = null;

                HardwareToken ht = Windows.System.Profile.HardwareIdentification.GetPackageSpecificToken(null);

                id = ht.Id;

                var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(id);
                byte[] bytes = new byte[id.Length];
                dataReader.ReadBytes(bytes);
                string s = BitConverter.ToString(bytes);
                if (!string.IsNullOrWhiteSpace(s)) s = s.Replace("-", string.Empty);
                _ashwid = s;
            }
            return _ashwid;
        }

        public static void SetTitleBarStyle(Color fc, Color bc, Color fcInactive, Color bcInactive, string title = null)
        {
            if (IsDesktopFamily)
            {
                ApplicationView appView = ApplicationView.GetForCurrentView();
                if (!string.IsNullOrEmpty(title))
                {
                    appView.Title = title;
                }

                ApplicationViewTitleBar titleBar = appView.TitleBar;

                titleBar.BackgroundColor = bc;
                titleBar.ForegroundColor = fc;
                titleBar.InactiveBackgroundColor = bcInactive;
                titleBar.InactiveForegroundColor = fcInactive;

                titleBar.ButtonBackgroundColor = bc;
                titleBar.ButtonHoverBackgroundColor = bc;
                titleBar.ButtonPressedBackgroundColor = bc;
                titleBar.ButtonInactiveBackgroundColor = bcInactive;

                // Title bar button foreground colors. Alpha must be 255.
                titleBar.ButtonForegroundColor = fc;
                titleBar.ButtonHoverForegroundColor = fc;
                titleBar.ButtonPressedForegroundColor = fc;
                titleBar.ButtonInactiveForegroundColor = fcInactive;
            }
        }

        public static void SetTitleBarStyle(Color fc, Color bc, string title = null)
        {
            if (IsDesktopFamily)
            {
                ApplicationView appView = ApplicationView.GetForCurrentView();
                if (!string.IsNullOrEmpty(title))
                {
                    appView.Title = title;
                }

                ApplicationViewTitleBar titleBar = appView.TitleBar;

                titleBar.BackgroundColor = bc;
                titleBar.ForegroundColor = fc;

                titleBar.ButtonBackgroundColor = bc;
                titleBar.InactiveBackgroundColor = bc;
                titleBar.ButtonHoverBackgroundColor = bc;
                titleBar.ButtonPressedBackgroundColor = bc;
                titleBar.ButtonInactiveBackgroundColor = bc;

                // Title bar button foreground colors. Alpha must be 255.
                titleBar.ButtonForegroundColor = fc;
                titleBar.ButtonHoverForegroundColor = fc;
                titleBar.ButtonPressedForegroundColor = fc;
                titleBar.ButtonInactiveForegroundColor = fc;
            }
        }

        public static void SetStatusBarStyle(Color f, Color b, double opacity)
        {
            if (CommonLibrary.DeviceInfoHelper.IsStatusBarPresent)
            {
                StatusBar sb = StatusBar.GetForCurrentView();
                sb.ForegroundColor = f;
                sb.BackgroundColor = b;
                sb.BackgroundOpacity = opacity;
            }
        }

        public static async void ShowStatusBar(bool visible)
        {
            StatusBar sb = StatusBar.GetForCurrentView();
            if (visible)
            {
                await sb.ShowAsync();
            }
            else
            {
                await sb.HideAsync();
            }
        }

        public static DeviceFamilies DeviceFamily = DeviceInfoHelper.GetDeviceFamilyName();
        public static bool IsMobileFamily = (DeviceFamily == DeviceFamilies.Mobile ? true : false);
        public static bool IsDesktopFamily = (DeviceFamily == DeviceFamilies.Desktop ? true : false);

        public static DeviceFamilies GetDeviceFamilyName()
        {
            switch (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily.ToLower())
            {
                case "windows.desktop":
                    return DeviceFamilies.Desktop;
                case "windows.mobile":
                    return DeviceFamilies.Mobile;
                default:
                    return DeviceFamilies.None;
            }
        }

        public static double BatteryInfo()
        {
            Battery b = Battery.AggregateBattery;
            BatteryReport br = b.GetReport();
            if (br.FullChargeCapacityInMilliwattHours != null && br.RemainingCapacityInMilliwattHours != null)
            {
                double remian = Convert.ToDouble(br.RemainingCapacityInMilliwattHours);
                double full = Convert.ToDouble(br.FullChargeCapacityInMilliwattHours);
                return remian / full * 100;
            }
            else
            {
                return 75d;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true-wifi; false-cell; null-other</returns>
        public static bool? IsWifiAvailable()
        {
            ConnectionProfile pf = NetworkInformation.GetInternetConnectionProfile();
            if (pf != null)
            {
                if (pf.IsWlanConnectionProfile)
                {
                    return true;
                }
                else if (pf.IsWwanConnectionProfile)
                {
                    return false;
                }
            }
            return null;
        }

        /// <summary>
        /// check if network is available now, note that it will be always true in emulator
        /// </summary>
        /// <returns></returns>
        public static bool IsNetworkAvailable()
        {
            try
            {
                return NetworkInterface.GetIsNetworkAvailable();
            }
            catch (Exception)
            {
            }
            return false;
        }

        public static NetworkTypes GetNetworkType()
        {
            if (IsNetworkAvailable())
            {
                //Windows.Networking.Connectivity.ConnectionProfile connectionProfile = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
                var profile = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
                if (profile != null)
                {
                    if (profile.IsWlanConnectionProfile)
                    {
                        return NetworkTypes.Wifi;
                    }
                    else
                    {
                        if (profile.IsWwanConnectionProfile)
                        {
                            var connClass = profile.WwanConnectionProfileDetails.GetCurrentDataClass();
                            switch (connClass)
                            {
                                case WwanDataClass.None:
                                    return NetworkTypes.None;
                                case WwanDataClass.Gprs:
                                    return NetworkTypes.G2;
                                case WwanDataClass.Edge:
                                    return NetworkTypes.G2;
                                case WwanDataClass.Umts:
                                    return NetworkTypes.G3;
                                case WwanDataClass.Hsdpa:
                                    return NetworkTypes.G3;
                                case WwanDataClass.Hsupa:
                                    return NetworkTypes.G3;
                                case WwanDataClass.LteAdvanced:
                                    return NetworkTypes.G4;
                                case WwanDataClass.Cdma1xRtt:
                                    return NetworkTypes.G3;
                                case WwanDataClass.Cdma1xEvdo:
                                    return NetworkTypes.G3;
                                case WwanDataClass.Cdma1xEvdoRevA:
                                    return NetworkTypes.G3;
                                case WwanDataClass.Cdma1xEvdv:
                                    return NetworkTypes.G3;
                                case WwanDataClass.Cdma3xRtt:
                                    return NetworkTypes.G3;
                                case WwanDataClass.Cdma1xEvdoRevB:
                                    return NetworkTypes.G3;
                                case WwanDataClass.CdmaUmb:
                                    return NetworkTypes.G4;
                                case WwanDataClass.Custom:
                                    return NetworkTypes.Other;
                                default:
                                    return NetworkTypes.Other;
                            }
                        }
                    }
                }

            }
            return NetworkTypes.None;
        }


        /// <summary>
        /// get profile name
        /// </summary>
        /// <returns></returns>
        public static string GetConnectionName()
        {
            string sRet = networkTypeString.other;
            if (IsNetworkAvailable())
            {
                //Windows.Networking.Connectivity.ConnectionProfile connectionProfile = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
                var profile = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
                if (profile != null)
                {
                    if (profile.IsWlanConnectionProfile)
                    {
                        sRet = networkTypeString.wifi;
                    }
                    else
                    {
                        if (profile.IsWwanConnectionProfile)
                        {
                            var connClass = profile.WwanConnectionProfileDetails.GetCurrentDataClass();
                            switch (connClass)
                            {
                                case WwanDataClass.None:
                                    sRet = networkTypeString.other;
                                    break;
                                case WwanDataClass.Gprs:
                                    sRet = networkTypeString.G2;
                                    break;
                                case WwanDataClass.Edge:
                                    sRet = networkTypeString.G2;
                                    break;
                                case WwanDataClass.Umts:
                                    sRet = networkTypeString.G3;
                                    break;
                                case WwanDataClass.Hsdpa:
                                    sRet = networkTypeString.G3;
                                    break;
                                case WwanDataClass.Hsupa:
                                    sRet = networkTypeString.G3;
                                    break;
                                case WwanDataClass.LteAdvanced:
                                    sRet = networkTypeString.G4;
                                    break;
                                case WwanDataClass.Cdma1xRtt:
                                    sRet = networkTypeString.G3;
                                    break;
                                case WwanDataClass.Cdma1xEvdo:
                                    sRet = networkTypeString.G3;
                                    break;
                                case WwanDataClass.Cdma1xEvdoRevA:
                                    sRet = networkTypeString.G3;
                                    break;
                                case WwanDataClass.Cdma1xEvdv:
                                    sRet = networkTypeString.G3;
                                    break;
                                case WwanDataClass.Cdma3xRtt:
                                    sRet = networkTypeString.G3;
                                    break;
                                case WwanDataClass.Cdma1xEvdoRevB:
                                    sRet = networkTypeString.G3;
                                    break;
                                case WwanDataClass.CdmaUmb:
                                    sRet = networkTypeString.G4;
                                    break;
                                case WwanDataClass.Custom:
                                    sRet = networkTypeString.other;
                                    break;
                                default:
                                    sRet = networkTypeString.other;
                                    break;
                            }
                        }
                    }
                }

            }
            return sRet;
        }

        /// <summary>
        /// return NOKIA ****
        /// </summary>
        /// <returns></returns>
        public static string GetDeviceModelName()
        {
            var deviceInfo = new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation();
            return string.Format("{0} {1}", deviceInfo.SystemManufacturer, deviceInfo.SystemProductName);
        }

        /// <summary>
        /// get geo position (lat/long as basic info) for current device
        /// </summary>
        /// <returns>null - GPS not active now</returns>
        public async static Task<BasicGeoposition> GetGeoPosition()
        {
            GeolocationAccessStatus result = await Geolocator.RequestAccessAsync();
            if (result == GeolocationAccessStatus.Allowed)
            {
                Geolocator geo = new Geolocator();
                Geoposition pos = await geo.GetGeopositionAsync();
                return pos.Coordinate.Point.Position;
            }
            else
            {
                return new BasicGeoposition() { Longitude = 0d, Latitude = 0d, Altitude = 0d };
            }
        }

        /// <summary>
        /// using this method to open location setting page on phone/pc
        /// </summary>
        /// <returns></returns>
        public async static Task LaunchLocationSettingPage()
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-location"));
        }
    }

    public enum DeviceFamilies
    {
        None = 0,
        Desktop = 1,
        Mobile = 2
    }

    public class networkTypeString
    {
        public const string G2 = "2g";
        public const string G3 = "3g";
        public const string G4 = "4g";
        public const string other = "other";
        public const string wifi = "wifi";
    }

    public enum NetworkTypes
    {
        None,
        G2,
        G3,
        G4,
        Other,
        Wifi,
    }

}

