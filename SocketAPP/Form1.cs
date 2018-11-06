using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using Newtonsoft.Json;

namespace SocketAPP
{
    public partial class Form1 : Form
    {

        //Variable
        Socket sck;
        EndPoint epLocal, epRemote;

        public Form1()
        {
            InitializeComponent();

            sck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sck.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            //set the local IP Adress int these two textboxes
            textLocalIp.Text = GetLocalIP();
            textFriendsIp.Text = GetLocalIP();
        }

        //this method will return the local IP address automatically
        private string GetLocalIP()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            //If this method can't return the IP address automatically return this IP Adress
            return "127.0.0.1";
        }


        private void MessageCallBack(IAsyncResult aResult)
        {
            try
            {
                int size = sck.EndReceiveFrom(aResult, ref epRemote);

                //this if statement will check if there is any information or not
                if (size > 0)
                {
                    //There this array will store the data
                    byte[] receivedData = new byte[1464];

                    //get the data
                    receivedData = (byte[])aResult.AsyncState;

                    //convert the byte array into string
                    ASCIIEncoding eEncoding = new ASCIIEncoding();
                    string receivedMessage = eEncoding.GetString(receivedData);

                    //display the message into the listbox
                    listMessage.Items.Add("Friend: " +receivedMessage);
                }

                byte[] buffer = new byte[1500];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }

        
        private void sendMsg()
        {
            try
            {
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                byte[] msg = new byte[1500];
                msg = enc.GetBytes(textMessage.Text);

                sck.Send(msg);
                listMessage.Items.Add("You: " + textMessage.Text);
                textMessage.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            sendMsg();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textLocalPort.Text = "80";
            textFriendPort.Text = "81";
        }


        //Here is my code for the keyDown event.
        //When the user press the Enter button the text into the textMessage textbox will be sent.
        private void textMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                sendMsg();
            }
        }

        //ColorButton
        private void button1_Click(object sender, EventArgs e)
        {
            int R, G, B, A;
            string Hex;


            if (colorDialog1.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
            {
                
                this.BackColor = colorDialog1.Color;
                R = colorDialog1.Color.R;
                G = colorDialog1.Color.G;
                B = colorDialog1.Color.B;
                A = colorDialog1.Color.A;
                Color clr = Color.FromArgb(R, G, B);
                Hex = "#" + clr.B.ToString("X2") + clr.R.ToString("X2") + clr.G.ToString("X2");

                ColorPickerCode clrCode = new ColorPickerCode(R, B, G, A, Hex);

                //Convert color code into Json the serialize it
                string strResultJson = JsonConvert.SerializeObject(clrCode);

                //write to text file in Json Format

                File.WriteAllText(@"selectedcolor.json", strResultJson);
                sendMsg();

                listMessage.Items.Add(strResultJson);
   
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                epLocal = new IPEndPoint(IPAddress.Parse(textLocalIp.Text), Convert.ToInt32(textLocalPort.Text));
                sck.Bind(epLocal);

                epRemote = new IPEndPoint(IPAddress.Parse(textFriendsIp.Text), Convert.ToInt32(textFriendPort.Text));
                sck.Connect(epRemote);


                byte[] buffer = new byte[1500];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);
                btnStart.Text = "Connected";
                btnStart.Enabled = false;
                btnSend.Enabled = true;
                textMessage.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
