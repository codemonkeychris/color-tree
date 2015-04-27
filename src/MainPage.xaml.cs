using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ColorTree
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        object topLevelSource;
        int topLevelSelectedIndex = 0;

        public MainPage()
        {
            this.InitializeComponent();
            gridView1.ItemClick += gridView1_ItemClick;
            gridView1.ContainerContentChanging += GridView1_ContainerContentChanging;
        }

        private void GridView1_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            FrameworkElement source = args.ItemContainer as FrameworkElement;
            if (args.Item is ColorEntry)
            {
                AutomationProperties.SetName(args.ItemContainer, ((ColorEntry)args.Item).name);
            }
            else
            {
                var i = (KeyValuePair<string, List<ColorEntry>>)args.Item;
                AutomationProperties.SetName(args.ItemContainer, i.Key);
            }
        }


        void gridView1_ItemClick(object sender, ItemClickEventArgs e)
        {
            // ignore clicks on child items
            if (gridView1.ItemsSource == topLevelSource)
            {
                topLevelSelectedIndex = gridView1.IndexFromContainer(gridView1.ContainerFromItem(e.ClickedItem));
                ShowItem(e.ClickedItem);
            }
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            gridView1.ItemsSource = topLevelSource = e.Parameter;
            topLevelSelectedIndex = 0;
        }

        void updateNarrator(string msg)
        {
            readme.Text = msg;
            FrameworkElementAutomationPeer.FromElement(readme).RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
        }

        async void ShowItem(object state, int selectedIndex = 0) 
        {
            // This simulates a long delay to load data... 
            //
             
            // first we show the progress UX and hide the grid
            //
            backButton.Click -= backButton_Click;
            gridView1.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            busyWait.Visibility = Windows.UI.Xaml.Visibility.Visible;
            busyWait.IsActive = true;
            updateNarrator("Loading data");

            // next we wait some time
            //
            await TimerTask.Wait(3000);

            // this is the real work, we update the data source of the list
            //
            if (state != null && !(state is ColorEntry || state is Dictionary<string, List<ColorEntry>>))
            {
                var topLevelClicked = (KeyValuePair<string, List<ColorEntry>>)state;
                gridView1.ItemsSource = topLevelClicked.Value;
                if (topLevelClicked.Value.Count == 0)
                {
                    updateNarrator("No data found");
                }
            }
            else if (state != null)
            {
                gridView1.ItemsSource = topLevelSource;
            }

            // finally we clean up the progress ux and show the grid
            //
            backButton.Click += backButton_Click;
            gridView1.Visibility = Windows.UI.Xaml.Visibility.Visible;
            busyWait.IsActive = false;
            busyWait.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            if (gridView1.Items.Count > selectedIndex)
            {
                gridView1.ScrollIntoView(gridView1.Items[selectedIndex]);
                gridView1.UpdateLayout();
                var item = (Control)gridView1.ContainerFromIndex(selectedIndex);
                item.Focus(FocusState.Programmatic);
            }
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            ShowItem(topLevelSource, topLevelSelectedIndex);
        }
    }

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

    // A common pattern is to re-use a ListView/GridView to contain multiple levels of hierarchy
    // so in this example we use a template selector to pick which item we are display and return
    // the right template
    //
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
 
}
