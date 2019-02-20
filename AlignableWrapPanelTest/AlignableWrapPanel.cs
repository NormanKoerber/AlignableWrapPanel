using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApplication12
{
    public class AlignableWrapPanel : Panel
    {
        protected override Size MeasureOverride(Size constraint)
        {
            Size curLineSize = new Size();
            Size panelSize = new Size();

            UIElementCollection children = InternalChildren;

            for (int i = 0; i < children.Count; i++)
            {
                UIElement child = children[i];

                // Flow passes its own constraint to children
                child.Measure(constraint);
                Size sz = child.DesiredSize;

                if (curLineSize.Width + sz.Width > constraint.Width) //need to switch to another line
                {
                    panelSize.Width = Math.Max(curLineSize.Width, panelSize.Width);
                    panelSize.Height += curLineSize.Height;
                    curLineSize = sz;

                    if (sz.Width > constraint.Width) // if the element is wider then the constraint - give it a separate line
                    {
                        panelSize.Width = Math.Max(sz.Width, panelSize.Width);
                        panelSize.Height += sz.Height;
                        curLineSize = new Size();
                    }
                }
                else //continue to accumulate a line
                {
                    curLineSize.Width += sz.Width;
                    curLineSize.Height = Math.Max(sz.Height, curLineSize.Height);
                }
            }

            // the last line size, if any need to be added
            panelSize.Width = Math.Max(curLineSize.Width, panelSize.Width);
            panelSize.Height += curLineSize.Height;

            return panelSize;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            int firstInLine = 0;
            Size currentLineSize = new Size();
            double accumulatedHeight = 0;
            UIElementCollection children = InternalChildren;
            int lineCount = 1;

            for (int i = 0; i < children.Count; i++)
            {
                Size childSize = children[i].DesiredSize;

                if (currentLineSize.Width + childSize.Width > arrangeBounds.Width) //need to switch to another line
                {
                    ArrangeLine(accumulatedHeight, currentLineSize, firstInLine, i);

                    accumulatedHeight += currentLineSize.Height;
                    currentLineSize = childSize;

                    if (childSize.Width > arrangeBounds.Width) //the element is wider then the constraint - give it a separate line
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

            if (firstInLine < children.Count)
                ArrangeLine(accumulatedHeight, currentLineSize, firstInLine, children.Count);

            if (lineCount == 1)
            {
                double x = 0.0;
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].Arrange(new Rect(x, 0, children[i].DesiredSize.Width, this.RenderSize.Height));
                    x += children[i].DesiredSize.Width;
                }
            }

            return arrangeBounds;
        }

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