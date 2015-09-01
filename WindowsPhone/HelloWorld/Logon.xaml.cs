using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using HelloWorld.OAuth2;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace HelloWorld
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Logon : Page
    {
        public static void Navigate(Config config)
        {
            App.Navigate(typeof(Logon), config);
        }

        public Logon()
        {
            this.InitializeComponent();
            webView.NavigationStarting += NavigationStarting;
            webView.NavigationCompleted += NavigationCompleted;
        }

        private void NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
        }

        private void NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            // invalid state
            if (Current == null)
            {
                args.Cancel = true;
                MainPage.Navigate();
                return;
            }
            // oauth authorization server has completed
            if (args.Uri.Authority == new Uri(Current.REDIRECT_URI).Authority)
            {
                args.Cancel = true;
                MainPage.Navigate(Current, args.Uri, State);
                return;
            }
            // prevent navigation out of bounds...
            if (args.Uri.Authority != new Uri(Current.AUTHORIZE_URI).Authority)
            {
                args.Cancel = true;
                MainPage.Navigate();
                return;
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            Current = await RelyingParty.DoDiscovery(e.Parameter as Config);
            if (Current == null)
            {
                throw new ArgumentNullException();
            }
            State = Guid.NewGuid();
            // launch authorization request in embedded browser
            webView.Navigate(RelyingParty.NewAuthnRequest(Current, State));
        }

        private Guid State { get; set; }
        private Config Current { get; set; }

        private RelyingParty RelyingParty
        {
            get { return (App.Current as App).RelyingParty; }
        }

    }
}
