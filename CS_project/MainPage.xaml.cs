﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using CppSensors;
using Windows.UI.Input;
using Windows.UI.Core;


namespace CS_project
{
  
    public sealed partial class MainPage : Page
    {
        // This is the socket we'll communicate with the gary  over
        private StreamSocket s;
        private DataWriter dw;
        private DataReader input;
        private CppAccelerometer myacc;       //Accelerometer object

        byte Signal;

        
        
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

      
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Start connecting to Bluetooth
            //SetupBluetoothLink1();

           myacc = new CppAccelerometer();
           EnableAllButton();
           
        }

        void myacc_onReadingChanged(double x, double y, double z)
        {

            /*
             * (0,0,-1) flat on table
             * 
             * (-1,0,0) backward
             * (1,0,0) forward
             * 
             * (0,1,0) Left
             * (0,-1,0) Right
             * 
             */
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TestBlock.Text = "x:" + Math.Round(x,2) + " y: " + Math.Round(y,2) + " z: " + Math.Round(z,2);
            });

        }

        private async Task<bool> SetupBluetoothLink1()
        {
            // Tell PeerFinder that we're a pair to anyone that has been paried with us over BT
            PeerFinder.AlternateIdentities["Bluetooth:PAIRED"] = "";

            // Find all peers
            var devices = await PeerFinder.FindAllPeersAsync();

            // If there are no peers, then complain
            if (devices.Count == 0)
            {
                await new MessageDialog("No bluetooth devices are paired, please pair your gary").ShowAsync();

                // Neat little line to open the bluetooth settings
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-bluetooth:"));
                return false;
            }

            // Convert peers to array from strange datatype return from PeerFinder.FindAllPeersAsync()
            PeerInformation[] peers = devices.ToArray();

            // Find paired peer that is the gary
            PeerInformation peerInfo = devices.FirstOrDefault(c => c.DisplayName.Contains("gary"));

            // If that doesn't exist, complain!
            if (peerInfo == null)
            {
                await new MessageDialog("No bluetooth devices are paired, please pair your gary").ShowAsync();
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-bluetooth:"));
                return false;
            }

            // Otherwise, create our StreamSocket and connect it!
            s = new StreamSocket();
            await s.ConnectAsync(peerInfo.HostName, "1");
            dw = new DataWriter(s.OutputStream);
            EnableAllButton();
            return true;
        }
        private async Task<bool> SetupBluetoothLink()
        {
            // Tell PeerFinder that we're a pair to anyone that has been paried with us over BT
            PeerFinder.AlternateIdentities["Bluetooth:PAIRED"] = "";

            // Find all peers
            var devices = await PeerFinder.FindAllPeersAsync();

            // If there are no peers, then complain
            if (devices.Count == 0)
            {
                await new MessageDialog("No bluetooth devices are paired, please pair your gary ").ShowAsync();

                // Neat little line to open the bluetooth settings
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-bluetooth:"));
                return false;
            }

            // Convert peers to array from strange datatype return from PeerFinder.FindAllPeersAsync()
            PeerInformation[] peers = devices.ToArray();

            // Find paired peer that is the gary 
            PeerInformation peerInfo = devices.FirstOrDefault(c => c.DisplayName.Contains("gary"));

            // If that doesn't exist, complain!
            if (peerInfo == null)
            {
                await new MessageDialog("No bluetooth devices are paired, please pair your gary ").ShowAsync();
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-bluetooth:"));
                return false;
            }

            // Otherwise, create our StreamSocket and connect it!
            s = new StreamSocket();
            await s.ConnectAsync(peerInfo.HostName, "1");
            dw = new DataWriter(s.OutputStream);
            input = new DataReader(s.InputStream);

            //reset();
            EnableAllButton();

            return true;
        }

        private async void reset()
        {
            string ReadData;
            bool WhileLoopSwitch = true;

            //Disable All button
            DisableAllButton();

            //Send a char to bluetooth
            dw.WriteByte((byte) 'H');
            TestBlock.Text= "Home buttom Click";
            //dw.WriteByte(0x72);   // 0x72 same as "r"
            await dw.StoreAsync();

            
            

            while(WhileLoopSwitch)
            {  
               //wait until recieving a singal from bluetooth 
               ReadData = (await readLine(input));
               
                if (ReadData.Equals("Done") )  
                {
                    //Able All button again
                    EnableAllButton();
                    WhileLoopSwitch = false;
                    Step_TextBox.Text = ""+ 10;
                }
            }

           
 

        }
        private async void SendToBT(HoldingRoutedEventArgs e, char character)
        {
            Signal = (byte) character;

            if (e.HoldingState == Windows.UI.Input.HoldingState.Canceled)
            {
                Signal = (byte) 'S';
                TestBlock.Text = "" + character + ": Canceled";
            }
            else if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                Signal = (byte)character;
                TestBlock.Text = "" + character + ": Started";
            }
            else if (e.HoldingState == Windows.UI.Input.HoldingState.Completed)
            {
                Signal = (byte)'S';
                TestBlock.Text = "" + character + ": Completed";
            }

           //dw.WriteByte(Signal);
           //await dw.StoreAsync();
        }
        private async void SendToBT(char character)
        {
            Signal = (byte) character;
            //dw.WriteByte(Signal);
            //await dw.StoreAsync();
        }

        private async Task<string> readLine(DataReader input)
        {
            string line = "";
            char a = ' ';
            // Keep looping as long as we haven't hit a newline OR line is empty
            while ((a != '\n' && a != '\r') || line.Length == 0)
            {
                // Wait until we have 1 byte available to read
                await input.LoadAsync(1);
                // Read that one byte, typecasting it as a char
                a = (char)input.ReadByte();

                // If the char is a newline or a carriage return, then don't add it on to the line and quit out
                if (a != '\n' && a != '\r')
                    line += a;
            }

            // Return the string we've built
            return line;
        }

        private void EnableAllButton()
        {
            Home.IsEnabled = true;
            AccButton.IsEnabled = true;

            F_Button.IsEnabled = true;
            B_Button.IsEnabled = true;
            R_Button.IsEnabled = true;
            L_Button.IsEnabled = true;
            U_Button.IsEnabled = true;
            D_Button.IsEnabled = true;
            Step_TextBox.IsEnabled = true;



        }
        private void DisableAllButton()
        {
            Home.IsEnabled = false;
            //AccButton.IsEnabled = false;

            F_Button.IsEnabled = false;
            B_Button.IsEnabled = false;
            R_Button.IsEnabled = false;
            L_Button.IsEnabled = false;
            U_Button.IsEnabled = false;
            D_Button.IsEnabled = false;
            Step_TextBox.IsEnabled = false;


        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            reset();
        }

        private void Acc_Button_Click(object sender, RoutedEventArgs e)
        {
            if (AccButton.Content.Equals("Switch to Acc"))
            {
                SendToBT('S');
                AccButton.Content = "Switch to Button";
                DisableAllButton();
                U_Button.IsEnabled = true;
                D_Button.IsEnabled = true;
                AccButton.IsEnabled = true;
                myacc.onReadingChanged += myacc_onReadingChanged;

            }
            else
            {
                SendToBT('S');
                AccButton.Content = "Switch to Acc";
                myacc.onReadingChanged -= myacc_onReadingChanged;
                EnableAllButton();
            }
        }

        private async void U_Click(object sender, RoutedEventArgs e)
        {
            dw.WriteByte((byte) 'U');
            await dw.StoreAsync();
        }
        private async void D_Click(object sender, RoutedEventArgs e)
        {
            dw.WriteByte((byte) 'D');
            await dw.StoreAsync();
        }

        private void F_Holding(object sender, HoldingRoutedEventArgs e)
        {
            SendToBT(e, 'F');
        }
        private void L_Holding(object sender, HoldingRoutedEventArgs e)
        {
            SendToBT(e, 'L');
        }
        private void R_Holding(object sender, HoldingRoutedEventArgs e)
        {
            SendToBT(e, 'R');
        }
        private void B_Holding(object sender, HoldingRoutedEventArgs e)
        {
            SendToBT(e, 'B');
        }

    }


}
