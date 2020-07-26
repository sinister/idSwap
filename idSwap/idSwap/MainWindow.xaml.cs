using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
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
using CefSharp.Wpf;

namespace idSwap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        string sSession1 = ""; // SessionID for browser1
        string sSession2 = ""; // same as above but for browser2
        string sOldID = "";
        string sNewID = "";
        bool oldVersion = false;
        /*
         Some comments might be outdated. shanty#0001 if you need me.
         */
        public MainWindow()
        {
            InitializeComponent();
            RequestContextSettings requestContextSettings = new RequestContextSettings();
            requestContextSettings.PersistSessionCookies = false;
            requestContextSettings.PersistUserPreferences = false;
            cef1.RequestContext = new RequestContext(requestContextSettings); // Makes browser1 have its own seperate context settings, essentially cookies
            cef2.RequestContext = new RequestContext(requestContextSettings); // Same as above but for browser2
        }

        async private Task SetProxy(ChromiumWebBrowser cwb, string Address)
        {
            await Cef.UIThreadTaskFactory.StartNew(delegate
            {

                var rc = cwb.GetBrowser().GetHost().RequestContext;
                var v = new Dictionary<string, object>();
                v["mode"] = "fixed_servers";
                v["server"] = Address;
                string error;
                bool success = rc.SetPreference("proxy", v, out error);
            });
        }

        private async void BtnUpdateInfo_Click(object sender, RoutedEventArgs e) // UpdateInfo button
        {
            await UpdateInfo();
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

        private void BtnPing_Click(object sender, RoutedEventArgs e)
        {
            pingCheck();
        }

        private void pingCheck()
        {
            long Google = new Ping().Send("www.google.com").RoundtripTime;
            long Steam = new Ping().Send("www.steamcommunity.com").RoundtripTime;
            tbxInfo.AppendText("Ping to google.com - " + Google.ToString() + "ms" + Environment.NewLine);
            tbxInfo.AppendText("Ping to steamcommunity.com - " + Steam.ToString() + "ms" + Environment.NewLine);
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            tbxInfo.Clear();
        }

        private void SldLoopNum_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            tbLoops.Text = sldLoopNum.Value + " Loops: ";
        }

        private async void BtnSetProxy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(tbxLBP.Text) && tbxLBP.Text != "IP:PORT")
                {
                    await SetProxy(cef1, tbxLBP.Text);
                }
                if (!string.IsNullOrWhiteSpace(tbxRBP.Text) && tbxRBP.Text != "IP:PORT")
                {
                    await SetProxy(cef2, tbxRBP.Text);
                }
            }
            catch (Exception ex)
            {
                tbxInfo.AppendText(ex.ToString() + Environment.NewLine);
            }
        }

        private void BtnWIMP_Click(object sender, RoutedEventArgs e)
        {
            cef1.Load("https://whatismyipaddress.com/");
            cef2.Load("https://whatismyipaddress.com/");
        }

        private void BtnSteam_Click(object sender, RoutedEventArgs e)
        {
            cef1.Load("https://steamcommunity.com/login/");
            cef2.Load("https://steamcommunity.com/login/");
        }

        private void BtnOldVersion_Click(object sender, RoutedEventArgs e)
        {
            oldVersion = !oldVersion;
            if (oldVersion)
            {
                btnSwap.Visibility = Visibility.Hidden;
                sldLoopNum.Visibility = Visibility.Hidden;
                tbLoops.Visibility = Visibility.Hidden;
                btnUpdateInfo.Visibility = Visibility.Visible;
                btnLeft.Visibility = Visibility.Visible;
                btnRight.Visibility = Visibility.Visible;
            }
            else
            {
                btnSwap.Visibility = Visibility.Visible;
                sldLoopNum.Visibility = Visibility.Visible;
                tbLoops.Visibility = Visibility.Visible;
                btnUpdateInfo.Visibility = Visibility.Hidden;
                btnLeft.Visibility = Visibility.Hidden;
                btnRight.Visibility = Visibility.Hidden;
            }
        }

        private async void BtnSwap_Click(object sender, RoutedEventArgs e)
        {
            await UpdateInfo();

            int numLoop = (int)sldLoopNum.Value;
            await Task.Delay(1000);
            cef1.Address = cef1.Address + "?sessionID=" + sSession1 + "&type=profileSave&customURL=" + sNewID; // Sets the CustomURl to the random gen one
            for (int i = 0; i < numLoop; i++)
            {
                await Task.Delay(10);
                cef2.Address = cef2.Address + "?sessionID=" + sSession2 + "&type=profileSave&customURL=" + sOldID; // Sets ID to the CustomURL which was on browser1
            }

        }

        private async Task UpdateInfo()
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
            task = cef1.EvaluateScriptAsync("(function() {return document.querySelector(\"#application_root > div.profileeditshell_Shell_2kqKZ > div.profileeditshell_PageContent_23XE6 > form > div:nth-child(11) > div.profileedit_ProfileBoxContent_3s6BB > div:nth-child(3) > label > div.DialogInput_Wrapper._DialogLayout > input\").value;})();");

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
    }
}
