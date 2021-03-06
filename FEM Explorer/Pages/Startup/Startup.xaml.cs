using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace FEM_Explorer
{
    public sealed partial class Startup : Page
    {
        internal static Startup Current;
        //private bool IsLoaded = false;

        public Startup()
        {
            this.InitializeComponent();
            CustomizeTitleBar();
        }

        private void CustomizeTitleBar()
        {

            // customize title area
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(trickyTitleBar);

            // customize buttons' colors
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;

            titleBar.BackgroundColor = Color.FromArgb(255, 64, 64, 64);
            titleBar.ButtonBackgroundColor = Color.FromArgb(255, 64, 64, 64);

            titleBar.ForegroundColor = Colors.White;
            titleBar.ButtonForegroundColor = Colors.White;

            // handle windows size changes to restore custom title bar
            Window.Current.SizeChanged += Current_SizeChanged_UpdateTitleBar;
        }

        private void Current_SizeChanged_UpdateTitleBar(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode == false)
            {
                customTitleBar.Visibility = Visibility.Visible;
            }
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine("Menu Button Clicked");
            if (LeftFrameColumn.Width == new GridLength(32))
            {
                LeftFrameColumn.Width = new GridLength(320);
                frameContentDetails.Navigate(typeof(Details));
            }
            else
            {
                LeftFrameColumn.Width = new GridLength(32);
                frameContentDetails.Navigate(typeof(Slim));
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Current = this;
            //IsLoaded = true;

            frameContentDetails.Navigate(typeof(Slim));
            frameContentDisplay.Navigate(typeof(StartupDisplay));

            //Debug.WriteLine("Page Loaded");
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Current_SizeChanged_UpdateTitleBar;
            //IsLoaded = false;
        }

        //public void NavigateToFileLoading()
        //{
        //    //Frame.Navigate(typeof(FileLoading));
        //    Frame rootFrame = Window.Current.Content as Frame;
        //    rootFrame.Navigate(typeof(FileLoading));
        //}
    }
}
