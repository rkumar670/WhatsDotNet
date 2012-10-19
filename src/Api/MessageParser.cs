using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhatsDotNet
{
    public class MessageParser
    {
        private int position = 0;

        public ProtocolNode ParseNode(byte[] message)
        {

            int stanzaSize = readInt16(message);
            if (stanzaSize > 0)
            {
                return ParseNodeInternal(message);
            }
            else
            {
                return null;
            }

        }

        private ProtocolNode ParseNodeInternal(byte[] message)
        {
            int integerType = readInt8(message);
            int size = readListSize(integerType, message);
            byte nodeType = readInt8(message);

            Dictionary<string, string> attributes = null;

            if (nodeType == 2)
            {
                return null;
            }
            else
            {
                string tag = getNodeTag(nodeType); 
                attributes = readAttributes(message, size);

                if ((size % 2) == 1)
                {
                    return new ProtocolNode(tag, attributes, null, "");
                }
                byte isListOrString = readInt8(message);

                if (isListTag(isListOrString))
                {
                    return new ProtocolNode(tag, attributes, readList(isListOrString, message), "");
                }
                else
                {
                    return new ProtocolNode(tag, attributes, null, readString(message, isListOrString));
                }
            }
        }

        protected bool isListTag(int token)
        {
            return ((token == 0xf8) || (token == 0) || (token == 0xf9));
        }

        private int readListSize(int integerType, byte[] message)
        {
            int size = 0;
            if (integerType == 0xf8)
            {
                size = readInt8(message);

            }
            else if (integerType == 0xf9)
            {
                size = readInt16(message);
            }
            else
            {
                throw new Exception("BinTreeNodeReader.readListSize: Invalid token  token");
            }
            return size;
        }

        protected List<ProtocolNode> readList(int integerType, byte[] message)
        {
            int size = readListSize(integerType, message);
            List<ProtocolNode> ret = new List<ProtocolNode>();
            for (int i = 0; i < size; i++)
            {
                ret.Add(ParseNodeInternal(message));
            }
            return ret;
        }

        protected byte readInt8(byte[] message)
        {
            byte ret = 0;
            if (message.Length >= 1)
            {
                ret = (byte)message[position];
                position++;
            }
            
            return ret;
        }

        protected ushort readInt16(byte[] message)
        {
            int ret = 0;
            if (message.Length >= 2)
            {
                ret = ((byte)message[position]) << 8;
                ret |= ((byte)message[position + 1]) << 0;
                position += 2;
            }

            return (ushort)ret; //TODO: review
        }

        protected int readInt24(byte[] message)
        {
            int ret = 0;
            if (message.Length >= 3)
            {
                ret = ((byte)message[position]) << 16;
                ret |= ((byte)message[position + 1]) << 8;
                ret |= ((byte)message[position + 2]) << 0;
                position += 3;
            }
            return ret;
        }

        protected string getNodeTag(byte token)
        {
            string ret = "";
            if ((token > 0) && (token < 0xf5))
            {
                ret = getToken(token);
            }
            return ret;
            
        }

        protected string readString(byte[] message, byte token)
        {
            string ret = "";
            int size = -1;
            if ((token > 0) && (token < 0xf5))
            {
                ret = getToken(token);
            }
            else if (token == 0xfc)
            {
                size = readInt8(message);
                ret =  Encoding.UTF8.GetString(getByteArray(message, (int)size));
            }
            else if (token == 0xfd)
            {
                size = readInt24(message);
                ret = Encoding.UTF8.GetString(getByteArray(message, size));
            }
            else if (token == 0xfe)
            {
                token = readInt8(message);
                ret = getToken((byte)(token + 0xf5)); //TODO> review
            }
            else if (token == 0xfa)
            {
                string user = readString(message, readInt8(message));
                string server = readString(message, readInt8(message));
                if ((user.Length > 0) && (server.Length > 0))
                {
                    ret = user + "@" + server;
                }
                else if (server.Length > 0)
                {
                    ret = server;
                }
            }
            return ret;
        }

        protected Dictionary<string, string> readAttributes(byte[]message, int size)
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            int attribCount = (size - 2 + size % 2) / 2;
            for (int i = 0; i < attribCount; i++)
            {
                string key = readString(message, readInt8(message));
                string value = readString(message, readInt8(message));
                attributes[key] = value;
            }
            return attributes;
        }

        protected byte[] getByteArray(byte[] message, int len)
        {
            byte[] ret = new byte[len];
            if (message.Length >= len)
            {
                Array.Copy(message, position, ret, 0, len);
                position += len;
            }
            return ret;
        }

        protected string getToken(byte token)
        {
            return ProtocolTable.Instance.Decode(token);
        }
    }
}
