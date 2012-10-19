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

namespace WhatsDotNetView
{
    /// <summary>
    /// Interaction logic for Contacts.xaml
    /// </summary>
    public partial class Contacts : UserControl
    {
        public Contacts()
        {
            InitializeComponent();
        }

        private void txtTo_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                StartConversation(this, txtTo.Text);
            }
        }

        public delegate void StartConversationEventHandler(object sender, string to);

        public event StartConversationEventHandler StartConversation;
    }
}
