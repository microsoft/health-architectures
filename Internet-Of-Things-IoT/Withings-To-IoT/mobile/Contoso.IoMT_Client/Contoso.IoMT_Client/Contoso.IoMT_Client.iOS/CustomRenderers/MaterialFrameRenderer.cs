// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CoreGraphics;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Material.iOS;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Frame), typeof(MaterialFrameRenderer))]

#pragma warning disable SA1300 // Element should begin with upper-case letter
namespace Contoso.IoMT_Client.iOS
#pragma warning restore SA1300 // Element should begin with upper-case letter
{
    public class MaterialFrameRenderer : FrameRenderer
    {
        public override void Draw(CGRect rect)
        {
            base.Draw(rect);

            Layer.ShadowRadius = 2.0f;
            Layer.ShadowOffset = new CGSize(2, 2);
            Layer.ShadowColor = UIColor.Gray.CGColor;
            Layer.ShadowOpacity = 0.80f;
            Layer.ShadowPath = UIBezierPath.FromRect(Layer.Bounds).CGPath;
            Layer.MasksToBounds = false;
        }
    }
}