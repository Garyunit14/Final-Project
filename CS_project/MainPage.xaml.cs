using System;
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
using System.Text;


namespace CS_project
{
  
    public sealed partial class MainPage : Page
    {
        // This is the socket we'll communicate with the DavidBT  over
        private StreamSocket s;
        private DataWriter dw;
        private DataReader input;
        private CppAccelerometer myacc;       //Accelerometer object

        private byte Signal = 0;

        private byte stop = 0;
        private byte F = 32;
        private byte R = 64;
        private byte B = 96;
        private byte L = 128;
        private byte U = 160;
        private byte D = 192;
        private byte H = 224;

        private Int16 UserSpeed = 0;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

      
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Start connecting to Bluetooth
            ReadData(SetupBluetoothLink_David());

           myacc = new CppAccelerometer();
           
           //EnableAllButton();
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

            byte Temp_byte= SendToBT(x, y, z);
            
/*      
            await input.LoadAsync(4);
            input.ReadBytes(byteBuffer);
            int length = BitConverter.ToInt32(byteBuffer, 0);
*/
            

            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TestBlock.Text = 
                Acc_TestBlock2.Text = PrintDirection(Temp_byte) + " With Speed of " + UserSpeed;
                Acc_TestBlock.Text = "x: " + Math.Round(x,2) + " y: " + Math.Round(y,2) + " z: " + Math.Round(z,2);
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
                await new MessageDialog("No bluetooth devices are paired, please pair your DavidBT").ShowAsync();

                // Neat little line to open the bluetooth settings
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-bluetooth:"));
                return false;
            }

            // Convert peers to array from strange datatype return from PeerFinder.FindAllPeersAsync()
            PeerInformation[] peers = devices.ToArray();

            // Find paired peer that is the DavidBT
            PeerInformation peerInfo = devices.FirstOrDefault(c => c.DisplayName.Contains("DavidBT"));

            // If that doesn't exist, complain!
            if (peerInfo == null)
            {
                await new MessageDialog("No bluetooth devices are paired, please pair your DavidBT").ShowAsync();
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-bluetooth:"));
                return false;
            }

            // Otherwise, create our StreamSocket and connect it!
            s = new StreamSocket();
            dw = new DataWriter(s.OutputStream);
            await s.ConnectAsync(peerInfo.HostName, "1");

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
                await new MessageDialog("No bluetooth devices are paired, please pair your DavidBT ").ShowAsync();

