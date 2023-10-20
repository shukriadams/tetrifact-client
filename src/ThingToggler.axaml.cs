using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Rendering.Composition;
using System;

namespace TetrifactClient
{
    public partial class ThingToggler : UserControl
    {
        public ThingToggler()
        {
            InitializeComponent();
        }

        private bool _isState1 = true;

        public void ToggleState(object? sender, RoutedEventArgs args)
        {
            _isState1 = !_isState1;

            if (_isState1)
            {
                State1.IsVisible = true;
                State2.IsVisible = false;
                //App.model.Caption = DateTime.Now.ToString();

                AnimationHelper.SlideInFromLeft(State1, 100);
            }
            else
            {
                State1.IsVisible = false;
                State2.IsVisible = true;

                AnimationHelper.SlideInFromLeft(State2, 100);
            }
        }
    }
}
