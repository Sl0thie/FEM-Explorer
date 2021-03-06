using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace FEM_Explorer
{
    public sealed partial class SliderExpValue : UserControl
    {
        public event EventHandler ValueChanged;

        private string title;
        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        private string displayValue;
        public string DisplayValue
        {
            get { return displayValue; }
            set
            {
                displayValue = value;
                textBlock_Value.Text = displayValue;
            }
        }

        private double minValue;
        public double MinValue
        {
            get { return minValue; }
            set
            {
                minValue = value;
                Slider_Primary.Minimum = value;
            }
        }

        private double maxValue;
        public double MaxValue
        {
            get { return maxValue; }
            set
            {

                maxValue = value;
                Slider_Primary.Maximum = value;
            }
        }

        private double newValue;
        public double NewValue
        {
            get { return newValue; }
            set { newValue = value; }
        }

        private double oldValue;
        public double OldValue
        {
            get { return oldValue; }
            set
            {
                oldValue = value;


            }
        }




        public SliderExpValue()
        {
            this.InitializeComponent();
        }

        //public void SetSliderValue(int newValue)
        //{
        //    Slider_Primary.Value = newValue;
        //}

        private void Slider_Primary_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            newValue = Slider_Primary.Value;
            if (newValue == oldValue)
            {

            }
            else
            {
                //Debug.WriteLine("Firing Event " + Slider_Primary.Value);

                oldValue = newValue;
                ValueChanged?.Invoke(this, new EventArgs());
            }

        }

        private void textBlock_Minus_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Slider_Primary.Value > Slider_Primary.Minimum)
            {
                Slider_Primary.Value--;
            }
        }

        private void textBlock_Plus_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Slider_Primary.Value < Slider_Primary.Maximum)
            {
                Slider_Primary.Value++;
            }
        }
    }
}
