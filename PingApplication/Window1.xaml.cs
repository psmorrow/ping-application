using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace PingApplication
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        Timer timer = null;

        public Window1()
        {
            InitializeComponent();

            //Properties.Resources.Culture = new CultureInfo("es-ES");
            //Properties.Resources.Culture = new CultureInfo("fr-FR");

            hostTextBox.Text = GetInitialIPAddress();
            hostTextBox.Focus();
        }

        private string GetInitialIPAddress()
        {
            string initialIPAddress = "";

            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                if (networkInterface.Supports(NetworkInterfaceComponent.IPv4) && (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet || networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) && networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties ipInterfaceProperties = networkInterface.GetIPProperties();

                    if (ipInterfaceProperties.GatewayAddresses.Count > 0 && ipInterfaceProperties.GatewayAddresses[0].Address.Equals(new IPAddress(0)) == false)
                    {
                        initialIPAddress = ipInterfaceProperties.GatewayAddresses[0].Address.ToString();
                        break;
                    }
                }
            }

            return initialIPAddress;
        }

        private void HostTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                StartPing(hostTextBox.Text);
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartPing(hostTextBox.Text);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopPing();
        }

        private void StartPing(string hostName)
        {
            hostTextBox.IsEnabled = false;
            startButton.IsEnabled = false;
            stopButton.IsEnabled = true;
            stopButton.Focus();

            resultsTextBox.Text = String.Format(Properties.Resources.PingingTextFormat, hostName);

            timer = new Timer(Timer_Callback, this, 1000, 1000);
        }

        private void StopPing()
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }

            hostTextBox.IsEnabled = true;
            startButton.IsEnabled = true;
            stopButton.IsEnabled = false;
            startButton.Focus();
        }

        private delegate string GetHostNameDelegate();

        private string GetHostName()
        {
            return hostTextBox.Text;
        }

        delegate void AddResultsDelegate(string message);

        public void AddResults(string results)
        {
            resultsTextBox.Text += "\r\n" + results;
            resultsTextBox.ScrollToEnd();
        }

        private static void Timer_Callback(object obj)
        {
            Window1 window1 = (Window1)obj;

            GetHostNameDelegate getHostNameDelegate = window1.GetHostName;
            string hostName = (string)window1.Dispatcher.Invoke(getHostNameDelegate, null);

            string results = DoPing(hostName);
            Console.WriteLine(results);

            AddResultsDelegate addResultsDelegate = window1.AddResults;
            window1.Dispatcher.Invoke(addResultsDelegate, results);
        }

        private static string DoPing(string hostName)
        {
            string results = "";

            Ping ping = new Ping();

            int timeout = 100;

            PingOptions pingOptions = new PingOptions();
            pingOptions.DontFragment = true;

            string data = "This is the data that is transmitted in the ping message.";
            byte[] buffer = Encoding.ASCII.GetBytes(data);

            PingReply pingReply = ping.Send(hostName, timeout, buffer, pingOptions);
            if (pingReply.Status == IPStatus.Success)
            {
                results = String.Format(Properties.Resources.PingSuccessFormat, pingReply.Address.ToString(), pingReply.RoundtripTime);
            }
            else
            {
                results = String.Format(Properties.Resources.PingErrorFormat, hostName);
            }

            return results;
        }
    }
}