                // Neat little line to open the bluetooth settings
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-bluetooth:"));
                return false;
            }

            // Convert peers to array from strange datatype return from PeerFinder.FindAllPeersAsync()
            PeerInformation[] peers = devices.ToArray();

            // Find paired peer that is the DavidBT 
            PeerInformation peerInfo = devices.FirstOrDefault(c => c.DisplayName.Contains("DavidBT"));

            // If that doesn't exist, complain!
            if (peerInfo == null)
            {
                await new MessageDialog("No bluetooth devices are paired, please pair your DavidBT ").ShowAsync();
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
        private async Task<bool> SetupBluetoothLink_David()
        {
            // Tell PeerFinder that we're a pair to anyone that has been paried with us over BT
            PeerFinder.AlternateIdentities["Bluetooth:PAIRED"] = "";

            // Find all peers
            var devices = await PeerFinder.FindAllPeersAsync();

            // If there are no peers, then complain
            if (devices.Count == 0)
            {
                await new MessageDialog("No bluetooth devices are paired, please pair your DavidBT").ShowAsync();

                // Neat little line to open the bluetooth settings
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-bluetooth:"));
                return false;
            }

            // Convert peers to array from strange datatype return from PeerFinder.FindAllPeersAsync()
            PeerInformation[] peers = devices.ToArray();

            // Find paired peer that is the DavidBT
            PeerInformation peerInfo = devices.FirstOrDefault(c => c.DisplayName.Contains("DavidBT"));

            // If that doesn't exist, complain!
            if (peerInfo == null)
            {
                await new MessageDialog("No bluetooth devices are paired, please pair your DavidBT").ShowAsync();
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-bluetooth:"));
                return false;
            }

            // Otherwise, create our StreamSocket and connect it!
            s = new StreamSocket();

            // Construct a dataReader so we can read junk in
            dw = new DataWriter(s.OutputStream);
            await s.ConnectAsync(peerInfo.HostName, "1");
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
            dw.WriteByte(H);
            TestBlock.Text= "Home buttom Click";
            await dw.StoreAsync();

            
            

            while(WhileLoopSwitch)
            {  
               //wait until recieving a singal from bluetooth 
               ReadData = (await readLine(input));
               
                if (ReadData.Equals("Done") )  
                {
                    Signal = 0;
                    UserSpeed = 0;
                    //Able All button again
                    EnableAllButton();
                    WhileLoopSwitch = false;
                    Step_TextBox.Text = ""+ 1;
                }
            }

           
 

        }

        private async void SendToBT(HoldingRoutedEventArgs e, byte direction , Int16 speed)
        {
            Signal = 0;

            if (e.HoldingState == Windows.UI.Input.HoldingState.Canceled)
            {
                Signal = stop;
                TestBlock.Text = PrintDirection(Signal) + ": Canceled";
            }
            else if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                Signal = (byte)(direction + speed);
                TestBlock.Text = PrintDirection(direction) + ": Started with " + PrintSpeed( speed);
            }
            else if (e.HoldingState == Windows.UI.Input.HoldingState.Completed)
            {
                Signal = stop;
                TestBlock.Text = PrintDirection(Signal) + ": Completed";
            }

           dw.WriteByte(Signal);
           await dw.StoreAsync();
        }
        private async void SendToBT(byte direction)
        {
            dw.WriteByte(direction);
            await dw.StoreAsync();
        }
        private byte SendToBT(Double x, Double y, Double z)
        {
            Signal = 0;
            Byte Direction = 0 ;

            if(z <= 0.0) // phone is upward
            {
                if (Math.Abs(x) <= 0.225 && Math.Abs(y) <= 0.225)  // Stop
                {
                    Signal = stop;
                }
                else if( x > 0.225 && Math.Abs(y) < 0.5)  //forward
                {
                    Direction = F;
                    //UserSpeed = Convert.ToInt16( Math.Round( Math.Abs(x)/0.05 , 2) - 1);
                    UserSpeed = Convert.ToInt16(Math.Round((Math.Abs(x) * 40 - 8),0));
                    Signal = (byte) (F + UserSpeed); // forward direction
                }
                else if(Math.Abs(x) < 0.5 && y < -0.225)   //Right
                {
                    Direction = R;
                    //UserSpeed = Convert.ToInt16( Math.Round( Math.Abs(y)/0.2 , 0) - 1);
                    UserSpeed = Convert.ToInt16(Math.Round((Math.Abs(y) * 40 - 8), 0));
                    Signal =  (byte) (R + UserSpeed);         
                }
                else if (x < -0.225 && Math.Abs(y) < 0.5)  //Back
                {
                    Direction = B;
                    //UserSpeed = Convert.ToInt16( Math.Round( Math.Abs(x)/0.2 , 0) - 1);
                    UserSpeed = Convert.ToInt16(Math.Round((Math.Abs(x) * 40 - 8), 0));
                    Signal = (byte) (B + UserSpeed);
                }
                else if (Math.Abs(x) < 0.5 && y > 0.225)  //Left
                {
                    Direction = L;
                    //UserSpeed = Convert.ToInt16( Math.Round( Math.Abs(y)/0.2 , 0) - 1);
                    UserSpeed = Convert.ToInt16(Math.Round((Math.Abs(y) * 40 - 8), 0));
                    Signal = (byte) (L + UserSpeed);
                }

            }
            else  // phone face downward
            {
                    Signal = stop;   // then stop
            }

            SendToBT(Signal);
            return Direction;

        }

        private string PrintDirection(byte direction)
        {
             String Command = System.String.Empty;
             
             switch (direction)
             {
                 case 0: //Stop
                     Command = "Stop";
                     break;

                 case  32: //Forward
                    Command= "Forward  x: 0, y: 0, z: 0";
                    break;

                 case 64: //Right
                    Command = "Right  x: 0, y: -1, z: 0";
                    break;

                 case 96: //Back
                     Command = "Back  x: -1, y: 0, z: 0" ;
                     break;

                 case 128: //Left
                     Command = "Left  x: 0, y: 1, z: 0";
                     break;

                 case 160:
                     Command = "UP";
                     break;

                 case 192:
                     Command="DOWN";
                     break;


                default: 
                    Command = "";
                    break;
                    
            }

             return Command;
        }
        private string PrintSpeed(Int16 speed)
        {
            String Command = System.String.Empty;

             switch (speed)
             {
                 case 0:
                     Command = "No Speed";
                     break;

                 case 1:
                     Command = "Speed of 1";
                     break;

                 case  2:
                    Command= "Speed of 2";
                    break;

                 case 3:
                    Command = "Speed of 3";
                    break;

                 case 4:
                    Command = "Speed of 4";
                    break;

                 case 5:
                    Command = "Speed of 5";
                    break;

                 case 6:
                    Command = "Speed of 6";
                    break;

                 case 7:
                    Command = "Speed of 7";
                    break;

                 case 8:
                    Command = "Speed of 8";
                    break;

                 case 9:
                     Command = "Speed of 9" ;
                     break;

                 case 10:
                     Command = "Speed of 10";
                     break;

                 case 11:
                     Command = "Speed of 11";
                     break;

                 case 12:
                     Command = "Speed of 12";
                     break;

                 case 13:
                     Command = "Speed of 13";
                     break;

                 case 14:
                     Command = "Speed of 14";
                     break;

                 case 15:
                     Command = "Speed of 15";
                     break;

                 case 16:
                     Command = "Speed of 16";
                     break;

                default:
                     Command = "";
                    break;
                    
            }
            return Command;
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
        private async void ReadData(Task<bool> setupOK)
        {
            // Wait for the setup function to finish, when it does, it returns a boolean
            // If the boolean is false, then something failed and we shouldn't attempt to read data
            if (!await setupOK)
                return;

            // Construct a dataReader so we can read junk in
            DataReader input = new DataReader(s.InputStream);

            // Loop forever
            while (true)
            {
                // Read a line from the input, once again using await to translate a "Task<xyz>" to an "xyz"
                string line = (await readLine(input));

                // Append that line to our TextOutput
                TestBlock.Text += line + "\n";
            }
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
                SendToBT(stop);
                AccButton.Content = "Switch to Button";
                DisableAllButton();
                U_Button.IsEnabled = true;
                D_Button.IsEnabled = true;
                AccButton.IsEnabled = true;
                myacc.onReadingChanged += myacc_onReadingChanged;

            }
            else
            {
                SendToBT(stop);
                AccButton.Content = "Switch to Acc";
                myacc.onReadingChanged -= myacc_onReadingChanged;
                EnableAllButton();
            }
        }

        private void U_Click(object sender, RoutedEventArgs e)
        {
            SendToBT(U);
            TestBlock.Text = PrintDirection(U) + ": Clicked";
            
        }
        private void D_Click(object sender, RoutedEventArgs e)
        {
            SendToBT(D);
            TestBlock.Text = PrintDirection(D) + ": Clicked";
        }

        private void F_Holding(object sender, HoldingRoutedEventArgs e)
        {
            try
            {
               if( CheckValue(Convert.ToByte(Step_TextBox.Text)) )
               {
                    UserSpeed = Convert.ToInt16(Step_TextBox.Text);
                    SendToBT(e, F,UserSpeed);
               }
               else 
               {
                   Display.Text = "Please type in 1 ~ 16 Number";
               }

            }
            catch
            {
                Display.Text = "Please type in Valid Value";
            }

        }
        private void L_Holding(object sender, HoldingRoutedEventArgs e)
        {
            try
            {
               if( CheckValue(Convert.ToByte(Step_TextBox.Text)) )
               {
                    UserSpeed = Convert.ToInt16(Step_TextBox.Text);
                    SendToBT(e, L,UserSpeed);
               }
               else
               {
                   Display.Text = "Please type in 1 ~ 16 Number";
               }

            }
            catch
            {
                Display.Text = "Please type in Valid Value";
            }
        }
        private void R_Holding(object sender, HoldingRoutedEventArgs e)
        {
            try
            {
               if( CheckValue(Convert.ToByte(Step_TextBox.Text)) )
               {
                    UserSpeed = Convert.ToInt16(Step_TextBox.Text);
                    SendToBT(e, R,UserSpeed);
               }
               else
               {
                   Display.Text = "Please type in 1 ~ 16 Number";
               }

            }
            catch
            {
                Display.Text = "Please type in Valid Value";
            }
        }
        private void B_Holding(object sender, HoldingRoutedEventArgs e)
        {
            try
            {
               if( CheckValue(Convert.ToByte(Step_TextBox.Text)) )
               {
                    UserSpeed = Convert.ToInt16(Step_TextBox.Text);
                    SendToBT(e, B,UserSpeed);
               }
               else
               {
                   Display.Text = "Please type in 1 ~ 16 Number";
               }

            }
            catch
            {
                Display.Text = "Please type in Valid Value";
            }
        }
        private bool CheckValue(byte number)
        {
            if (number >= 1 && number <= 16)
            {  //1 <= number <= 16
                return true;
            }
            else
            {
                return false;
            }
        }
    }


}
