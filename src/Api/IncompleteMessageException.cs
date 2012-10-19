using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhatsDotNet
{
    class IncompleteMessageException : Exception
    {
        private string input;
        public IncompleteMessageException(string message)
            : base(message)
        {
        }

        public void SetInput(string input)
        {
            this.input = input;
        }

        public string GetInput()
        {
            return input;
        }


    }
}
