﻿using Avalonia.Controls;
using System;

namespace TetrifactClient
{
    public static  class FormHelper
    {
        /// <summary>
        /// Centers a window on a parent window, centering and size it down relative from parent.
        /// </summary>
        /// <param name="currentWindow"></param>
        /// <param name="parentWindow"></param>
        public static void CenterOn(this Window currentWindow, Window parentWindow, bool fitSize = false, int fitBufferMargin = 100)
        {
            if (fitSize) 
            {
                currentWindow.Width = parentWindow.Width - fitBufferMargin;
                currentWindow.Height = parentWindow.Height - fitBufferMargin;
            }

            int x = Convert.ToInt32(parentWindow.Position.X + (parentWindow.Width / 2) - (currentWindow.Width / 2));
            int y = Convert.ToInt32(parentWindow.Position.Y + (parentWindow.Height / 2) - (currentWindow.Height / 2));

            currentWindow.Position = new Avalonia.PixelPoint(x, y);
        }
    }
}
