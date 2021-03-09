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
    public sealed partial class LogSlider : UserControl
    {
        public event EventHandler ValueChanged;
        public event EventHandler Checked;
        public event EventHandler Unchecked;

        public LogSlider()
        {
            this.InitializeComponent();
        }

        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                textBlock_Title.Content = title;
            }
        }

        private string displayValue;
        public string DisplayValue
        {
            get { return displayValue; }
            set
            {
                displayValue = value;
                textBlock_Value.Text = value;
            }
        }


        private bool BlockUpdate = false;


        internal void SetNewValue(float newValue)
        {
            BlockUpdate = true;
            if (newValue <= 0.00001f) { slider_Value.Value = 1; }
            else if (newValue <= 0.00002f) { slider_Value.Value = 2; }
            else if (newValue <= 0.00003f) { slider_Value.Value = 3; }
            else if (newValue <= 0.00004f) { slider_Value.Value = 4; }
            else if (newValue <= 0.00005f) { slider_Value.Value = 5; }
            else if (newValue <= 0.00006f) { slider_Value.Value = 6; }
            else if (newValue <= 0.00007f) { slider_Value.Value = 7; }
            else if (newValue <= 0.00008f) { slider_Value.Value = 8; }
            else if (newValue <= 0.00009f) { slider_Value.Value = 9; }
            else if (newValue <= 0.0001f) { slider_Value.Value = 10; }
            else if (newValue <= 0.0002f) { slider_Value.Value = 11; }
            else if (newValue <= 0.0003f) { slider_Value.Value = 12; }
            else if (newValue <= 0.0004f) { slider_Value.Value = 13; }
            else if (newValue <= 0.0005f) { slider_Value.Value = 14; }
            else if (newValue <= 0.0006f) { slider_Value.Value = 15; }
            else if (newValue <= 0.0007f) { slider_Value.Value = 16; }
            else if (newValue <= 0.0008f) { slider_Value.Value = 17; }
            else if (newValue <= 0.0009f) { slider_Value.Value = 18; }
            else if (newValue <= 0.001f) { slider_Value.Value = 19; }
            else if (newValue <= 0.002f) { slider_Value.Value = 20; }
            else if (newValue <= 0.003f) { slider_Value.Value = 21; }
            else if (newValue <= 0.004f) { slider_Value.Value = 22; }
            else if (newValue <= 0.005f) { slider_Value.Value = 23; }
            else if (newValue <= 0.006f) { slider_Value.Value = 24; }
            else if (newValue <= 0.007f) { slider_Value.Value = 25; }
            else if (newValue <= 0.008f) { slider_Value.Value = 26; }
            else if (newValue <= 0.009f) { slider_Value.Value = 27; }
            else if (newValue <= 0.01f) { slider_Value.Value = 28; }
            else if (newValue <= 0.02f) { slider_Value.Value = 29; }
            else if (newValue <= 0.03f) { slider_Value.Value = 30; }
            else if (newValue <= 0.04f) { slider_Value.Value = 31; }
            else if (newValue <= 0.05f) { slider_Value.Value = 32; }
            else if (newValue <= 0.06f) { slider_Value.Value = 33; }
            else if (newValue <= 0.07f) { slider_Value.Value = 34; }
            else if (newValue <= 0.08f) { slider_Value.Value = 35; }
            else if (newValue <= 0.09f) { slider_Value.Value = 36; }
            else if (newValue <= 0.1f) { slider_Value.Value = 37; }
            else if (newValue <= 0.2f) { slider_Value.Value = 38; }
            else if (newValue <= 0.3f) { slider_Value.Value = 39; }
            else if (newValue <= 0.4f) { slider_Value.Value = 40; }
            else if (newValue <= 0.5f) { slider_Value.Value = 41; }
            else if (newValue <= 0.6f) { slider_Value.Value = 42; }
            else if (newValue <= 0.7f) { slider_Value.Value = 43; }
            else if (newValue <= 0.7f) { slider_Value.Value = 44; }
            else if (newValue <= 0.8f) { slider_Value.Value = 45; }
            else if (newValue <= 0.9f) { slider_Value.Value = 46; }
            else if (newValue <= 1f) { slider_Value.Value = 47; }
            else if (newValue <= 2f) { slider_Value.Value = 48; }
            else if (newValue <= 3f) { slider_Value.Value = 49; }
            else if (newValue <= 4f) { slider_Value.Value = 50; }
            else if (newValue <= 5f) { slider_Value.Value = 51; }
            else if (newValue <= 6f) { slider_Value.Value = 52; }
            else if (newValue <= 7f) { slider_Value.Value = 53; }
            else if (newValue <= 8f) { slider_Value.Value = 54; }
            else if (newValue <= 9f) { slider_Value.Value = 55; }
            else if (newValue <= 10f) { slider_Value.Value = 56; }
            else if (newValue <= 20f) { slider_Value.Value = 57; }
            else if (newValue <= 30f) { slider_Value.Value = 58; }
            else if (newValue <= 40f) { slider_Value.Value = 59; }
            else if (newValue <= 50f) { slider_Value.Value = 60; }
            else if (newValue <= 60f) { slider_Value.Value = 61; }
            else if (newValue <= 70f) { slider_Value.Value = 62; }
            else if (newValue <= 80f) { slider_Value.Value = 63; }
            else if (newValue <= 90f) { slider_Value.Value = 64; }
            else if (newValue <= 100f) { slider_Value.Value = 65; }
            else if (newValue <= 200f) { slider_Value.Value = 66; }
            else if (newValue <= 300f) { slider_Value.Value = 67; }
            else if (newValue <= 400f) { slider_Value.Value = 68; }
            else if (newValue <= 500f) { slider_Value.Value = 69; }
            else if (newValue <= 600f) { slider_Value.Value = 70; }
            else if (newValue <= 700f) { slider_Value.Value = 71; }
            else if (newValue <= 800f) { slider_Value.Value = 72; }
            else if (newValue <= 900f) { slider_Value.Value = 73; }
            else if (newValue <= 1000f) { slider_Value.Value = 74; }
            else if (newValue <= 2000f) { slider_Value.Value = 75; }
            else if (newValue <= 3000f) { slider_Value.Value = 76; }
            else if (newValue <= 4000f) { slider_Value.Value = 77; }
            else if (newValue <= 5000f) { slider_Value.Value = 78; }
            else if (newValue <= 6000f) { slider_Value.Value = 79; }
            else if (newValue <= 7000f) { slider_Value.Value = 80; }
            else if (newValue <= 8000f) { slider_Value.Value = 81; }
            else if (newValue <= 9000f) { slider_Value.Value = 82; }
            else if (newValue <= 10000f) { slider_Value.Value = 83; }
            else if (newValue <= 20000f) { slider_Value.Value = 84; }
            else if (newValue <= 30000f) { slider_Value.Value = 85; }
            else if (newValue <= 40000f) { slider_Value.Value = 86; }
            else if (newValue <= 50000f) { slider_Value.Value = 87; }
            else if (newValue <= 60000f) { slider_Value.Value = 88; }
            else if (newValue <= 70000f) { slider_Value.Value = 89; }
            else if (newValue <= 80000f) { slider_Value.Value = 90; }
            else if (newValue <= 90000f) { slider_Value.Value = 91; }
            else if (newValue <= 100000f) { slider_Value.Value = 92; }

            BlockUpdate = false;
        }

        private float theValue;
        public float TheValue
        {
            get { return theValue; }
            set { theValue = value; }
        }

        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                textBlock_Title.IsChecked = value;
            }
        }

        private void slider_Value_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            


                switch ((int)slider_Value.Value)
                {
                    case 1: theValue = 0.00001f; break;
                    case 2: theValue = 0.00002f; break;
                    case 3: theValue = 0.00003f; break;
                    case 4: theValue = 0.00004f; break;
                    case 5: theValue = 0.00005f; break;
                    case 6: theValue = 0.00006f; break;
                    case 7: theValue = 0.00007f; break;
                    case 8: theValue = 0.00008f; break;
                    case 9: theValue = 0.00009f; break;
                    case 10: theValue = 0.0001f; break;
                    case 11: theValue = 0.0002f; break;
                    case 12: theValue = 0.0003f; break;
                    case 13: theValue = 0.0004f; break;
                    case 14: theValue = 0.0005f; break;
                    case 15: theValue = 0.0006f; break;
                    case 16: theValue = 0.0007f; break;
                    case 17: theValue = 0.0008f; break;
                    case 18: theValue = 0.0009f; break;
                    case 19: theValue = 0.001f; break;
                    case 20: theValue = 0.002f; break;
                    case 21: theValue = 0.003f; break;
                    case 22: theValue = 0.004f; break;
                    case 23: theValue = 0.005f; break;
                    case 24: theValue = 0.006f; break;
                    case 25: theValue = 0.007f; break;
                    case 26: theValue = 0.008f; break;
                    case 27: theValue = 0.009f; break;
                    case 28: theValue = 0.01f; break;
                    case 29: theValue = 0.02f; break;
                    case 30: theValue = 0.03f; break;
                    case 31: theValue = 0.04f; break;
                    case 32: theValue = 0.05f; break;
                    case 33: theValue = 0.06f; break;
                    case 34: theValue = 0.07f; break;
                    case 35: theValue = 0.08f; break;
                    case 36: theValue = 0.09f; break;
                    case 37: theValue = 0.1f; break;
                    case 38: theValue = 0.2f; break;
                    case 39: theValue = 0.3f; break;
                    case 40: theValue = 0.4f; break;
                    case 41: theValue = 0.5f; break;
                    case 42: theValue = 0.6f; break;
                    case 43: theValue = 0.7f; break;
                    case 44: theValue = 0.7f; break;
                    case 45: theValue = 0.8f; break;
                    case 46: theValue = 0.9f; break;
                    case 47: theValue = 1f; break;
                    case 48: theValue = 2f; break;
                    case 49: theValue = 3f; break;
                    case 50: theValue = 4f; break;
                    case 51: theValue = 5f; break;
                    case 52: theValue = 6f; break;
                    case 53: theValue = 7f; break;
                    case 54: theValue = 8f; break;
                    case 55: theValue = 9f; break;
                    case 56: theValue = 10f; break;
                    case 57: theValue = 20f; break;
                    case 58: theValue = 30f; break;
                    case 59: theValue = 40f; break;
                    case 60: theValue = 50f; break;
                    case 61: theValue = 60f; break;
                    case 62: theValue = 70f; break;
                    case 63: theValue = 80f; break;
                    case 64: theValue = 90f; break;
                    case 65: theValue = 100f; break;
                    case 66: theValue = 200f; break;
                    case 67: theValue = 300f; break;
                    case 68: theValue = 400f; break;
                    case 69: theValue = 500f; break;
                    case 70: theValue = 600f; break;
                    case 71: theValue = 700f; break;
                    case 72: theValue = 800f; break;
                    case 73: theValue = 900f; break;
                    case 74: theValue = 1000f; break;
                    case 75: theValue = 2000f; break;
                    case 76: theValue = 3000f; break;
                    case 77: theValue = 4000f; break;
                    case 78: theValue = 5000f; break;
                    case 79: theValue = 6000f; break;
                    case 80: theValue = 7000f; break;
                    case 81: theValue = 8000f; break;
                    case 82: theValue = 9000f; break;
                    case 83: theValue = 10000f; break;
                    case 84: theValue = 20000f; break;
                    case 85: theValue = 30000f; break;
                    case 86: theValue = 40000f; break;
                    case 87: theValue = 50000f; break;
                    case 88: theValue = 60000f; break;
                    case 89: theValue = 70000f; break;
                    case 90: theValue = 80000f; break;
                    case 91: theValue = 90000f; break;
                    case 92: theValue = 100000f; break;
                }

            textBlock_Value.Text = "x" + theValue.ToString();

            if (!BlockUpdate)
            {            
                ValueChanged?.Invoke(this, new EventArgs());
            }
        }

        private void textBlock_Minus_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (slider_Value.Value > 1) { slider_Value.Value = slider_Value.Value - 1; }
        }

        private void textBlock_Plus_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (slider_Value.Value < 92) { slider_Value.Value = slider_Value.Value + 1; }
        }

        private void textBlock_Title_Checked(object sender, RoutedEventArgs e)
        {
            isChecked = true;
            Checked?.Invoke(this, new EventArgs());
        }

        private void textBlock_Title_Unchecked(object sender, RoutedEventArgs e)
        {
            isChecked = false;
            Unchecked?.Invoke(this, new EventArgs());
        }
    }
}

