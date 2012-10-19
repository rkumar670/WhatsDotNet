using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace WhatsDotNet.ConsoleApp
{
    class Program
    {

        static WhatsAppApi api;
        static void Main(string[] args)
        {
            api = new WhatsAppApi();
            string myNumber = System.Configuration.ConfigurationManager.AppSettings["number"];
            string myIMEI = System.Configuration.ConfigurationManager.AppSettings["IMEI"];
            api.MessageReceived += new WhatsAppApi.MessageReceivedEventHandler(api_MessageReceived);
            api.Login(myNumber, myIMEI);

            while (true)
            {
                Console.Write(">");
                string message = Console.ReadLine();
                api.SendMessage("555192192122", message);
            }

        }

        static void api_MessageReceived(object sender, string from, string messageReceived)
        {
            Console.WriteLine(from + ":" + messageReceived);
        }




    }
}
