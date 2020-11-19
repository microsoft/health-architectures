// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CoreGraphics;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Material.iOS;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Frame), typeof(MaterialFrameRenderer))]

namespace Contoso.IoMT_Client.iOS
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