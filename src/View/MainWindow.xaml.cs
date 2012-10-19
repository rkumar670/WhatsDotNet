using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Threading;

namespace WhatsDotNetView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WhatsDotNet.WhatsAppApi api;
        WhatsDotNetView.Login loginControl;
        WhatsDotNetView.Contacts contactsControl;
        Dictionary<string, Conversation> conversations;
        ManualResetEvent conversationStarted;

        public delegate void StartConversation(string to);
        public delegate void MessageReceived(string from, string message);
        public MainWindow()
        {
            InitializeComponent();
            loginControl = new Login();
            loginControl.LoginButtonClicked += new EventHandler(Login_LoginButtonClicked);
            windowGrid.Children.Add(loginControl);
            conversations = new Dictionary<string, Conversation>();
            api = new WhatsDotNet.WhatsAppApi();
            conversationStarted = new ManualResetEvent(false);
            api.MessageReceived += new WhatsDotNet.WhatsAppApi.MessageReceivedEventHandler(api_MessageReceived);
        }

        void api_MessageReceived(object sender, string from, string messageReceived)
        {
            conversationStarted.Reset();
            if(!conversations.Keys.Contains(from))
            {
                Dispatcher.Invoke(new StartConversation(CreateNewConversation), from);
            }

            Dispatcher.Invoke(new MessageReceived(ReceiveMessage), from, messageReceived);
            
            
        }

        void ReceiveMessage(string from, string messageReceived)
        {
            conversations[from].ReceiveMessage(messageReceived);
        }

        private void Login_LoginButtonClicked(object sender, EventArgs e)
        {
            bool result = api.Login(loginControl.Number, loginControl.IMEI);
            if (result)
            {
                MessageBox.Show("Logged In!");
                windowGrid.Children.Remove(loginControl);
                contactsControl = new Contacts();
                contactsControl.StartConversation += new Contacts.StartConversationEventHandler(contactsControl_StartConversation);
                windowGrid.Children.Add(contactsControl);

            }
            else
            {
                MessageBox.Show("Error!");
            }
        }

        void contactsControl_StartConversation(object sender, string to)
        {
            Dispatcher.BeginInvoke(new StartConversation(CreateNewConversation), to);  
        }

        void CreateNewConversation(object o)
        {
            string to = (string)o;
            Conversation c = new Conversation(api, to);
            conversations.Add(to, c);
            c.Show();
            conversationStarted.Set();
        }
    }
}
