using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CefSharp;

namespace idSwap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string sSession1 = ""; // SessionID for browser1
        string sSession2 = ""; // same as above but for browser2
        string sOldID = "";
        string sNewID = "";
        public MainWindow()
        {
            InitializeComponent();
            RequestContextSettings requestContextSettings = new RequestContextSettings();
            requestContextSettings.PersistSessionCookies = false;
            requestContextSettings.PersistUserPreferences = false;
            cef1.RequestContext = new RequestContext(requestContextSettings); // Makes browser1 have its own seperate context settings, essentially cookies
            cef2.RequestContext = new RequestContext(requestContextSettings); // Same as above but for browser2
        }

        private async void BtnUpdateInfo_Click(object sender, RoutedEventArgs e) // UpdateInfo button
        {
            cef1.ExecuteScriptAsync("document.getElementsByClassName('whiteLink')[1].click()"); // Reloads profile, uses link instead of normal reload
            cef2.ExecuteScriptAsync("document.getElementsByClassName('whiteLink')[1].click()"); // for reason stated above
            tbxInfo.Clear();

            // Gets the sessionid for browser1
            var task = cef1.EvaluateScriptAsync("g_sessionID"); 

            await task.ContinueWith(t =>
            {
                if (!t.IsFaulted)
                {
                    var response = t.Result;

                    if (response.Success && response.Result != null)
                    {
                        sSession1 = response.Result.ToString();
                    }
                }
            });
            tbxInfo.AppendText("B1 Session ID: " + sSession1 + Environment.NewLine);

            // Gets the sessionid for browser2
            task = cef2.EvaluateScriptAsync("g_sessionID");

            await task.ContinueWith(t =>
            {
                if (!t.IsFaulted)
                {
                    var response = t.Result;

                    if (response.Success && response.Result != null)
                    {
                        sSession2 = response.Result.ToString();
                    }
                }
            });
            tbxInfo.AppendText("B2 Session ID: " + sSession2 + Environment.NewLine);

            // Gets the CustomURL on browser1
            task = cef1.EvaluateScriptAsync("(function() {return document.getElementById('customURL').value;})();");

            await task.ContinueWith(t =>
            {
                if (!t.IsFaulted)
                {
                    var response = t.Result;

                    if (response.Success && response.Result != null)
                    {
                        sOldID = response.Result.ToString();
                    }
                }
            });

            tbxInfo.AppendText("Custom URL to swap: " + sOldID + Environment.NewLine);

            // Generates a random CustomURL to set on browser1
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_";
            var stringChars = new char[32];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            sNewID = new String(stringChars);
            tbxInfo.AppendText("URL to change to: " + sNewID + Environment.NewLine);
        }

        private void BtnLeft_Click(object sender, RoutedEventArgs e) // Left button
        {
            if (!string.IsNullOrWhiteSpace(sSession1))
            {
                cef1.Address = cef1.Address + "?sessionID=" + sSession1 + "&type=profileSave&customURL=" + sNewID; // Sets the CustomURl to the random gen one
            }
        }

        private void BtnRight_Click(object sender, RoutedEventArgs e) // Right button
        {
            cef2.Address = cef2.Address + "?sessionID=" + sSession2 + "&type=profileSave&customURL=" + sOldID; // Sets ID to the CustomURL which was on browser1
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e) // Refresh button
        {
            if (cef1.Address.Contains(@"/edit"))
            {
                cef1.ExecuteScriptAsync("document.getElementsByClassName('whiteLink')[1].click()"); // Clicks the edit profile link as otherwise reloading after
            }                                                                                       // ID has changed will result in profile not being found
            else
            {
                cef1.GetBrowser().Reload();
            }
            if (cef2.Address.Contains(@"/edit"))
            {
                cef2.ExecuteScriptAsync("document.getElementsByClassName('whiteLink')[1].click()");
            }
            else
            {
                cef2.GetBrowser().Reload();
            }
        }
    }
}
