using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls;

namespace RockChat.UWP.Common
{
    public class NineGridPanel : Panel
    {
        public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(
            nameof(Padding), typeof(double), typeof(NineGridPanel), new PropertyMetadata(8d));

        public double Padding
        {
            get => (double) GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var currentY = 0D;
            var currentX = 0D;
            if (Children.Count == 1)
            {
                foreach (var child in Children)
                {
                    var bounds = new Rect(currentX, currentY, finalSize.Width, child.DesiredSize.Height);
                    child.Arrange(bounds);
                }

            }
            else
            {
                var itemSize = Math.Max(finalSize.Width / 3D - Padding, 0);
                foreach (var child in Children)
                {
                    var bounds = new Rect(currentX, currentY, itemSize, itemSize);
                    child.Arrange(bounds);

                    currentX += itemSize + Padding;
                    if (currentX >= finalSize.Width)
                    {
                        currentX = 0D;
                        currentY += itemSize + Padding;
                    }
                }

            }
            return finalSize;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var itemSize = Math.Max(availableSize.Width / 3D - Padding, 0);
            var rowCount = Math.Ceiling(Convert.ToDouble(Children.Count) / 3D);
            if (rowCount == 0d)
            {
                return new Size(0d, 0d);
            }

            var totalHeight = rowCount * itemSize + (rowCount - 1) * Padding;
            var requestWidth = availableSize.Width;
            var requestHeight = totalHeight;
            if (Children.Count == 1)
            {
                requestHeight = itemSize * 2;
                foreach (var child in Children)
                {
                    child.Measure(new Size(availableSize.Width, itemSize));
                    if (child.DesiredSize.Width != 0)
                    {
                        requestWidth = child.DesiredSize.Width;
                    }
                    if (child.DesiredSize.Height != 0d && child.DesiredSize.Height < requestHeight)
                    {
                        requestHeight = child.DesiredSize.Height;
                    }
                }
            }
            else
            {
                foreach (var child in Children)
                {
                    child.Measure(new Size(itemSize, itemSize));
                }
            }

            if (double.IsPositiveInfinity(requestHeight) || double.IsPositiveInfinity(requestWidth))
            {
                return new Size(0, 0);
            }
            return new Size(requestWidth, requestHeight);
        }
    }
}
