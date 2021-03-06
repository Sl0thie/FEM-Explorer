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
    public sealed partial class Construction : Page
    {
        internal static Construction Current;
        private bool IsLoaded = false;
        
        private bool detailsIsOpen = true;
        public bool DetailsIsOpen
        {
            get { return detailsIsOpen; }
            set { detailsIsOpen = value; }
        }

        public Construction()
        {
            this.InitializeComponent();
            CustomizeTitleBar();
        }
        
        public void Refresh()
        {
            if (IsLoaded)
            {
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(Construction));
            }
        }

        public void SetTitle(string newTitle)
        {
            textBlock_Title.Text = newTitle;
        }

        #region Show

        public void ShowMember()
        {
            frameDetails.Navigate(typeof(PanelMember));
        }

        public void ShowSolver()
        {
            if (IsLoaded)
            {
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(Solver));
            }
        }

        public void ShowHelp()
        {
            frameDetails.Width = 220;
            frameDetails.Navigate(typeof(PanelHelp));
            frameDisplay.Navigate(typeof(Help));
        }

        public void ShowModel()
        {
            if (IsLoaded)
            {
                App.CurrentPageState = PageState.Construction;
                frameDetails.Width = 300;
                frameDetails.Navigate(typeof(PanelModel));
            }
        }

        public void ShowSections()
        {
            FileManager.SaveFile();
            App.CurrentPageState = PageState.Sections;
            frameDetails.Width = 220;
            frameDetails.Navigate(typeof(PanelSections));
            frameDisplay.Navigate(typeof(DisplaySection));
        }

        public void ShowSection()
        {
            FileManager.SaveFile();
            App.CurrentPageState = PageState.Sections;
            frameDetails.Width = 220;
            frameDetails.Navigate(typeof(PanelSections));
            frameDisplay.Navigate(typeof(DisplaySection));
        }

        public void ShowSettingsColors()
        {
            frameDetails.Width = 300;
            frameDetails.Navigate(typeof(PanelSettingsColors));
        }

        public void ShowSettingsSolver()
        {
            frameDetails.Width = 300;
            frameDetails.Navigate(typeof(PanelSettingsSolver));
        }

        public void ShowSettingsGeneral()
        {
            frameDetails.Width = 300;
            frameDetails.Navigate(typeof(PanelSettingsGeneral));
        }

        public void ShowDrawing()
        {
            if (Model.Members.CurrentMember == null)
            {
                frameDetails.Width = 300;
                frameDetails.Navigate(typeof(PanelModel));
            }
            else
            {
                frameDetails.Width = 300;
                frameDetails.Navigate(typeof(PanelMember));
            }
            frameDisplay.Navigate(typeof(ConstructionDisplay));
        }

        #endregion

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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

            Current = this;
            IsLoaded = true;
            if (Model.Members.CurrentMember == null)
            {
                Model.Sections.CurrentSection = Model.Sections["Default"];
                frameDetails.Navigate(typeof(PanelModel));
            }
            else
            {
                frameDetails.Navigate(typeof(PanelMember));
            }
            frameDisplay.Navigate(typeof(ConstructionDisplay));
            App.CurrentPageState = PageState.Construction;
            SetTitle("Plane Frame Analysis - " + FileManager.FileTitle);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            IsLoaded = false;
        }

        private void MenuFlyoutNew(object sender, RoutedEventArgs e)
        {

        }

        private void MenuFlyoutOpen(object sender, RoutedEventArgs e)
        {

        }

        private void MenuFlyoutSave(object sender, RoutedEventArgs e)
        {

        }

        private void MenuFlyoutSaveAs(object sender, RoutedEventArgs e)
        {

        }

        private void menuButton_Click(object sender, RoutedEventArgs e)
        {
            if (detailsIsOpen)
            {
                detailsIsOpen = false;
                frameDetails.Width = Constants.WidthDetailsSlim;
                frameDetails.Navigate(typeof(ConstructionSlim));
            }
            else
            {
                detailsIsOpen = true;
                frameDetails.Width = Constants.WidthDetailsNormal;
                frameDetails.Navigate(typeof(PanelModel));
            }

        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            frameDisplay.Width = this.ActualWidth;
            frameDisplay.Height = this.ActualHeight - Constants.HeightTitleBar;

            if (detailsIsOpen)
            {
                frameDetails.Width = Constants.WidthDetailsNormal;
            }
            else
            {
                frameDetails.Width = Constants.WidthDetailsSlim;
            }
            frameDetails.Height = this.ActualHeight - Constants.HeightTitleBar;
        }
    }
}
