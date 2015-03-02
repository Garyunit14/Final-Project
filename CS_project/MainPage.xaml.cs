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


namespace CS_project
{
  
    public sealed partial class MainPage : Page
    {
        // This is the socket we'll communicate with the gary over
        private StreamSocket s;
        private DataWriter dw;
        private DataReader input;
        private CppAccelerometer myacc;       //Accelerometer object

        private int x , y;

        
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

      
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Start connecting to Bluetooth
            //DisableAllButton();
           // SetupBluetoothLink();
            EnableAllButton();
   
            //myacc.onReadingChanged += myacc_onReadingChanged;

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
            input = new DataReader(s.InputStream);

            reset();
            
            return true;
        }

        private async void reset()
        {
            string ReadData;
            bool WhileLoopSwitch = true;

            //Disable All button
            DisableAllButton();

            //Send a char to bluetooth
            dw.WriteString("Home");
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
                    x = 0;
                    y = 0;
                    Step_TextBox.Text = ""+ 10;
                }
            }

           
 

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
            GyroButton.IsEnabled = true;

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
            GyroButton.IsEnabled = false;

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
            if (GyroButton.Content.Equals("Switch to Acc"))
            {
                GyroButton.Content = "Switch to Figure Trace";
                myacc.start();
                DisableAllButton();
            }
            else
            {
                GyroButton.Content = "Switch to Acc";
                myacc.stop();
                EnableAllButton();
            }
        }

        private void F_Click(object sender, RoutedEventArgs e)
        {            
            dw.WriteString("F" + Step_TextBox.Text  + '\n');
            dw.StoreAsync();
            Test.Text = "2";
           
        }
        private void L_Click(object sender, RoutedEventArgs e)
        {
            dw.WriteString("L" + Step_TextBox.Text);
            dw.StoreAsync();
        }
        private void B_Click(object sender, RoutedEventArgs e)
        {
            dw.WriteString("B" + Step_TextBox.Text);
            dw.StoreAsync();
        }
        private void R_Click(object sender, RoutedEventArgs e)
        {
            dw.WriteString("R" + Step_TextBox.Text);
            dw.StoreAsync();
        }
        private void U_Click(object sender, RoutedEventArgs e)
        {
            dw.WriteString("U");
            dw.StoreAsync();
        }
        private void D_Click(object sender, RoutedEventArgs e)
        {
            dw.WriteString("D");
            dw.StoreAsync();
        }

        private void F_Holding(object sender, HoldingRoutedEventArgs e)
        {
            //dw.WriteString("F" + Step_TextBox.Text);
            //dw.StoreAsync();
            while (true)
            {
                Test.Text += "1";
            }


        }
        private void L_Holding(object sender, HoldingRoutedEventArgs e)
        {

        }


/*
         
        void loop() // run over and over
        {
          string line;
          line = readFunction();
  
  
  
        }
                 string readFunction()
                 {
                       string line = "";

                      while( (a != "\n")
                      {
                         line +=mySerial.read();
                      }
  

                    return line
                }
         */




    }
}
