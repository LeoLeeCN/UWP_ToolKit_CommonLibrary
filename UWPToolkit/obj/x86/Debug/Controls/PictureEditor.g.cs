﻿#pragma checksum "D:\Test_Work\UWPToolkit\UWPToolkit\Controls\PictureEditor.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "7B03D37BD0CB7475029D381406CB8E50"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UWPToolkit.Controls
{
    partial class PictureEditor : 
        global::Windows.UI.Xaml.Controls.UserControl, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 14.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                {
                    global::Windows.UI.Xaml.Controls.UserControl element1 = (global::Windows.UI.Xaml.Controls.UserControl)(target);
                    #line 11 "..\..\..\Controls\PictureEditor.xaml"
                    ((global::Windows.UI.Xaml.Controls.UserControl)element1).Unloaded += this.UserControl_Unloaded;
                    #line default
                }
                break;
            case 2:
                {
                    this.mask_grid = (global::Windows.UI.Xaml.Controls.Grid)(target);
                }
                break;
            case 3:
                {
                    this.ringGrid = (global::Windows.UI.Xaml.Controls.Grid)(target);
                }
                break;
            case 4:
                {
                    this.ring = (global::Windows.UI.Xaml.Controls.ProgressRing)(target);
                }
                break;
            case 5:
                {
                    this.img_grid = (global::Windows.UI.Xaml.Controls.Grid)(target);
                }
                break;
            case 6:
                {
                    this.img = (global::Windows.UI.Xaml.Controls.Image)(target);
                }
                break;
            case 7:
                {
                    this.ink_canvas = (global::Windows.UI.Xaml.Controls.InkCanvas)(target);
                }
                break;
            case 8:
                {
                    this.inktoolbar = (global::Microsoft.Labs.InkToolbarControl.InkToolbar)(target);
                    #line 53 "..\..\..\Controls\PictureEditor.xaml"
                    ((global::Microsoft.Labs.InkToolbarControl.InkToolbar)this.inktoolbar).Loaded += this.inktoolbar_Loaded;
                    #line default
                }
                break;
            case 9:
                {
                    this.CropButton = (global::Windows.UI.Xaml.Controls.AppBarButton)(target);
                    #line 46 "..\..\..\Controls\PictureEditor.xaml"
                    ((global::Windows.UI.Xaml.Controls.AppBarButton)this.CropButton).Click += this.CropButton_Click;
                    #line default
                }
                break;
            case 10:
                {
                    this.SaveButton = (global::Windows.UI.Xaml.Controls.AppBarButton)(target);
                    #line 48 "..\..\..\Controls\PictureEditor.xaml"
                    ((global::Windows.UI.Xaml.Controls.AppBarButton)this.SaveButton).Click += this.SaveButton_Click;
                    #line default
                }
                break;
            case 11:
                {
                    this.CancelButton = (global::Windows.UI.Xaml.Controls.AppBarButton)(target);
                    #line 50 "..\..\..\Controls\PictureEditor.xaml"
                    ((global::Windows.UI.Xaml.Controls.AppBarButton)this.CancelButton).Click += this.CancelButton_Click;
                    #line default
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 14.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}
