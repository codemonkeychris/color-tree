using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ColorTree
{
    public class PrepareContainerEventArgs
    {
        public DependencyObject element;
        public object item;
    }

    public class GridView2 : GridView
    {
        public event EventHandler<PrepareContainerEventArgs> PrepareContainer;

        protected override void PrepareContainerForItemOverride(Windows.UI.Xaml.DependencyObject element, object item)
        {
            if (PrepareContainer != null)
            {
                PrepareContainer(this, new PrepareContainerEventArgs() { element = element, item = item });
            }
            base.PrepareContainerForItemOverride(element, item);
        }
    }
}
