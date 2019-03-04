using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApplication12
{
    /// <summary>
    /// A wrap panel that stretches all items with vertical alignment <see cref="VerticalAlignment.Stretch"/> vertically,
    /// when all items can be placed in one line.
    /// </summary>
    public class StretchWrapPanel : Panel
    {
        /// <summary>
        /// When overridden in a derived class, measures the size in layout required for child elements and
        /// determines a size for the <see cref="FrameworkElement" />-derived class.
        /// </summary>
        /// <returns>The size that this element determines it needs during layout, based on its calculations of child element sizes.</returns>
        /// <param name="availableSize">
        /// The available size that this element can give to child elements.
        /// Infinity can be specified as a value to indicate that the element will size to whatever content is available.
        /// </param>
        protected override Size MeasureOverride(Size availableSize)
        {
            Size currentLineSize = new Size();
            Size panelSize = new Size();

            UIElementCollection children = InternalChildren;

            for (int i = 0; i < children.Count; i++)
            {
                UIElement child = children[i];

                // Flow passes its own constraint to children
                child.Measure(availableSize);
                Size size = child.DesiredSize;

                if (currentLineSize.Width + size.Width > availableSize.Width) //need to switch to another line
                {
                    panelSize.Width = Math.Max(currentLineSize.Width, panelSize.Width);
                    panelSize.Height += currentLineSize.Height;
                    currentLineSize = size;

                    if (size.Width > availableSize.Width) // if the element is wider then the constraint - give it a separate line
                    {
                        panelSize.Width = Math.Max(size.Width, panelSize.Width);
                        panelSize.Height += size.Height;
                        currentLineSize = new Size();
                    }
                }
                else //continue to accumulate a line
                {
                    currentLineSize.Width += size.Width;
                    currentLineSize.Height = Math.Max(size.Height, currentLineSize.Height);
                }
            }

            // the last line size, if any need to be added
            panelSize.Width = Math.Max(currentLineSize.Width, panelSize.Width);
            panelSize.Height += currentLineSize.Height;

            return panelSize;
        }

        /// <summary>
        /// When overridden in a derived class, positions child elements and determines a size for a <see cref="FrameworkElement" /> derived class.
        /// </summary>
        /// <returns>The actual size used.</returns>
        /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (ChildrenFitInOneLine(finalSize))
                ArrangeChildrenInSingleLine();
            else
                finalSize = ArrangeChildrenInMultipleLines(finalSize);

            return finalSize;
        }

        private Size ArrangeChildrenInMultipleLines(Size finalSize)
        {
            int firstInLine = 0;
            Size currentLineSize = new Size();
            double accumulatedHeight = 0;
            int lineCount = 1;

            for (int i = 0; i < InternalChildren.Count; i++)
            {
                Size childSize = InternalChildren[i].DesiredSize;

                if (currentLineSize.Width + childSize.Width > finalSize.Width) //need to switch to another line
                {
                    ArrangeLine(accumulatedHeight, currentLineSize, firstInLine, i);

                    accumulatedHeight += currentLineSize.Height;
                    currentLineSize = childSize;

                    if (childSize.Width > finalSize.Width) //the element is wider then the constraint - give it a separate line
                    {
                        ArrangeLine(accumulatedHeight, childSize, i, ++i);
                        accumulatedHeight += childSize.Height;
                        currentLineSize = new Size();
                    }
                    firstInLine = i;
                    lineCount++;
                }
                else //continue to accumulate a line
                {
                    currentLineSize.Width += childSize.Width;
                    currentLineSize.Height = Math.Max(childSize.Height, currentLineSize.Height);
                }
            }

            if (firstInLine < InternalChildren.Count)
                ArrangeLine(accumulatedHeight, currentLineSize, firstInLine, InternalChildren.Count);

            return finalSize;
        }

        private void ArrangeChildrenInSingleLine()
        {
            double x = 0.0;
            for (int i = 0; i < InternalChildren.Count; i++)
            {
                InternalChildren[i].Arrange(new Rect(x, 0, InternalChildren[i].DesiredSize.Width, this.RenderSize.Height));
                x += InternalChildren[i].DesiredSize.Width;
            }
        }

        private bool ChildrenFitInOneLine(Size finalSize) => InternalChildren.Cast<UIElement>().Sum(c => c.DesiredSize.Width) <= finalSize.Width;

        private void ArrangeLine(double y, Size lineSize, int start, int end)
        {
            UIElementCollection children = InternalChildren;

            double x = 0.0;
            for (int i = start; i < end; i++)
            {
                children[i].Arrange(new Rect(x, y, children[i].DesiredSize.Width, lineSize.Height));
                x += children[i].DesiredSize.Width;
            }
        }
    }
}