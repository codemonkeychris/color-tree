using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ColorTree
{
    public class TopLevelEntryColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var item = (KeyValuePair<string, List<ColorEntry>>)value;
            var color = item.Value.FirstOrDefault();
            if (color == null)
            {
                var c = new GradientStopCollection();
                c.Add(new GradientStop() { Color = Colors.White, Offset = 0 });
                c.Add(new GradientStop() { Color = Colors.Black, Offset = 1 });
                return new LinearGradientBrush(c, 45);
            }
            else
            {
                return new SolidColorBrush(Color.FromArgb(255, color.r, color.g, color.b));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class EntryColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var color = value as ColorEntry;
            if (color == null)
            {
                var c = new GradientStopCollection();
                c.Add(new GradientStop() { Color = Colors.White, Offset = 0 });
                c.Add(new GradientStop() { Color = Colors.Black, Offset = 1 });
                return new LinearGradientBrush(c, 45);
            }
            else
            {
                return new SolidColorBrush(Color.FromArgb(255, color.r, color.g, color.b));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ColorTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Entry { get; set; }
        public DataTemplate TopLevel { get; set; }
 
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is ColorEntry)
            {
                return Entry;
            }
            else
            {
                return TopLevel;
            }
        }
    }
 

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        object topLevelSource;
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            gridView1.ItemsSource = topLevelSource = e.Parameter;
        }

        private void gridView1_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem != null && !(e.ClickedItem is ColorEntry))
            {
                var topLevelClicked = (KeyValuePair<string, List<ColorEntry>>)e.ClickedItem;
                gridView1.ItemsSource = topLevelClicked.Value;
            }
            else if (e.ClickedItem != null)
            {
                gridView1.ItemsSource = topLevelSource;
            }
        }
    }
}
