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
using System.Windows.Shapes;

namespace WhatsDotNetView
{
    /// <summary>
    /// Interaction logic for Conversation.xaml
    /// </summary>
    public partial class Conversation : Window
    {
        public string To { get; set; }
        private WhatsDotNet.WhatsAppApi api;
        public Conversation(WhatsDotNet.WhatsAppApi api, string to)
        {
            InitializeComponent();
            this.api = api;
            this.To = to;
            lblHint.Content = "Conversation with " + To;
        }

        private void txtTextToSend_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                api.SendMessage(To, txtTextToSend.Text);
                AddChat("Me", txtTextToSend.Text);
                txtTextToSend.Text = "";
            }
        }

        public void ReceiveMessage(string messageReceived)
        {
            AddChat(To, messageReceived);
        }

        public void AddChat(string person, string message)
        {
            txtConversation.Text += person;
            txtConversation.Text += ": ";
            txtConversation.Text += message;
            txtConversation.Text += "\n";
        }
    }
}
