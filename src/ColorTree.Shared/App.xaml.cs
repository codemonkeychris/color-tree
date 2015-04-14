﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace ColorTree
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
#endif

        public static IAsyncAction BeginInvoke(DispatchedHandler callback)
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            return dispatcher.RunAsync(CoreDispatcherPriority.Normal, callback);
        }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;

            LoadData();
        }

        string rgbToHex(int r, int g, int b)
        {
            return String.Format("#{0:x2}{1:x2}{2:x2}", r, g, b);
        }

        private async void LoadData()
        {
            var file = await GetPackagedFile(null, "satfaces.txt");
            var content = await FileIO.ReadLinesAsync(file);
            var rexp = new Regex(@"\[\s*([0-9]+),\s*([0-9]+),\s*([0-9]+)\s*]\s*([a-zA-Z ]+)");
            var colorTree = new Dictionary<string, List<ColorEntry>>();

            var hexIndex = new Dictionary<string, List<ColorEntry>>();
            for (int i = 0; i < rawData.Length; i++)
            {
                List<ColorEntry> entries;
                if (!hexIndex.TryGetValue(rawData[i].hexCode, out entries))
                {
                    entries = new List<ColorEntry>();
                    hexIndex[rawData[i].hexCode] = entries;
                }

                var item = rawData[i];
                item.r = byte.Parse(item.hexCode.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
                item.g = byte.Parse(item.hexCode.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                item.b = byte.Parse(item.hexCode.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);

                entries.Add(rawData[i]);
            }


            foreach (var line in content)
            {
                var parsed = rexp.Match(line);
                int r = int.Parse(parsed.Groups[1].Value);
                int g = int.Parse(parsed.Groups[2].Value);
                int b = int.Parse(parsed.Groups[3].Value);
                string name = parsed.Groups[4].Value;

                List<ColorEntry> entries;
                if (!colorTree.TryGetValue(name, out entries))
                {
                    entries = new List<ColorEntry>();
                    colorTree[name] = entries;
                }

                string hex = rgbToHex(r, g, b);

                List<ColorEntry> found;
                if (hexIndex.TryGetValue(hex, out found))
                {
                    entries.AddRange(found);
                }
            }

            await BeginInvoke(() =>
            {
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(MainPage), colorTree);
            });
        }

        private async Task<StorageFile> GetPackagedFile(string folderName, string fileName)
        {
            StorageFolder installFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;

            if (folderName != null)
            {
                StorageFolder subFolder = await installFolder.GetFolderAsync(folderName);
                return await subFolder.GetFileAsync(fileName);
            }
            else
            {
                return await installFolder.GetFileAsync(fileName);
            }
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // TODO: change this value to a cache size that is appropriate for your application
                rootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(splash), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }
#endif

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        static ColorEntry[] rawData = {
            // I love XKCD: http://blog.xkcd.com/2010/05/03/color-survey-results/
            // License: http://creativecommons.org/publicdomain/zero/1.0/
            new ColorEntry { name = "cloudy blue", hexCode = "#acc2d9" },
            new ColorEntry { name = "dark pastel green", hexCode = "#56ae57" },
            new ColorEntry { name = "dust", hexCode = "#b2996e" },
            new ColorEntry { name = "electric lime", hexCode = "#a8ff04" },
            new ColorEntry { name = "fresh green", hexCode = "#69d84f" },
            new ColorEntry { name = "light eggplant", hexCode = "#894585" },
            new ColorEntry { name = "nasty green", hexCode = "#70b23f" },
            new ColorEntry { name = "really light blue", hexCode = "#d4ffff" },
            new ColorEntry { name = "tea", hexCode = "#65ab7c" },
            new ColorEntry { name = "warm purple", hexCode = "#952e8f" },
            new ColorEntry { name = "yellowish tan", hexCode = "#fcfc81" },
            new ColorEntry { name = "cement", hexCode = "#a5a391" },
            new ColorEntry { name = "dark grass green", hexCode = "#388004" },
            new ColorEntry { name = "dusty teal", hexCode = "#4c9085" },
            new ColorEntry { name = "grey teal", hexCode = "#5e9b8a" },
            new ColorEntry { name = "macaroni and cheese", hexCode = "#efb435" },
            new ColorEntry { name = "pinkish tan", hexCode = "#d99b82" },
            new ColorEntry { name = "spruce", hexCode = "#0a5f38" },
            new ColorEntry { name = "strong blue", hexCode = "#0c06f7" },
            new ColorEntry { name = "toxic green", hexCode = "#61de2a" },
            new ColorEntry { name = "windows blue", hexCode = "#3778bf" },
            new ColorEntry { name = "blue blue", hexCode = "#2242c7" },
            new ColorEntry { name = "blue with a hint of purple", hexCode = "#533cc6" },
            new ColorEntry { name = "booger", hexCode = "#9bb53c" },
            new ColorEntry { name = "bright sea green", hexCode = "#05ffa6" },
            new ColorEntry { name = "dark green blue", hexCode = "#1f6357" },
            new ColorEntry { name = "deep turquoise", hexCode = "#017374" },
            new ColorEntry { name = "green teal", hexCode = "#0cb577" },
            new ColorEntry { name = "strong pink", hexCode = "#ff0789" },
            new ColorEntry { name = "bland", hexCode = "#afa88b" },
            new ColorEntry { name = "deep aqua", hexCode = "#08787f" },
            new ColorEntry { name = "lavender pink", hexCode = "#dd85d7" },
            new ColorEntry { name = "light moss green", hexCode = "#a6c875" },
            new ColorEntry { name = "light seafoam green", hexCode = "#a7ffb5" },
            new ColorEntry { name = "olive yellow", hexCode = "#c2b709" },
            new ColorEntry { name = "pig pink", hexCode = "#e78ea5" },
            new ColorEntry { name = "deep lilac", hexCode = "#966ebd" },
            new ColorEntry { name = "desert", hexCode = "#ccad60" },
            new ColorEntry { name = "dusty lavender", hexCode = "#ac86a8" },
            new ColorEntry { name = "purpley grey", hexCode = "#947e94" },
            new ColorEntry { name = "purply", hexCode = "#983fb2" },
            new ColorEntry { name = "candy pink", hexCode = "#ff63e9" },
            new ColorEntry { name = "light pastel green", hexCode = "#b2fba5" },
            new ColorEntry { name = "boring green", hexCode = "#63b365" },
            new ColorEntry { name = "kiwi green", hexCode = "#8ee53f" },
            new ColorEntry { name = "light grey green", hexCode = "#b7e1a1" },
            new ColorEntry { name = "orange pink", hexCode = "#ff6f52" },
            new ColorEntry { name = "tea green", hexCode = "#bdf8a3" },
            new ColorEntry { name = "very light brown", hexCode = "#d3b683" },
            new ColorEntry { name = "egg shell", hexCode = "#fffcc4" },
            new ColorEntry { name = "eggplant purple", hexCode = "#430541" },
            new ColorEntry { name = "powder pink", hexCode = "#ffb2d0" },
            new ColorEntry { name = "reddish grey", hexCode = "#997570" },
            new ColorEntry { name = "baby shit brown", hexCode = "#ad900d" },
            new ColorEntry { name = "liliac", hexCode = "#c48efd" },
            new ColorEntry { name = "stormy blue", hexCode = "#507b9c" },
            new ColorEntry { name = "ugly brown", hexCode = "#7d7103" },
            new ColorEntry { name = "custard", hexCode = "#fffd78" },
            new ColorEntry { name = "darkish pink", hexCode = "#da467d" },
            new ColorEntry { name = "deep brown", hexCode = "#410200" },
            new ColorEntry { name = "greenish beige", hexCode = "#c9d179" },
            new ColorEntry { name = "manilla", hexCode = "#fffa86" },
            new ColorEntry { name = "off blue", hexCode = "#5684ae" },
            new ColorEntry { name = "battleship grey", hexCode = "#6b7c85" },
            new ColorEntry { name = "browny green", hexCode = "#6f6c0a" },
            new ColorEntry { name = "bruise", hexCode = "#7e4071" },
            new ColorEntry { name = "kelley green", hexCode = "#009337" },
            new ColorEntry { name = "sickly yellow", hexCode = "#d0e429" },
            new ColorEntry { name = "sunny yellow", hexCode = "#fff917" },
            new ColorEntry { name = "azul", hexCode = "#1d5dec" },
            new ColorEntry { name = "darkgreen", hexCode = "#054907" },
            new ColorEntry { name = "green/yellow", hexCode = "#b5ce08" },
            new ColorEntry { name = "lichen", hexCode = "#8fb67b" },
            new ColorEntry { name = "light light green", hexCode = "#c8ffb0" },
            new ColorEntry { name = "pale gold", hexCode = "#fdde6c" },
            new ColorEntry { name = "sun yellow", hexCode = "#ffdf22" },
            new ColorEntry { name = "tan green", hexCode = "#a9be70" },
            new ColorEntry { name = "burple", hexCode = "#6832e3" },
            new ColorEntry { name = "butterscotch", hexCode = "#fdb147" },
            new ColorEntry { name = "toupe", hexCode = "#c7ac7d" },
            new ColorEntry { name = "dark cream", hexCode = "#fff39a" },
            new ColorEntry { name = "indian red", hexCode = "#850e04" },
            new ColorEntry { name = "light lavendar", hexCode = "#efc0fe" },
            new ColorEntry { name = "poison green", hexCode = "#40fd14" },
            new ColorEntry { name = "baby puke green", hexCode = "#b6c406" },
            new ColorEntry { name = "bright yellow green", hexCode = "#9dff00" },
            new ColorEntry { name = "charcoal grey", hexCode = "#3c4142" },
            new ColorEntry { name = "squash", hexCode = "#f2ab15" },
            new ColorEntry { name = "cinnamon", hexCode = "#ac4f06" },
            new ColorEntry { name = "light pea green", hexCode = "#c4fe82" },
            new ColorEntry { name = "radioactive green", hexCode = "#2cfa1f" },
            new ColorEntry { name = "raw sienna", hexCode = "#9a6200" },
            new ColorEntry { name = "baby purple", hexCode = "#ca9bf7" },
            new ColorEntry { name = "cocoa", hexCode = "#875f42" },
            new ColorEntry { name = "light royal blue", hexCode = "#3a2efe" },
            new ColorEntry { name = "orangeish", hexCode = "#fd8d49" },
            new ColorEntry { name = "rust brown", hexCode = "#8b3103" },
            new ColorEntry { name = "sand brown", hexCode = "#cba560" },
            new ColorEntry { name = "swamp", hexCode = "#698339" },
            new ColorEntry { name = "tealish green", hexCode = "#0cdc73" },
            new ColorEntry { name = "burnt siena", hexCode = "#b75203" },
            new ColorEntry { name = "camo", hexCode = "#7f8f4e" },
            new ColorEntry { name = "dusk blue", hexCode = "#26538d" },
            new ColorEntry { name = "fern", hexCode = "#63a950" },
            new ColorEntry { name = "old rose", hexCode = "#c87f89" },
            new ColorEntry { name = "pale light green", hexCode = "#b1fc99" },
            new ColorEntry { name = "peachy pink", hexCode = "#ff9a8a" },
            new ColorEntry { name = "rosy pink", hexCode = "#f6688e" },
            new ColorEntry { name = "light bluish green", hexCode = "#76fda8" },
            new ColorEntry { name = "light bright green", hexCode = "#53fe5c" },
            new ColorEntry { name = "light neon green", hexCode = "#4efd54" },
            new ColorEntry { name = "light seafoam", hexCode = "#a0febf" },
            new ColorEntry { name = "tiffany blue", hexCode = "#7bf2da" },
            new ColorEntry { name = "washed out green", hexCode = "#bcf5a6" },
            new ColorEntry { name = "browny orange", hexCode = "#ca6b02" },
            new ColorEntry { name = "nice blue", hexCode = "#107ab0" },
            new ColorEntry { name = "sapphire", hexCode = "#2138ab" },
            new ColorEntry { name = "greyish teal", hexCode = "#719f91" },
            new ColorEntry { name = "orangey yellow", hexCode = "#fdb915" },
            new ColorEntry { name = "parchment", hexCode = "#fefcaf" },
            new ColorEntry { name = "straw", hexCode = "#fcf679" },
            new ColorEntry { name = "very dark brown", hexCode = "#1d0200" },
            new ColorEntry { name = "terracota", hexCode = "#cb6843" },
            new ColorEntry { name = "ugly blue", hexCode = "#31668a" },
            new ColorEntry { name = "clear blue", hexCode = "#247afd" },
            new ColorEntry { name = "creme", hexCode = "#ffffb6" },
            new ColorEntry { name = "foam green", hexCode = "#90fda9" },
            new ColorEntry { name = "grey/green", hexCode = "#86a17d" },
            new ColorEntry { name = "light gold", hexCode = "#fddc5c" },
            new ColorEntry { name = "seafoam blue", hexCode = "#78d1b6" },
            new ColorEntry { name = "topaz", hexCode = "#13bbaf" },
            new ColorEntry { name = "violet pink", hexCode = "#fb5ffc" },
            new ColorEntry { name = "wintergreen", hexCode = "#20f986" },
            new ColorEntry { name = "yellow tan", hexCode = "#ffe36e" },
            new ColorEntry { name = "dark fuchsia", hexCode = "#9d0759" },
            new ColorEntry { name = "indigo blue", hexCode = "#3a18b1" },
            new ColorEntry { name = "light yellowish green", hexCode = "#c2ff89" },
            new ColorEntry { name = "pale magenta", hexCode = "#d767ad" },
            new ColorEntry { name = "rich purple", hexCode = "#720058" },
            new ColorEntry { name = "sunflower yellow", hexCode = "#ffda03" },
            new ColorEntry { name = "green/blue", hexCode = "#01c08d" },
            new ColorEntry { name = "leather", hexCode = "#ac7434" },
            new ColorEntry { name = "racing green", hexCode = "#014600" },
            new ColorEntry { name = "vivid purple", hexCode = "#9900fa" },
            new ColorEntry { name = "dark royal blue", hexCode = "#02066f" },
            new ColorEntry { name = "hazel", hexCode = "#8e7618" },
            new ColorEntry { name = "muted pink", hexCode = "#d1768f" },
            new ColorEntry { name = "booger green", hexCode = "#96b403" },
            new ColorEntry { name = "canary", hexCode = "#fdff63" },
            new ColorEntry { name = "cool grey", hexCode = "#95a3a6" },
            new ColorEntry { name = "dark taupe", hexCode = "#7f684e" },
            new ColorEntry { name = "darkish purple", hexCode = "#751973" },
            new ColorEntry { name = "true green", hexCode = "#089404" },
            new ColorEntry { name = "coral pink", hexCode = "#ff6163" },
            new ColorEntry { name = "dark sage", hexCode = "#598556" },
            new ColorEntry { name = "dark slate blue", hexCode = "#214761" },
            new ColorEntry { name = "flat blue", hexCode = "#3c73a8" },
            new ColorEntry { name = "mushroom", hexCode = "#ba9e88" },
            new ColorEntry { name = "rich blue", hexCode = "#021bf9" },
            new ColorEntry { name = "dirty purple", hexCode = "#734a65" },
            new ColorEntry { name = "greenblue", hexCode = "#23c48b" },
            new ColorEntry { name = "icky green", hexCode = "#8fae22" },
            new ColorEntry { name = "light khaki", hexCode = "#e6f2a2" },
            new ColorEntry { name = "warm blue", hexCode = "#4b57db" },
            new ColorEntry { name = "dark hot pink", hexCode = "#d90166" },
            new ColorEntry { name = "deep sea blue", hexCode = "#015482" },
            new ColorEntry { name = "carmine", hexCode = "#9d0216" },
            new ColorEntry { name = "dark yellow green", hexCode = "#728f02" },
            new ColorEntry { name = "pale peach", hexCode = "#ffe5ad" },
            new ColorEntry { name = "plum purple", hexCode = "#4e0550" },
            new ColorEntry { name = "golden rod", hexCode = "#f9bc08" },
            new ColorEntry { name = "neon red", hexCode = "#ff073a" },
            new ColorEntry { name = "old pink", hexCode = "#c77986" },
            new ColorEntry { name = "very pale blue", hexCode = "#d6fffe" },
            new ColorEntry { name = "blood orange", hexCode = "#fe4b03" },
            new ColorEntry { name = "grapefruit", hexCode = "#fd5956" },
            new ColorEntry { name = "sand yellow", hexCode = "#fce166" },
            new ColorEntry { name = "clay brown", hexCode = "#b2713d" },
            new ColorEntry { name = "dark blue grey", hexCode = "#1f3b4d" },
            new ColorEntry { name = "flat green", hexCode = "#699d4c" },
            new ColorEntry { name = "light green blue", hexCode = "#56fca2" },
            new ColorEntry { name = "warm pink", hexCode = "#fb5581" },
            new ColorEntry { name = "dodger blue", hexCode = "#3e82fc" },
            new ColorEntry { name = "gross green", hexCode = "#a0bf16" },
            new ColorEntry { name = "ice", hexCode = "#d6fffa" },
            new ColorEntry { name = "metallic blue", hexCode = "#4f738e" },
            new ColorEntry { name = "pale salmon", hexCode = "#ffb19a" },
            new ColorEntry { name = "sap green", hexCode = "#5c8b15" },
            new ColorEntry { name = "algae", hexCode = "#54ac68" },
            new ColorEntry { name = "bluey grey", hexCode = "#89a0b0" },
            new ColorEntry { name = "greeny grey", hexCode = "#7ea07a" },
            new ColorEntry { name = "highlighter green", hexCode = "#1bfc06" },
            new ColorEntry { name = "light light blue", hexCode = "#cafffb" },
            new ColorEntry { name = "light mint", hexCode = "#b6ffbb" },
            new ColorEntry { name = "raw umber", hexCode = "#a75e09" },
            new ColorEntry { name = "vivid blue", hexCode = "#152eff" },
            new ColorEntry { name = "deep lavender", hexCode = "#8d5eb7" },
            new ColorEntry { name = "dull teal", hexCode = "#5f9e8f" },
            new ColorEntry { name = "light greenish blue", hexCode = "#63f7b4" },
            new ColorEntry { name = "mud green", hexCode = "#606602" },
            new ColorEntry { name = "pinky", hexCode = "#fc86aa" },
            new ColorEntry { name = "red wine", hexCode = "#8c0034" },
            new ColorEntry { name = "shit green", hexCode = "#758000" },
            new ColorEntry { name = "tan brown", hexCode = "#ab7e4c" },
            new ColorEntry { name = "darkblue", hexCode = "#030764" },
            new ColorEntry { name = "rosa", hexCode = "#fe86a4" },
            new ColorEntry { name = "lipstick", hexCode = "#d5174e" },
            new ColorEntry { name = "pale mauve", hexCode = "#fed0fc" },
            new ColorEntry { name = "claret", hexCode = "#680018" },
            new ColorEntry { name = "dandelion", hexCode = "#fedf08" },
            new ColorEntry { name = "orangered", hexCode = "#fe420f" },
            new ColorEntry { name = "poop green", hexCode = "#6f7c00" },
            new ColorEntry { name = "ruby", hexCode = "#ca0147" },
            new ColorEntry { name = "dark", hexCode = "#1b2431" },
            new ColorEntry { name = "greenish turquoise", hexCode = "#00fbb0" },
            new ColorEntry { name = "pastel red", hexCode = "#db5856" },
            new ColorEntry { name = "piss yellow", hexCode = "#ddd618" },
            new ColorEntry { name = "bright cyan", hexCode = "#41fdfe" },
            new ColorEntry { name = "dark coral", hexCode = "#cf524e" },
            new ColorEntry { name = "algae green", hexCode = "#21c36f" },
            new ColorEntry { name = "darkish red", hexCode = "#a90308" },
            new ColorEntry { name = "reddy brown", hexCode = "#6e1005" },
            new ColorEntry { name = "blush pink", hexCode = "#fe828c" },
            new ColorEntry { name = "camouflage green", hexCode = "#4b6113" },
            new ColorEntry { name = "lawn green", hexCode = "#4da409" },
            new ColorEntry { name = "putty", hexCode = "#beae8a" },
            new ColorEntry { name = "vibrant blue", hexCode = "#0339f8" },
            new ColorEntry { name = "dark sand", hexCode = "#a88f59" },
            new ColorEntry { name = "purple/blue", hexCode = "#5d21d0" },
            new ColorEntry { name = "saffron", hexCode = "#feb209" },
            new ColorEntry { name = "twilight", hexCode = "#4e518b" },
            new ColorEntry { name = "warm brown", hexCode = "#964e02" },
            new ColorEntry { name = "bluegrey", hexCode = "#85a3b2" },
            new ColorEntry { name = "bubble gum pink", hexCode = "#ff69af" },
            new ColorEntry { name = "duck egg blue", hexCode = "#c3fbf4" },
            new ColorEntry { name = "greenish cyan", hexCode = "#2afeb7" },
            new ColorEntry { name = "petrol", hexCode = "#005f6a" },
            new ColorEntry { name = "royal", hexCode = "#0c1793" },
            new ColorEntry { name = "butter", hexCode = "#ffff81" },
            new ColorEntry { name = "dusty orange", hexCode = "#f0833a" },
            new ColorEntry { name = "off yellow", hexCode = "#f1f33f" },
            new ColorEntry { name = "pale olive green", hexCode = "#b1d27b" },
            new ColorEntry { name = "orangish", hexCode = "#fc824a" },
            new ColorEntry { name = "leaf", hexCode = "#71aa34" },
            new ColorEntry { name = "light blue grey", hexCode = "#b7c9e2" },
            new ColorEntry { name = "dried blood", hexCode = "#4b0101" },
            new ColorEntry { name = "lightish purple", hexCode = "#a552e6" },
            new ColorEntry { name = "rusty red", hexCode = "#af2f0d" },
            new ColorEntry { name = "lavender blue", hexCode = "#8b88f8" },
            new ColorEntry { name = "light grass green", hexCode = "#9af764" },
            new ColorEntry { name = "light mint green", hexCode = "#a6fbb2" },
            new ColorEntry { name = "sunflower", hexCode = "#ffc512" },
            new ColorEntry { name = "velvet", hexCode = "#750851" },
            new ColorEntry { name = "brick orange", hexCode = "#c14a09" },
            new ColorEntry { name = "lightish red", hexCode = "#fe2f4a" },
            new ColorEntry { name = "pure blue", hexCode = "#0203e2" },
            new ColorEntry { name = "twilight blue", hexCode = "#0a437a" },
            new ColorEntry { name = "violet red", hexCode = "#a50055" },
            new ColorEntry { name = "yellowy brown", hexCode = "#ae8b0c" },
            new ColorEntry { name = "carnation", hexCode = "#fd798f" },
            new ColorEntry { name = "muddy yellow", hexCode = "#bfac05" },
            new ColorEntry { name = "dark seafoam green", hexCode = "#3eaf76" },
            new ColorEntry { name = "deep rose", hexCode = "#c74767" },
            new ColorEntry { name = "dusty red", hexCode = "#b9484e" },
            new ColorEntry { name = "grey/blue", hexCode = "#647d8e" },
            new ColorEntry { name = "lemon lime", hexCode = "#bffe28" },
            new ColorEntry { name = "purple/pink", hexCode = "#d725de" },
            new ColorEntry { name = "brown yellow", hexCode = "#b29705" },
            new ColorEntry { name = "purple brown", hexCode = "#673a3f" },
            new ColorEntry { name = "wisteria", hexCode = "#a87dc2" },
            new ColorEntry { name = "banana yellow", hexCode = "#fafe4b" },
            new ColorEntry { name = "lipstick red", hexCode = "#c0022f" },
            new ColorEntry { name = "water blue", hexCode = "#0e87cc" },
            new ColorEntry { name = "brown grey", hexCode = "#8d8468" },
            new ColorEntry { name = "vibrant purple", hexCode = "#ad03de" },
            new ColorEntry { name = "baby green", hexCode = "#8cff9e" },
            new ColorEntry { name = "barf green", hexCode = "#94ac02" },
            new ColorEntry { name = "eggshell blue", hexCode = "#c4fff7" },
            new ColorEntry { name = "sandy yellow", hexCode = "#fdee73" },
            new ColorEntry { name = "cool green", hexCode = "#33b864" },
            new ColorEntry { name = "pale", hexCode = "#fff9d0" },
            new ColorEntry { name = "blue/grey", hexCode = "#758da3" },
            new ColorEntry { name = "hot magenta", hexCode = "#f504c9" },
            new ColorEntry { name = "greyblue", hexCode = "#77a1b5" },
            new ColorEntry { name = "purpley", hexCode = "#8756e4" },
            new ColorEntry { name = "baby shit green", hexCode = "#889717" },
            new ColorEntry { name = "brownish pink", hexCode = "#c27e79" },
            new ColorEntry { name = "dark aquamarine", hexCode = "#017371" },
            new ColorEntry { name = "diarrhea", hexCode = "#9f8303" },
            new ColorEntry { name = "light mustard", hexCode = "#f7d560" },
            new ColorEntry { name = "pale sky blue", hexCode = "#bdf6fe" },
            new ColorEntry { name = "turtle green", hexCode = "#75b84f" },
            new ColorEntry { name = "bright olive", hexCode = "#9cbb04" },
            new ColorEntry { name = "dark grey blue", hexCode = "#29465b" },
            new ColorEntry { name = "greeny brown", hexCode = "#696006" },
            new ColorEntry { name = "lemon green", hexCode = "#adf802" },
            new ColorEntry { name = "light periwinkle", hexCode = "#c1c6fc" },
            new ColorEntry { name = "seaweed green", hexCode = "#35ad6b" },
            new ColorEntry { name = "sunshine yellow", hexCode = "#fffd37" },
            new ColorEntry { name = "ugly purple", hexCode = "#a442a0" },
            new ColorEntry { name = "medium pink", hexCode = "#f36196" },
            new ColorEntry { name = "puke brown", hexCode = "#947706" },
            new ColorEntry { name = "very light pink", hexCode = "#fff4f2" },
            new ColorEntry { name = "viridian", hexCode = "#1e9167" },
            new ColorEntry { name = "bile", hexCode = "#b5c306" },
            new ColorEntry { name = "faded yellow", hexCode = "#feff7f" },
            new ColorEntry { name = "very pale green", hexCode = "#cffdbc" },
            new ColorEntry { name = "vibrant green", hexCode = "#0add08" },
            new ColorEntry { name = "bright lime", hexCode = "#87fd05" },
            new ColorEntry { name = "spearmint", hexCode = "#1ef876" },
            new ColorEntry { name = "light aquamarine", hexCode = "#7bfdc7" },
            new ColorEntry { name = "light sage", hexCode = "#bcecac" },
            new ColorEntry { name = "yellowgreen", hexCode = "#bbf90f" },
            new ColorEntry { name = "baby poo", hexCode = "#ab9004" },
            new ColorEntry { name = "dark seafoam", hexCode = "#1fb57a" },
            new ColorEntry { name = "deep teal", hexCode = "#00555a" },
            new ColorEntry { name = "heather", hexCode = "#a484ac" },
            new ColorEntry { name = "rust orange", hexCode = "#c45508" },
            new ColorEntry { name = "dirty blue", hexCode = "#3f829d" },
            new ColorEntry { name = "fern green", hexCode = "#548d44" },
            new ColorEntry { name = "bright lilac", hexCode = "#c95efb" },
            new ColorEntry { name = "weird green", hexCode = "#3ae57f" },
            new ColorEntry { name = "peacock blue", hexCode = "#016795" },
            new ColorEntry { name = "avocado green", hexCode = "#87a922" },
            new ColorEntry { name = "faded orange", hexCode = "#f0944d" },
            new ColorEntry { name = "grape purple", hexCode = "#5d1451" },
            new ColorEntry { name = "hot green", hexCode = "#25ff29" },
            new ColorEntry { name = "lime yellow", hexCode = "#d0fe1d" },
            new ColorEntry { name = "mango", hexCode = "#ffa62b" },
            new ColorEntry { name = "shamrock", hexCode = "#01b44c" },
            new ColorEntry { name = "bubblegum", hexCode = "#ff6cb5" },
            new ColorEntry { name = "purplish brown", hexCode = "#6b4247" },
            new ColorEntry { name = "vomit yellow", hexCode = "#c7c10c" },
            new ColorEntry { name = "pale cyan", hexCode = "#b7fffa" },
            new ColorEntry { name = "key lime", hexCode = "#aeff6e" },
            new ColorEntry { name = "tomato red", hexCode = "#ec2d01" },
            new ColorEntry { name = "lightgreen", hexCode = "#76ff7b" },
            new ColorEntry { name = "merlot", hexCode = "#730039" },
            new ColorEntry { name = "night blue", hexCode = "#040348" },
            new ColorEntry { name = "purpleish pink", hexCode = "#df4ec8" },
            new ColorEntry { name = "apple", hexCode = "#6ecb3c" },
            new ColorEntry { name = "baby poop green", hexCode = "#8f9805" },
            new ColorEntry { name = "green apple", hexCode = "#5edc1f" },
            new ColorEntry { name = "heliotrope", hexCode = "#d94ff5" },
            new ColorEntry { name = "yellow/green", hexCode = "#c8fd3d" },
            new ColorEntry { name = "almost black", hexCode = "#070d0d" },
            new ColorEntry { name = "cool blue", hexCode = "#4984b8" },
            new ColorEntry { name = "leafy green", hexCode = "#51b73b" },
            new ColorEntry { name = "mustard brown", hexCode = "#ac7e04" },
            new ColorEntry { name = "dusk", hexCode = "#4e5481" },
            new ColorEntry { name = "dull brown", hexCode = "#876e4b" },
            new ColorEntry { name = "frog green", hexCode = "#58bc08" },
            new ColorEntry { name = "vivid green", hexCode = "#2fef10" },
            new ColorEntry { name = "bright light green", hexCode = "#2dfe54" },
            new ColorEntry { name = "fluro green", hexCode = "#0aff02" },
            new ColorEntry { name = "kiwi", hexCode = "#9cef43" },
            new ColorEntry { name = "seaweed", hexCode = "#18d17b" },
            new ColorEntry { name = "navy green", hexCode = "#35530a" },
            new ColorEntry { name = "ultramarine blue", hexCode = "#1805db" },
            new ColorEntry { name = "iris", hexCode = "#6258c4" },
            new ColorEntry { name = "pastel orange", hexCode = "#ff964f" },
            new ColorEntry { name = "yellowish orange", hexCode = "#ffab0f" },
            new ColorEntry { name = "perrywinkle", hexCode = "#8f8ce7" },
            new ColorEntry { name = "tealish", hexCode = "#24bca8" },
            new ColorEntry { name = "dark plum", hexCode = "#3f012c" },
            new ColorEntry { name = "pear", hexCode = "#cbf85f" },
            new ColorEntry { name = "pinkish orange", hexCode = "#ff724c" },
            new ColorEntry { name = "midnight purple", hexCode = "#280137" },
            new ColorEntry { name = "light urple", hexCode = "#b36ff6" },
            new ColorEntry { name = "dark mint", hexCode = "#48c072" },
            new ColorEntry { name = "greenish tan", hexCode = "#bccb7a" },
            new ColorEntry { name = "light burgundy", hexCode = "#a8415b" },
            new ColorEntry { name = "turquoise blue", hexCode = "#06b1c4" },
            new ColorEntry { name = "ugly pink", hexCode = "#cd7584" },
            new ColorEntry { name = "sandy", hexCode = "#f1da7a" },
            new ColorEntry { name = "electric pink", hexCode = "#ff0490" },
            new ColorEntry { name = "muted purple", hexCode = "#805b87" },
            new ColorEntry { name = "mid green", hexCode = "#50a747" },
            new ColorEntry { name = "greyish", hexCode = "#a8a495" },
            new ColorEntry { name = "neon yellow", hexCode = "#cfff04" },
            new ColorEntry { name = "banana", hexCode = "#ffff7e" },
            new ColorEntry { name = "carnation pink", hexCode = "#ff7fa7" },
            new ColorEntry { name = "tomato", hexCode = "#ef4026" },
            new ColorEntry { name = "sea", hexCode = "#3c9992" },
            new ColorEntry { name = "muddy brown", hexCode = "#886806" },
            new ColorEntry { name = "turquoise green", hexCode = "#04f489" },
            new ColorEntry { name = "buff", hexCode = "#fef69e" },
            new ColorEntry { name = "fawn", hexCode = "#cfaf7b" },
            new ColorEntry { name = "muted blue", hexCode = "#3b719f" },
            new ColorEntry { name = "pale rose", hexCode = "#fdc1c5" },
            new ColorEntry { name = "dark mint green", hexCode = "#20c073" },
            new ColorEntry { name = "amethyst", hexCode = "#9b5fc0" },
            new ColorEntry { name = "blue/green", hexCode = "#0f9b8e" },
            new ColorEntry { name = "chestnut", hexCode = "#742802" },
            new ColorEntry { name = "sick green", hexCode = "#9db92c" },
            new ColorEntry { name = "pea", hexCode = "#a4bf20" },
            new ColorEntry { name = "rusty orange", hexCode = "#cd5909" },
            new ColorEntry { name = "stone", hexCode = "#ada587" },
            new ColorEntry { name = "rose red", hexCode = "#be013c" },
            new ColorEntry { name = "pale aqua", hexCode = "#b8ffeb" },
            new ColorEntry { name = "deep orange", hexCode = "#dc4d01" },
            new ColorEntry { name = "earth", hexCode = "#a2653e" },
            new ColorEntry { name = "mossy green", hexCode = "#638b27" },
            new ColorEntry { name = "grassy green", hexCode = "#419c03" },
            new ColorEntry { name = "pale lime green", hexCode = "#b1ff65" },
            new ColorEntry { name = "light grey blue", hexCode = "#9dbcd4" },
            new ColorEntry { name = "pale grey", hexCode = "#fdfdfe" },
            new ColorEntry { name = "asparagus", hexCode = "#77ab56" },
            new ColorEntry { name = "blueberry", hexCode = "#464196" },
            new ColorEntry { name = "purple red", hexCode = "#990147" },
            new ColorEntry { name = "pale lime", hexCode = "#befd73" },
            new ColorEntry { name = "greenish teal", hexCode = "#32bf84" },
            new ColorEntry { name = "caramel", hexCode = "#af6f09" },
            new ColorEntry { name = "deep magenta", hexCode = "#a0025c" },
            new ColorEntry { name = "light peach", hexCode = "#ffd8b1" },
            new ColorEntry { name = "milk chocolate", hexCode = "#7f4e1e" },
            new ColorEntry { name = "ocher", hexCode = "#bf9b0c" },
            new ColorEntry { name = "off green", hexCode = "#6ba353" },
            new ColorEntry { name = "purply pink", hexCode = "#f075e6" },
            new ColorEntry { name = "lightblue", hexCode = "#7bc8f6" },
            new ColorEntry { name = "dusky blue", hexCode = "#475f94" },
            new ColorEntry { name = "golden", hexCode = "#f5bf03" },
            new ColorEntry { name = "light beige", hexCode = "#fffeb6" },
            new ColorEntry { name = "butter yellow", hexCode = "#fffd74" },
            new ColorEntry { name = "dusky purple", hexCode = "#895b7b" },
            new ColorEntry { name = "french blue", hexCode = "#436bad" },
            new ColorEntry { name = "ugly yellow", hexCode = "#d0c101" },
            new ColorEntry { name = "greeny yellow", hexCode = "#c6f808" },
            new ColorEntry { name = "orangish red", hexCode = "#f43605" },
            new ColorEntry { name = "shamrock green", hexCode = "#02c14d" },
            new ColorEntry { name = "orangish brown", hexCode = "#b25f03" },
            new ColorEntry { name = "tree green", hexCode = "#2a7e19" },
            new ColorEntry { name = "deep violet", hexCode = "#490648" },
            new ColorEntry { name = "gunmetal", hexCode = "#536267" },
            new ColorEntry { name = "blue/purple", hexCode = "#5a06ef" },
            new ColorEntry { name = "cherry", hexCode = "#cf0234" },
            new ColorEntry { name = "sandy brown", hexCode = "#c4a661" },
            new ColorEntry { name = "warm grey", hexCode = "#978a84" },
            new ColorEntry { name = "dark indigo", hexCode = "#1f0954" },
            new ColorEntry { name = "midnight", hexCode = "#03012d" },
            new ColorEntry { name = "bluey green", hexCode = "#2bb179" },
            new ColorEntry { name = "grey pink", hexCode = "#c3909b" },
            new ColorEntry { name = "soft purple", hexCode = "#a66fb5" },
            new ColorEntry { name = "blood", hexCode = "#770001" },
            new ColorEntry { name = "brown red", hexCode = "#922b05" },
            new ColorEntry { name = "medium grey", hexCode = "#7d7f7c" },
            new ColorEntry { name = "berry", hexCode = "#990f4b" },
            new ColorEntry { name = "poo", hexCode = "#8f7303" },
            new ColorEntry { name = "purpley pink", hexCode = "#c83cb9" },
            new ColorEntry { name = "light salmon", hexCode = "#fea993" },
            new ColorEntry { name = "snot", hexCode = "#acbb0d" },
            new ColorEntry { name = "easter purple", hexCode = "#c071fe" },
            new ColorEntry { name = "light yellow green", hexCode = "#ccfd7f" },
            new ColorEntry { name = "dark navy blue", hexCode = "#00022e" },
            new ColorEntry { name = "drab", hexCode = "#828344" },
            new ColorEntry { name = "light rose", hexCode = "#ffc5cb" },
            new ColorEntry { name = "rouge", hexCode = "#ab1239" },
            new ColorEntry { name = "purplish red", hexCode = "#b0054b" },
            new ColorEntry { name = "slime green", hexCode = "#99cc04" },
            new ColorEntry { name = "baby poop", hexCode = "#937c00" },
            new ColorEntry { name = "irish green", hexCode = "#019529" },
            new ColorEntry { name = "pink/purple", hexCode = "#ef1de7" },
            new ColorEntry { name = "dark navy", hexCode = "#000435" },
            new ColorEntry { name = "greeny blue", hexCode = "#42b395" },
            new ColorEntry { name = "light plum", hexCode = "#9d5783" },
            new ColorEntry { name = "pinkish grey", hexCode = "#c8aca9" },
            new ColorEntry { name = "dirty orange", hexCode = "#c87606" },
            new ColorEntry { name = "rust red", hexCode = "#aa2704" },
            new ColorEntry { name = "pale lilac", hexCode = "#e4cbff" },
            new ColorEntry { name = "orangey red", hexCode = "#fa4224" },
            new ColorEntry { name = "primary blue", hexCode = "#0804f9" },
            new ColorEntry { name = "kermit green", hexCode = "#5cb200" },
            new ColorEntry { name = "brownish purple", hexCode = "#76424e" },
            new ColorEntry { name = "murky green", hexCode = "#6c7a0e" },
            new ColorEntry { name = "wheat", hexCode = "#fbdd7e" },
            new ColorEntry { name = "very dark purple", hexCode = "#2a0134" },
            new ColorEntry { name = "bottle green", hexCode = "#044a05" },
            new ColorEntry { name = "watermelon", hexCode = "#fd4659" },
            new ColorEntry { name = "deep sky blue", hexCode = "#0d75f8" },
            new ColorEntry { name = "fire engine red", hexCode = "#fe0002" },
            new ColorEntry { name = "yellow ochre", hexCode = "#cb9d06" },
            new ColorEntry { name = "pumpkin orange", hexCode = "#fb7d07" },
            new ColorEntry { name = "pale olive", hexCode = "#b9cc81" },
            new ColorEntry { name = "light lilac", hexCode = "#edc8ff" },
            new ColorEntry { name = "lightish green", hexCode = "#61e160" },
            new ColorEntry { name = "carolina blue", hexCode = "#8ab8fe" },
            new ColorEntry { name = "mulberry", hexCode = "#920a4e" },
            new ColorEntry { name = "shocking pink", hexCode = "#fe02a2" },
            new ColorEntry { name = "auburn", hexCode = "#9a3001" },
            new ColorEntry { name = "bright lime green", hexCode = "#65fe08" },
            new ColorEntry { name = "celadon", hexCode = "#befdb7" },
            new ColorEntry { name = "pinkish brown", hexCode = "#b17261" },
            new ColorEntry { name = "poo brown", hexCode = "#885f01" },
            new ColorEntry { name = "bright sky blue", hexCode = "#02ccfe" },
            new ColorEntry { name = "celery", hexCode = "#c1fd95" },
            new ColorEntry { name = "dirt brown", hexCode = "#836539" },
            new ColorEntry { name = "strawberry", hexCode = "#fb2943" },
            new ColorEntry { name = "dark lime", hexCode = "#84b701" },
            new ColorEntry { name = "copper", hexCode = "#b66325" },
            new ColorEntry { name = "medium brown", hexCode = "#7f5112" },
            new ColorEntry { name = "muted green", hexCode = "#5fa052" },
            new ColorEntry { name = "robin's egg", hexCode = "#6dedfd" },
            new ColorEntry { name = "bright aqua", hexCode = "#0bf9ea" },
            new ColorEntry { name = "bright lavender", hexCode = "#c760ff" },
            new ColorEntry { name = "ivory", hexCode = "#ffffcb" },
            new ColorEntry { name = "very light purple", hexCode = "#f6cefc" },
            new ColorEntry { name = "light navy", hexCode = "#155084" },
            new ColorEntry { name = "pink red", hexCode = "#f5054f" },
            new ColorEntry { name = "olive brown", hexCode = "#645403" },
            new ColorEntry { name = "poop brown", hexCode = "#7a5901" },
            new ColorEntry { name = "mustard green", hexCode = "#a8b504" },
            new ColorEntry { name = "ocean green", hexCode = "#3d9973" },
            new ColorEntry { name = "very dark blue", hexCode = "#000133" },
            new ColorEntry { name = "dusty green", hexCode = "#76a973" },
            new ColorEntry { name = "light navy blue", hexCode = "#2e5a88" },
            new ColorEntry { name = "minty green", hexCode = "#0bf77d" },
            new ColorEntry { name = "adobe", hexCode = "#bd6c48" },
            new ColorEntry { name = "barney", hexCode = "#ac1db8" },
            new ColorEntry { name = "jade green", hexCode = "#2baf6a" },
            new ColorEntry { name = "bright light blue", hexCode = "#26f7fd" },
            new ColorEntry { name = "light lime", hexCode = "#aefd6c" },
            new ColorEntry { name = "dark khaki", hexCode = "#9b8f55" },
            new ColorEntry { name = "orange yellow", hexCode = "#ffad01" },
            new ColorEntry { name = "ocre", hexCode = "#c69c04" },
            new ColorEntry { name = "maize", hexCode = "#f4d054" },
            new ColorEntry { name = "faded pink", hexCode = "#de9dac" },
            new ColorEntry { name = "british racing green", hexCode = "#05480d" },
            new ColorEntry { name = "sandstone", hexCode = "#c9ae74" },
            new ColorEntry { name = "mud brown", hexCode = "#60460f" },
            new ColorEntry { name = "light sea green", hexCode = "#98f6b0" },
            new ColorEntry { name = "robin egg blue", hexCode = "#8af1fe" },
            new ColorEntry { name = "aqua marine", hexCode = "#2ee8bb" },
            new ColorEntry { name = "dark sea green", hexCode = "#11875d" },
            new ColorEntry { name = "soft pink", hexCode = "#fdb0c0" },
            new ColorEntry { name = "orangey brown", hexCode = "#b16002" },
            new ColorEntry { name = "cherry red", hexCode = "#f7022a" },
            new ColorEntry { name = "burnt yellow", hexCode = "#d5ab09" },
            new ColorEntry { name = "brownish grey", hexCode = "#86775f" },
            new ColorEntry { name = "camel", hexCode = "#c69f59" },
            new ColorEntry { name = "purplish grey", hexCode = "#7a687f" },
            new ColorEntry { name = "marine", hexCode = "#042e60" },
            new ColorEntry { name = "greyish pink", hexCode = "#c88d94" },
            new ColorEntry { name = "pale turquoise", hexCode = "#a5fbd5" },
            new ColorEntry { name = "pastel yellow", hexCode = "#fffe71" },
            new ColorEntry { name = "bluey purple", hexCode = "#6241c7" },
            new ColorEntry { name = "canary yellow", hexCode = "#fffe40" },
            new ColorEntry { name = "faded red", hexCode = "#d3494e" },
            new ColorEntry { name = "sepia", hexCode = "#985e2b" },
            new ColorEntry { name = "coffee", hexCode = "#a6814c" },
            new ColorEntry { name = "bright magenta", hexCode = "#ff08e8" },
            new ColorEntry { name = "mocha", hexCode = "#9d7651" },
            new ColorEntry { name = "ecru", hexCode = "#feffca" },
            new ColorEntry { name = "purpleish", hexCode = "#98568d" },
            new ColorEntry { name = "cranberry", hexCode = "#9e003a" },
            new ColorEntry { name = "darkish green", hexCode = "#287c37" },
            new ColorEntry { name = "brown orange", hexCode = "#b96902" },
            new ColorEntry { name = "dusky rose", hexCode = "#ba6873" },
            new ColorEntry { name = "melon", hexCode = "#ff7855" },
            new ColorEntry { name = "sickly green", hexCode = "#94b21c" },
            new ColorEntry { name = "silver", hexCode = "#c5c9c7" },
            new ColorEntry { name = "purply blue", hexCode = "#661aee" },
            new ColorEntry { name = "purpleish blue", hexCode = "#6140ef" },
            new ColorEntry { name = "hospital green", hexCode = "#9be5aa" },
            new ColorEntry { name = "shit brown", hexCode = "#7b5804" },
            new ColorEntry { name = "mid blue", hexCode = "#276ab3" },
            new ColorEntry { name = "amber", hexCode = "#feb308" },
            new ColorEntry { name = "easter green", hexCode = "#8cfd7e" },
            new ColorEntry { name = "soft blue", hexCode = "#6488ea" },
            new ColorEntry { name = "cerulean blue", hexCode = "#056eee" },
            new ColorEntry { name = "golden brown", hexCode = "#b27a01" },
            new ColorEntry { name = "bright turquoise", hexCode = "#0ffef9" },
            new ColorEntry { name = "red pink", hexCode = "#fa2a55" },
            new ColorEntry { name = "red purple", hexCode = "#820747" },
            new ColorEntry { name = "greyish brown", hexCode = "#7a6a4f" },
            new ColorEntry { name = "vermillion", hexCode = "#f4320c" },
            new ColorEntry { name = "russet", hexCode = "#a13905" },
            new ColorEntry { name = "steel grey", hexCode = "#6f828a" },
            new ColorEntry { name = "lighter purple", hexCode = "#a55af4" },
            new ColorEntry { name = "bright violet", hexCode = "#ad0afd" },
            new ColorEntry { name = "prussian blue", hexCode = "#004577" },
            new ColorEntry { name = "slate green", hexCode = "#658d6d" },
            new ColorEntry { name = "dirty pink", hexCode = "#ca7b80" },
            new ColorEntry { name = "dark blue green", hexCode = "#005249" },
            new ColorEntry { name = "pine", hexCode = "#2b5d34" },
            new ColorEntry { name = "yellowy green", hexCode = "#bff128" },
            new ColorEntry { name = "dark gold", hexCode = "#b59410" },
            new ColorEntry { name = "bluish", hexCode = "#2976bb" },
            new ColorEntry { name = "darkish blue", hexCode = "#014182" },
            new ColorEntry { name = "dull red", hexCode = "#bb3f3f" },
            new ColorEntry { name = "pinky red", hexCode = "#fc2647" },
            new ColorEntry { name = "bronze", hexCode = "#a87900" },
            new ColorEntry { name = "pale teal", hexCode = "#82cbb2" },
            new ColorEntry { name = "military green", hexCode = "#667c3e" },
            new ColorEntry { name = "barbie pink", hexCode = "#fe46a5" },
            new ColorEntry { name = "bubblegum pink", hexCode = "#fe83cc" },
            new ColorEntry { name = "pea soup green", hexCode = "#94a617" },
            new ColorEntry { name = "dark mustard", hexCode = "#a88905" },
            new ColorEntry { name = "shit", hexCode = "#7f5f00" },
            new ColorEntry { name = "medium purple", hexCode = "#9e43a2" },
            new ColorEntry { name = "very dark green", hexCode = "#062e03" },
            new ColorEntry { name = "dirt", hexCode = "#8a6e45" },
            new ColorEntry { name = "dusky pink", hexCode = "#cc7a8b" },
            new ColorEntry { name = "red violet", hexCode = "#9e0168" },
            new ColorEntry { name = "lemon yellow", hexCode = "#fdff38" },
            new ColorEntry { name = "pistachio", hexCode = "#c0fa8b" },
            new ColorEntry { name = "dull yellow", hexCode = "#eedc5b" },
            new ColorEntry { name = "dark lime green", hexCode = "#7ebd01" },
            new ColorEntry { name = "denim blue", hexCode = "#3b5b92" },
            new ColorEntry { name = "teal blue", hexCode = "#01889f" },
            new ColorEntry { name = "lightish blue", hexCode = "#3d7afd" },
            new ColorEntry { name = "purpley blue", hexCode = "#5f34e7" },
            new ColorEntry { name = "light indigo", hexCode = "#6d5acf" },
            new ColorEntry { name = "swamp green", hexCode = "#748500" },
            new ColorEntry { name = "brown green", hexCode = "#706c11" },
            new ColorEntry { name = "dark maroon", hexCode = "#3c0008" },
            new ColorEntry { name = "hot purple", hexCode = "#cb00f5" },
            new ColorEntry { name = "dark forest green", hexCode = "#002d04" },
            new ColorEntry { name = "faded blue", hexCode = "#658cbb" },
            new ColorEntry { name = "drab green", hexCode = "#749551" },
            new ColorEntry { name = "light lime green", hexCode = "#b9ff66" },
            new ColorEntry { name = "snot green", hexCode = "#9dc100" },
            new ColorEntry { name = "yellowish", hexCode = "#faee66" },
            new ColorEntry { name = "light blue green", hexCode = "#7efbb3" },
            new ColorEntry { name = "bordeaux", hexCode = "#7b002c" },
            new ColorEntry { name = "light mauve", hexCode = "#c292a1" },
            new ColorEntry { name = "ocean", hexCode = "#017b92" },
            new ColorEntry { name = "marigold", hexCode = "#fcc006" },
            new ColorEntry { name = "muddy green", hexCode = "#657432" },
            new ColorEntry { name = "dull orange", hexCode = "#d8863b" },
            new ColorEntry { name = "steel", hexCode = "#738595" },
            new ColorEntry { name = "electric purple", hexCode = "#aa23ff" },
            new ColorEntry { name = "fluorescent green", hexCode = "#08ff08" },
            new ColorEntry { name = "yellowish brown", hexCode = "#9b7a01" },
            new ColorEntry { name = "blush", hexCode = "#f29e8e" },
            new ColorEntry { name = "soft green", hexCode = "#6fc276" },
            new ColorEntry { name = "bright orange", hexCode = "#ff5b00" },
            new ColorEntry { name = "lemon", hexCode = "#fdff52" },
            new ColorEntry { name = "purple grey", hexCode = "#866f85" },
            new ColorEntry { name = "acid green", hexCode = "#8ffe09" },
            new ColorEntry { name = "pale lavender", hexCode = "#eecffe" },
            new ColorEntry { name = "violet blue", hexCode = "#510ac9" },
            new ColorEntry { name = "light forest green", hexCode = "#4f9153" },
            new ColorEntry { name = "burnt red", hexCode = "#9f2305" },
            new ColorEntry { name = "khaki green", hexCode = "#728639" },
            new ColorEntry { name = "cerise", hexCode = "#de0c62" },
            new ColorEntry { name = "faded purple", hexCode = "#916e99" },
            new ColorEntry { name = "apricot", hexCode = "#ffb16d" },
            new ColorEntry { name = "dark olive green", hexCode = "#3c4d03" },
            new ColorEntry { name = "grey brown", hexCode = "#7f7053" },
            new ColorEntry { name = "green grey", hexCode = "#77926f" },
            new ColorEntry { name = "true blue", hexCode = "#010fcc" },
            new ColorEntry { name = "pale violet", hexCode = "#ceaefa" },
            new ColorEntry { name = "periwinkle blue", hexCode = "#8f99fb" },
            new ColorEntry { name = "light sky blue", hexCode = "#c6fcff" },
            new ColorEntry { name = "blurple", hexCode = "#5539cc" },
            new ColorEntry { name = "green brown", hexCode = "#544e03" },
            new ColorEntry { name = "bluegreen", hexCode = "#017a79" },
            new ColorEntry { name = "bright teal", hexCode = "#01f9c6" },
            new ColorEntry { name = "brownish yellow", hexCode = "#c9b003" },
            new ColorEntry { name = "pea soup", hexCode = "#929901" },
            new ColorEntry { name = "forest", hexCode = "#0b5509" },
            new ColorEntry { name = "barney purple", hexCode = "#a00498" },
            new ColorEntry { name = "ultramarine", hexCode = "#2000b1" },
            new ColorEntry { name = "purplish", hexCode = "#94568c" },
            new ColorEntry { name = "puke yellow", hexCode = "#c2be0e" },
            new ColorEntry { name = "bluish grey", hexCode = "#748b97" },
            new ColorEntry { name = "dark periwinkle", hexCode = "#665fd1" },
            new ColorEntry { name = "dark lilac", hexCode = "#9c6da5" },
            new ColorEntry { name = "reddish", hexCode = "#c44240" },
            new ColorEntry { name = "light maroon", hexCode = "#a24857" },
            new ColorEntry { name = "dusty purple", hexCode = "#825f87" },
            new ColorEntry { name = "terra cotta", hexCode = "#c9643b" },
            new ColorEntry { name = "avocado", hexCode = "#90b134" },
            new ColorEntry { name = "marine blue", hexCode = "#01386a" },
            new ColorEntry { name = "teal green", hexCode = "#25a36f" },
            new ColorEntry { name = "slate grey", hexCode = "#59656d" },
            new ColorEntry { name = "lighter green", hexCode = "#75fd63" },
            new ColorEntry { name = "electric green", hexCode = "#21fc0d" },
            new ColorEntry { name = "dusty blue", hexCode = "#5a86ad" },
            new ColorEntry { name = "golden yellow", hexCode = "#fec615" },
            new ColorEntry { name = "bright yellow", hexCode = "#fffd01" },
            new ColorEntry { name = "light lavender", hexCode = "#dfc5fe" },
            new ColorEntry { name = "umber", hexCode = "#b26400" },
            new ColorEntry { name = "poop", hexCode = "#7f5e00" },
            new ColorEntry { name = "dark peach", hexCode = "#de7e5d" },
            new ColorEntry { name = "jungle green", hexCode = "#048243" },
            new ColorEntry { name = "eggshell", hexCode = "#ffffd4" },
            new ColorEntry { name = "denim", hexCode = "#3b638c" },
            new ColorEntry { name = "yellow brown", hexCode = "#b79400" },
            new ColorEntry { name = "dull purple", hexCode = "#84597e" },
            new ColorEntry { name = "chocolate brown", hexCode = "#411900" },
            new ColorEntry { name = "wine red", hexCode = "#7b0323" },
            new ColorEntry { name = "neon blue", hexCode = "#04d9ff" },
            new ColorEntry { name = "dirty green", hexCode = "#667e2c" },
            new ColorEntry { name = "light tan", hexCode = "#fbeeac" },
            new ColorEntry { name = "ice blue", hexCode = "#d7fffe" },
            new ColorEntry { name = "cadet blue", hexCode = "#4e7496" },
            new ColorEntry { name = "dark mauve", hexCode = "#874c62" },
            new ColorEntry { name = "very light blue", hexCode = "#d5ffff" },
            new ColorEntry { name = "grey purple", hexCode = "#826d8c" },
            new ColorEntry { name = "pastel pink", hexCode = "#ffbacd" },
            new ColorEntry { name = "very light green", hexCode = "#d1ffbd" },
            new ColorEntry { name = "dark sky blue", hexCode = "#448ee4" },
            new ColorEntry { name = "evergreen", hexCode = "#05472a" },
            new ColorEntry { name = "dull pink", hexCode = "#d5869d" },
            new ColorEntry { name = "aubergine", hexCode = "#3d0734" },
            new ColorEntry { name = "mahogany", hexCode = "#4a0100" },
            new ColorEntry { name = "reddish orange", hexCode = "#f8481c" },
            new ColorEntry { name = "deep green", hexCode = "#02590f" },
            new ColorEntry { name = "vomit green", hexCode = "#89a203" },
            new ColorEntry { name = "purple pink", hexCode = "#e03fd8" },
            new ColorEntry { name = "dusty pink", hexCode = "#d58a94" },
            new ColorEntry { name = "faded green", hexCode = "#7bb274" },
            new ColorEntry { name = "camo green", hexCode = "#526525" },
            new ColorEntry { name = "pinky purple", hexCode = "#c94cbe" },
            new ColorEntry { name = "pink purple", hexCode = "#db4bda" },
            new ColorEntry { name = "brownish red", hexCode = "#9e3623" },
            new ColorEntry { name = "dark rose", hexCode = "#b5485d" },
            new ColorEntry { name = "mud", hexCode = "#735c12" },
            new ColorEntry { name = "brownish", hexCode = "#9c6d57" },
            new ColorEntry { name = "emerald green", hexCode = "#028f1e" },
            new ColorEntry { name = "pale brown", hexCode = "#b1916e" },
            new ColorEntry { name = "dull blue", hexCode = "#49759c" },
            new ColorEntry { name = "burnt umber", hexCode = "#a0450e" },
            new ColorEntry { name = "medium green", hexCode = "#39ad48" },
            new ColorEntry { name = "clay", hexCode = "#b66a50" },
            new ColorEntry { name = "light aqua", hexCode = "#8cffdb" },
            new ColorEntry { name = "light olive green", hexCode = "#a4be5c" },
            new ColorEntry { name = "brownish orange", hexCode = "#cb7723" },
            new ColorEntry { name = "dark aqua", hexCode = "#05696b" },
            new ColorEntry { name = "purplish pink", hexCode = "#ce5dae" },
            new ColorEntry { name = "dark salmon", hexCode = "#c85a53" },
            new ColorEntry { name = "greenish grey", hexCode = "#96ae8d" },
            new ColorEntry { name = "jade", hexCode = "#1fa774" },
            new ColorEntry { name = "ugly green", hexCode = "#7a9703" },
            new ColorEntry { name = "dark beige", hexCode = "#ac9362" },
            new ColorEntry { name = "emerald", hexCode = "#01a049" },
            new ColorEntry { name = "pale red", hexCode = "#d9544d" },
            new ColorEntry { name = "light magenta", hexCode = "#fa5ff7" },
            new ColorEntry { name = "sky", hexCode = "#82cafc" },
            new ColorEntry { name = "light cyan", hexCode = "#acfffc" },
            new ColorEntry { name = "yellow orange", hexCode = "#fcb001" },
            new ColorEntry { name = "reddish purple", hexCode = "#910951" },
            new ColorEntry { name = "reddish pink", hexCode = "#fe2c54" },
            new ColorEntry { name = "orchid", hexCode = "#c875c4" },
            new ColorEntry { name = "dirty yellow", hexCode = "#cdc50a" },
            new ColorEntry { name = "orange red", hexCode = "#fd411e" },
            new ColorEntry { name = "deep red", hexCode = "#9a0200" },
            new ColorEntry { name = "orange brown", hexCode = "#be6400" },
            new ColorEntry { name = "cobalt blue", hexCode = "#030aa7" },
            new ColorEntry { name = "neon pink", hexCode = "#fe019a" },
            new ColorEntry { name = "rose pink", hexCode = "#f7879a" },
            new ColorEntry { name = "greyish purple", hexCode = "#887191" },
            new ColorEntry { name = "raspberry", hexCode = "#b00149" },
            new ColorEntry { name = "aqua green", hexCode = "#12e193" },
            new ColorEntry { name = "salmon pink", hexCode = "#fe7b7c" },
            new ColorEntry { name = "tangerine", hexCode = "#ff9408" },
            new ColorEntry { name = "brownish green", hexCode = "#6a6e09" },
            new ColorEntry { name = "red brown", hexCode = "#8b2e16" },
            new ColorEntry { name = "greenish brown", hexCode = "#696112" },
            new ColorEntry { name = "pumpkin", hexCode = "#e17701" },
            new ColorEntry { name = "pine green", hexCode = "#0a481e" },
            new ColorEntry { name = "charcoal", hexCode = "#343837" },
            new ColorEntry { name = "baby pink", hexCode = "#ffb7ce" },
            new ColorEntry { name = "cornflower", hexCode = "#6a79f7" },
            new ColorEntry { name = "blue violet", hexCode = "#5d06e9" },
            new ColorEntry { name = "chocolate", hexCode = "#3d1c02" },
            new ColorEntry { name = "greyish green", hexCode = "#82a67d" },
            new ColorEntry { name = "scarlet", hexCode = "#be0119" },
            new ColorEntry { name = "green yellow", hexCode = "#c9ff27" },
            new ColorEntry { name = "dark olive", hexCode = "#373e02" },
            new ColorEntry { name = "sienna", hexCode = "#a9561e" },
            new ColorEntry { name = "pastel purple", hexCode = "#caa0ff" },
            new ColorEntry { name = "terracotta", hexCode = "#ca6641" },
            new ColorEntry { name = "aqua blue", hexCode = "#02d8e9" },
            new ColorEntry { name = "sage green", hexCode = "#88b378" },
            new ColorEntry { name = "blood red", hexCode = "#980002" },
            new ColorEntry { name = "deep pink", hexCode = "#cb0162" },
            new ColorEntry { name = "grass", hexCode = "#5cac2d" },
            new ColorEntry { name = "moss", hexCode = "#769958" },
            new ColorEntry { name = "pastel blue", hexCode = "#a2bffe" },
            new ColorEntry { name = "bluish green", hexCode = "#10a674" },
            new ColorEntry { name = "green blue", hexCode = "#06b48b" },
            new ColorEntry { name = "dark tan", hexCode = "#af884a" },
            new ColorEntry { name = "greenish blue", hexCode = "#0b8b87" },
            new ColorEntry { name = "pale orange", hexCode = "#ffa756" },
            new ColorEntry { name = "vomit", hexCode = "#a2a415" },
            new ColorEntry { name = "forrest green", hexCode = "#154406" },
            new ColorEntry { name = "dark lavender", hexCode = "#856798" },
            new ColorEntry { name = "dark violet", hexCode = "#34013f" },
            new ColorEntry { name = "purple blue", hexCode = "#632de9" },
            new ColorEntry { name = "dark cyan", hexCode = "#0a888a" },
            new ColorEntry { name = "olive drab", hexCode = "#6f7632" },
            new ColorEntry { name = "pinkish", hexCode = "#d46a7e" },
            new ColorEntry { name = "cobalt", hexCode = "#1e488f" },
            new ColorEntry { name = "neon purple", hexCode = "#bc13fe" },
            new ColorEntry { name = "light turquoise", hexCode = "#7ef4cc" },
            new ColorEntry { name = "apple green", hexCode = "#76cd26" },
            new ColorEntry { name = "dull green", hexCode = "#74a662" },
            new ColorEntry { name = "wine", hexCode = "#80013f" },
            new ColorEntry { name = "powder blue", hexCode = "#b1d1fc" },
            new ColorEntry { name = "off white", hexCode = "#ffffe4" },
            new ColorEntry { name = "electric blue", hexCode = "#0652ff" },
            new ColorEntry { name = "dark turquoise", hexCode = "#045c5a" },
            new ColorEntry { name = "blue purple", hexCode = "#5729ce" },
            new ColorEntry { name = "azure", hexCode = "#069af3" },
            new ColorEntry { name = "bright red", hexCode = "#ff000d" },
            new ColorEntry { name = "pinkish red", hexCode = "#f10c45" },
            new ColorEntry { name = "cornflower blue", hexCode = "#5170d7" },
            new ColorEntry { name = "light olive", hexCode = "#acbf69" },
            new ColorEntry { name = "grape", hexCode = "#6c3461" },
            new ColorEntry { name = "greyish blue", hexCode = "#5e819d" },
            new ColorEntry { name = "purplish blue", hexCode = "#601ef9" },
            new ColorEntry { name = "yellowish green", hexCode = "#b0dd16" },
            new ColorEntry { name = "greenish yellow", hexCode = "#cdfd02" },
            new ColorEntry { name = "medium blue", hexCode = "#2c6fbb" },
            new ColorEntry { name = "dusty rose", hexCode = "#c0737a" },
            new ColorEntry { name = "light violet", hexCode = "#d6b4fc" },
            new ColorEntry { name = "midnight blue", hexCode = "#020035" },
            new ColorEntry { name = "bluish purple", hexCode = "#703be7" },
            new ColorEntry { name = "red orange", hexCode = "#fd3c06" },
            new ColorEntry { name = "dark magenta", hexCode = "#960056" },
            new ColorEntry { name = "greenish", hexCode = "#40a368" },
            new ColorEntry { name = "ocean blue", hexCode = "#03719c" },
            new ColorEntry { name = "coral", hexCode = "#fc5a50" },
            new ColorEntry { name = "cream", hexCode = "#ffffc2" },
            new ColorEntry { name = "reddish brown", hexCode = "#7f2b0a" },
            new ColorEntry { name = "burnt sienna", hexCode = "#b04e0f" },
            new ColorEntry { name = "brick", hexCode = "#a03623" },
            new ColorEntry { name = "sage", hexCode = "#87ae73" },
            new ColorEntry { name = "grey green", hexCode = "#789b73" },
            new ColorEntry { name = "white", hexCode = "#ffffff" },
            new ColorEntry { name = "robin's egg blue", hexCode = "#98eff9" },
            new ColorEntry { name = "moss green", hexCode = "#658b38" },
            new ColorEntry { name = "steel blue", hexCode = "#5a7d9a" },
            new ColorEntry { name = "eggplant", hexCode = "#380835" },
            new ColorEntry { name = "light yellow", hexCode = "#fffe7a" },
            new ColorEntry { name = "leaf green", hexCode = "#5ca904" },
            new ColorEntry { name = "light grey", hexCode = "#d8dcd6" },
            new ColorEntry { name = "puke", hexCode = "#a5a502" },
            new ColorEntry { name = "pinkish purple", hexCode = "#d648d7" },
            new ColorEntry { name = "sea blue", hexCode = "#047495" },
            new ColorEntry { name = "pale purple", hexCode = "#b790d4" },
            new ColorEntry { name = "slate blue", hexCode = "#5b7c99" },
            new ColorEntry { name = "blue grey", hexCode = "#607c8e" },
            new ColorEntry { name = "hunter green", hexCode = "#0b4008" },
            new ColorEntry { name = "fuchsia", hexCode = "#ed0dd9" },
            new ColorEntry { name = "crimson", hexCode = "#8c000f" },
            new ColorEntry { name = "pale yellow", hexCode = "#ffff84" },
            new ColorEntry { name = "ochre", hexCode = "#bf9005" },
            new ColorEntry { name = "mustard yellow", hexCode = "#d2bd0a" },
            new ColorEntry { name = "light red", hexCode = "#ff474c" },
            new ColorEntry { name = "cerulean", hexCode = "#0485d1" },
            new ColorEntry { name = "pale pink", hexCode = "#ffcfdc" },
            new ColorEntry { name = "deep blue", hexCode = "#040273" },
            new ColorEntry { name = "rust", hexCode = "#a83c09" },
            new ColorEntry { name = "light teal", hexCode = "#90e4c1" },
            new ColorEntry { name = "slate", hexCode = "#516572" },
            new ColorEntry { name = "goldenrod", hexCode = "#fac205" },
            new ColorEntry { name = "dark yellow", hexCode = "#d5b60a" },
            new ColorEntry { name = "dark grey", hexCode = "#363737" },
            new ColorEntry { name = "army green", hexCode = "#4b5d16" },
            new ColorEntry { name = "grey blue", hexCode = "#6b8ba4" },
            new ColorEntry { name = "seafoam", hexCode = "#80f9ad" },
            new ColorEntry { name = "puce", hexCode = "#a57e52" },
            new ColorEntry { name = "spring green", hexCode = "#a9f971" },
            new ColorEntry { name = "dark orange", hexCode = "#c65102" },
            new ColorEntry { name = "sand", hexCode = "#e2ca76" },
            new ColorEntry { name = "pastel green", hexCode = "#b0ff9d" },
            new ColorEntry { name = "mint", hexCode = "#9ffeb0" },
            new ColorEntry { name = "light orange", hexCode = "#fdaa48" },
            new ColorEntry { name = "bright pink", hexCode = "#fe01b1" },
            new ColorEntry { name = "chartreuse", hexCode = "#c1f80a" },
            new ColorEntry { name = "deep purple", hexCode = "#36013f" },
            new ColorEntry { name = "dark brown", hexCode = "#341c02" },
            new ColorEntry { name = "taupe", hexCode = "#b9a281" },
            new ColorEntry { name = "pea green", hexCode = "#8eab12" },
            new ColorEntry { name = "puke green", hexCode = "#9aae07" },
            new ColorEntry { name = "kelly green", hexCode = "#02ab2e" },
            new ColorEntry { name = "seafoam green", hexCode = "#7af9ab" },
            new ColorEntry { name = "blue green", hexCode = "#137e6d" },
            new ColorEntry { name = "khaki", hexCode = "#aaa662" },
            new ColorEntry { name = "burgundy", hexCode = "#610023" },
            new ColorEntry { name = "dark teal", hexCode = "#014d4e" },
            new ColorEntry { name = "brick red", hexCode = "#8f1402" },
            new ColorEntry { name = "royal purple", hexCode = "#4b006e" },
            new ColorEntry { name = "plum", hexCode = "#580f41" },
            new ColorEntry { name = "mint green", hexCode = "#8fff9f" },
            new ColorEntry { name = "gold", hexCode = "#dbb40c" },
            new ColorEntry { name = "baby blue", hexCode = "#a2cffe" },
            new ColorEntry { name = "yellow green", hexCode = "#c0fb2d" },
            new ColorEntry { name = "bright purple", hexCode = "#be03fd" },
            new ColorEntry { name = "dark red", hexCode = "#840000" },
            new ColorEntry { name = "pale blue", hexCode = "#d0fefe" },
            new ColorEntry { name = "grass green", hexCode = "#3f9b0b" },
            new ColorEntry { name = "navy", hexCode = "#01153e" },
            new ColorEntry { name = "aquamarine", hexCode = "#04d8b2" },
            new ColorEntry { name = "burnt orange", hexCode = "#c04e01" },
            new ColorEntry { name = "neon green", hexCode = "#0cff0c" },
            new ColorEntry { name = "bright blue", hexCode = "#0165fc" },
            new ColorEntry { name = "rose", hexCode = "#cf6275" },
            new ColorEntry { name = "light pink", hexCode = "#ffd1df" },
            new ColorEntry { name = "mustard", hexCode = "#ceb301" },
            new ColorEntry { name = "indigo", hexCode = "#380282" },
            new ColorEntry { name = "lime", hexCode = "#aaff32" },
            new ColorEntry { name = "sea green", hexCode = "#53fca1" },
            new ColorEntry { name = "periwinkle", hexCode = "#8e82fe" },
            new ColorEntry { name = "dark pink", hexCode = "#cb416b" },
            new ColorEntry { name = "olive green", hexCode = "#677a04" },
            new ColorEntry { name = "peach", hexCode = "#ffb07c" },
            new ColorEntry { name = "pale green", hexCode = "#c7fdb5" },
            new ColorEntry { name = "light brown", hexCode = "#ad8150" },
            new ColorEntry { name = "hot pink", hexCode = "#ff028d" },
            new ColorEntry { name = "black", hexCode = "#000000" },
            new ColorEntry { name = "lilac", hexCode = "#cea2fd" },
            new ColorEntry { name = "navy blue", hexCode = "#001146" },
            new ColorEntry { name = "royal blue", hexCode = "#0504aa" },
            new ColorEntry { name = "beige", hexCode = "#e6daa6" },
            new ColorEntry { name = "salmon", hexCode = "#ff796c" },
            new ColorEntry { name = "olive", hexCode = "#6e750e" },
            new ColorEntry { name = "maroon", hexCode = "#650021" },
            new ColorEntry { name = "bright green", hexCode = "#01ff07" },
            new ColorEntry { name = "dark purple", hexCode = "#35063e" },
            new ColorEntry { name = "mauve", hexCode = "#ae7181" },
            new ColorEntry { name = "forest green", hexCode = "#06470c" },
            new ColorEntry { name = "aqua", hexCode = "#13eac9" },
            new ColorEntry { name = "cyan", hexCode = "#00ffff" },
            new ColorEntry { name = "tan", hexCode = "#d1b26f" },
            new ColorEntry { name = "dark blue", hexCode = "#00035b" },
            new ColorEntry { name = "lavender", hexCode = "#c79fef" },
            new ColorEntry { name = "turquoise", hexCode = "#06c2ac" },
            new ColorEntry { name = "dark green", hexCode = "#033500" },
            new ColorEntry { name = "violet", hexCode = "#9a0eea" },
            new ColorEntry { name = "light purple", hexCode = "#bf77f6" },
            new ColorEntry { name = "lime green", hexCode = "#89fe05" },
            new ColorEntry { name = "grey", hexCode = "#929591" },
            new ColorEntry { name = "sky blue", hexCode = "#75bbfd" },
            new ColorEntry { name = "yellow", hexCode = "#ffff14" },
            new ColorEntry { name = "magenta", hexCode = "#c20078" },
            new ColorEntry { name = "light green", hexCode = "#96f97b" },
            new ColorEntry { name = "orange", hexCode = "#f97306" },
            new ColorEntry { name = "teal", hexCode = "#029386" },
            new ColorEntry { name = "light blue", hexCode = "#95d0fc" },
            new ColorEntry { name = "red", hexCode = "#e50000" },
            new ColorEntry { name = "brown", hexCode = "#653700" },
            new ColorEntry { name = "pink", hexCode = "#ff81c0" },
            new ColorEntry { name = "blue", hexCode = "#0343df" },
            new ColorEntry { name = "green", hexCode = "#15b01a" },
            new ColorEntry { name = "purple", hexCode = "#7e1e9c" }
        };
    }
    public class ColorEntry
    {
        public string name { get; set; }
        public string hexCode { get; set; }
        public byte r { get; set; }
        public byte g { get; set; }
        public byte b { get; set; }
    }

}