using Avalonia.Controls;
using Avalonia.Rendering.Composition;
using System;

namespace TetrifactClient
{
    internal class AnimationHelper
    {
        public static void SlideInFromLeft(Control control, double slideAmount) 
        {
            CompositionVisual compositionVisual = ElementComposition.GetElementVisual(control);

            Vector3KeyFrameAnimation animation = compositionVisual.Compositor.CreateVector3KeyFrameAnimation();
            // Change the offset of the visual slightly to the left when the animation beginning
            var vector = new System.Numerics.Vector3
            {
                X = (float)compositionVisual.Offset.X,
                Y = (float)compositionVisual.Offset.Y,
                Z = (float)compositionVisual.Offset.Z
            };

            animation.InsertKeyFrame(0f, vector with
            {
                X = vector.X - (float)slideAmount
            });

            // Revert the offset to the original position (0,0,0) when the animation ends
            animation.InsertKeyFrame(1f, vector);
            animation.Duration = TimeSpan.FromMilliseconds(500);
            // Start the new animation!
            compositionVisual.StartAnimation("Offset", animation);
        }
    }
}
