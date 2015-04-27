# ColorTree demo

* Demo prep:
```
git clone https://github.com/codemonkeychris/color-tree.git color-tree
git checkout demo-start
start src\ColorTree.sln
```
    
* Baseline accessibility
    * F5
    * Show UX
    * Highlight problems
        * Extra goop read on each line
        * No notification of data loading
        * Focus isn't restore
        
        
* Fix what is read for each item
    * Wire up listener (in constructor)
    ```C#
    gridView1.ContainerContentChanging += GridView1_ContainerContentChanging;
    ```
    * Implement method
    ```C#
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
    ```
* Notify user of loading data
    * Add invisible element (not Visibility=Collapsed)
    ```XAML
    <TextBlock Opacity="0" Visibility="Visible" x:Name="readme" AutomationProperties.LiveSetting="Polite" /> 
    ```
    * Add method to update
    ```C#    
    void updateNarrator(string msg)  
    {  
        readme.Text = msg;  
        FrameworkElementAutomationPeer.FromElement(readme).RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);  
    }
    ```  
    * Update user at key points
    ```C#
    updateNarrator("Loading data");
    ```
* Restore focus (solves a couple problems)
    * Basic notification
    ```C#
    if (gridView1.Items.Count > selectedIndex)  
    {  
        gridView1.ScrollIntoView(gridView1.Items[selectedIndex]);  
        gridView1.UpdateLayout();  
        var item = (Control)gridView1.ContainerFromIndex(selectedIndex);  
        item.Focus(FocusState.Programmatic);  
    }  
    ```
    * Don't forget special case of zero items
    ```C#
    if (topLevelClicked.Value.Count == 0)  
    {  
        updateNarrator("No data found");  
    }  
    ```
    
