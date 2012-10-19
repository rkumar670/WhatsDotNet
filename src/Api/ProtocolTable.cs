using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhatsDotNet
{
    class ProtocolTable
    {
        private static ProtocolTable instance;

        Dictionary<string, byte> tokenMap;
        Dictionary<byte, string> decodeDict;

        private ProtocolTable() 
        {
            InitTokenMap();
        }

        public static ProtocolTable Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ProtocolTable();
                }
                return instance;
            }
        }

        public string Decode(byte b)
        {
            return decodeDict[b];
        }

        public byte Encode(string s)
        {
            return tokenMap[s];
        }

        public bool CanEncode(string s)
        {
            return tokenMap.ContainsKey(s);
        }


        private void InitTokenMap()
        {
            tokenMap = new Dictionary<string, byte>();

            CreateDictionary();

            for (byte i = 0; i < decodeDict.Count; i++)
            {
                if (decodeDict[i].Length > 0)
                {
                    tokenMap[decodeDict[i]] = (byte)i;
                }
            }
        }

        private Dictionary<byte, string> CreateDictionary()
        {
            string decodeDictA = "WHATISTHIS";
            decodeDict = new Dictionary<byte, string>();
            decodeDict[0] = "";
            decodeDict[1] = "stream:stream";
            decodeDict[2] = "";
            decodeDict[3] = "";
            decodeDict[4] = "";
            decodeDict[5] = "1";
            decodeDict[6] = "1.0";
            decodeDict[7] = "ack";
            decodeDict[8] = "action";
            decodeDict[9] = "active";
            decodeDict[10] = "add";
            decodeDict[11] = "all";
            decodeDict[12] = "allow";
            decodeDict[13] = "apple";
            decodeDict[14] = "audio";
            decodeDict[15] = "auth";
            decodeDict[16] = "author";
            decodeDict[17] = "available";
            decodeDict[18] = "bad-request";
            decodeDict[19] = "basee64";
            decodeDict[20] = "Bell.caf";
            decodeDict[21] = "bind";
            decodeDict[22] = "body";
            decodeDict[23] = "Boing.caf";
            decodeDict[24] = "cancel";
            decodeDict[25] = "category";
            decodeDict[26] = "challenge";
            decodeDict[27] = "chat";
            decodeDict[28] = "clean";
            decodeDict[29] = "code";
            decodeDict[30] = "composing";
            decodeDict[31] = "config";
            decodeDict[32] = "conflict";
            decodeDict[33] = "contacts";
            decodeDict[34] = "create";
            decodeDict[35] = "creation";
            decodeDict[36] = "default";
            decodeDict[37] = "delay";
            decodeDict[38] = "delete";
            decodeDict[39] = "delivered";
            decodeDict[40] = "deny";
            decodeDict[41] = "DIGEST-MD5";
            decodeDict[42] = "DIGEST-MD5-1";
            decodeDict[43] = "dirty";
            decodeDict[44] = "en";
            decodeDict[45] = "enable";
            decodeDict[46] = "encoding";
            decodeDict[47] = "error";
            decodeDict[48] = "expiration";
            decodeDict[49] = "expired";
            decodeDict[50] = "failure";
            decodeDict[51] = "false";
            decodeDict[52] = "favorites";
            decodeDict[53] = "feature";
            decodeDict[54] = "field";
            decodeDict[55] = "free";
            decodeDict[56] = "from";
            decodeDict[57] = "g.us";
            decodeDict[58] = "get";
            decodeDict[59] = "Glas.caf";
            decodeDict[60] = "google";
            decodeDict[61] = "group";
            decodeDict[62] = "groups";
            decodeDict[63] = "g_sound";
            decodeDict[64] = "Harp.caf";
            decodeDict[65] = "http://etherx.jabber.org/streams";
            decodeDict[66] = "http://jabber.org/protocol/chatstates";
            decodeDict[67] = "id";
            decodeDict[68] = "image";
            decodeDict[69] = "img";
            decodeDict[70] = "inactive";
            decodeDict[71] = "internal-server-error";
            decodeDict[72] = "iq";
            decodeDict[73] = "item";
            decodeDict[74] = "item-not-found";
            decodeDict[75] = "jabber:client";
            decodeDict[76] = "jabber:iq:last";
            decodeDict[77] = "jabber:iq:privacy";
            decodeDict[78] = "jabber:x:delay";
            decodeDict[79] = "jabber:x:event";
            decodeDict[80] = "jid";
            decodeDict[81] = "jid-malformed";
            decodeDict[82] = "kind";
            decodeDict[83] = "leave";
            decodeDict[84] = "leave-all";
            decodeDict[85] = "list";
            decodeDict[86] = "location";
            decodeDict[87] = "max_groups";
            decodeDict[88] = "max_participants";
            decodeDict[89] = "max_subject";
            decodeDict[90] = "mechanism";
            decodeDict[91] = "mechanisms";
            decodeDict[92] = "media";
            decodeDict[93] = "message";
            decodeDict[94] = "message_acks";
            decodeDict[95] = "missing";
            decodeDict[96] = "modify";
            decodeDict[97] = "name";
            decodeDict[98] = "not-acceptable";
            decodeDict[99] = "not-allowed";
            decodeDict[100] = "not-authorized";
            decodeDict[101] = "notify";
            decodeDict[102] = "Offline Storage";
            decodeDict[103] = "order";
            decodeDict[104] = "owner";
            decodeDict[105] = "owning";
            decodeDict[106] = "paid";
            decodeDict[107] = "participant";
            decodeDict[108] = "participants";
            decodeDict[109] = "participating";
            decodeDict[110] = "fail";
            decodeDict[111] = "paused";
            decodeDict[112] = "picture";
            decodeDict[113] = "ping";
            decodeDict[114] = "PLAIN";
            decodeDict[115] = "platform";
            decodeDict[116] = "presence";
            decodeDict[117] = "preview";
            decodeDict[118] = "probe";
            decodeDict[119] = "prop";
            decodeDict[120] = "props";
            decodeDict[121] = "p_o";
            decodeDict[122] = "p_t";
            decodeDict[123] = "query";
            decodeDict[124] = "raw";
            decodeDict[125] = "receipt";
            decodeDict[126] = "receipt_acks";
            decodeDict[127] = "received";
            decodeDict[128] = "relay";
            decodeDict[129] = "remove";
            decodeDict[130] = "Replaced by new connection";
            decodeDict[131] = "request";
            decodeDict[132] = "resource";
            decodeDict[133] = "resource-constraint";
            decodeDict[134] = "response";
            decodeDict[135] = "result";
            decodeDict[136] = "retry";
            decodeDict[137] = "rim";
            decodeDict[138] = "s.whatsapp.net";
            decodeDict[139] = "seconds";
            decodeDict[140] = "server";
            decodeDict[141] = "session";
            decodeDict[142] = "set";
            decodeDict[143] = "show";
            decodeDict[144] = "sid";
            decodeDict[145] = "sound";
            decodeDict[146] = "stamp";
            decodeDict[147] = "starttls";
            decodeDict[148] = "status";
            decodeDict[149] = "stream:error";
            decodeDict[150] = "stream:features";
            decodeDict[151] = "subject";
            decodeDict[152] = "subscribe";
            decodeDict[153] = "success";
            decodeDict[154] = "system-shutdown";
            decodeDict[155] = "s_o";
            decodeDict[156] = "s_t";
            decodeDict[157] = "t";
            decodeDict[158] = "TimePassing.caf";
            decodeDict[159] = "timestamp";
            decodeDict[160] = "to";
            decodeDict[161] = "Tri-tone.caf";
            decodeDict[162] = "type";
            decodeDict[163] = "unavailable";
            decodeDict[164] = "uri";
            decodeDict[165] = "url";
            decodeDict[166] = "urn:ietf:params:xml:ns:xmpp-bind";
            decodeDict[167] = "urn:ietf:params:xml:ns:xmpp-sasl";
            decodeDict[168] = "urn:ietf:params:xml:ns:xmpp-session";
            decodeDict[169] = "urn:ietf:params:xml:ns:xmpp-stanzas";
            decodeDict[170] = "urn:ietf:params:xml:ns:xmpp-streams";
            decodeDict[171] = "urn:xmpp:delay";
            decodeDict[172] = "urn:xmpp:ping";
            decodeDict[173] = "urn:xmpp:receipts";
            decodeDict[174] = "urn:xmpp:whatsapp";
            decodeDict[175] = "urn:xmpp:whatsapp:dirty";
            decodeDict[176] = "urn:xmpp:whatsapp:mms";
            decodeDict[177] = "urn:xmpp:whatsapp:push";
            decodeDict[178] = "value";
            decodeDict[179] = "vcard";
            decodeDict[180] = "version";
            decodeDict[181] = "video";
            decodeDict[182] = "w";
            decodeDict[183] = "w:g";
            decodeDict[184] = "w:p:r";
            decodeDict[185] = "wait";
            decodeDict[186] = "x";
            decodeDict[187] = "xml-not-well-formed";
            decodeDict[188] = "xml:lang";
            decodeDict[189] = "xmlns";
            decodeDict[190] = "xmlns:stream";
            decodeDict[191] = "Xylophone.caf";
            decodeDict[192] = "account";
            decodeDict[193] = "digest";
            decodeDict[194] = "g_notify";
            decodeDict[195] = "method";
            decodeDict[196] = "password";
            decodeDict[197] = "registration";
            decodeDict[198] = "stat";
            decodeDict[199] = "text";
            decodeDict[200] = "user";
            decodeDict[201] = "username";
            decodeDict[202] = "event";
            decodeDict[203] = "latitude";
            decodeDict[204] = "longitude";
            decodeDict[205] = "true";
            decodeDict[206] = "after";
            decodeDict[207] = "before";
            decodeDict[208] = "broadcast";
            decodeDict[209] = "count";
            decodeDict[210] = "features";
            decodeDict[211] = "first";
            decodeDict[212] = "index";
            decodeDict[213] = "invalid-mechanism";
            decodeDict[214] = "l" + decodeDictA;
            decodeDict[215] = "max";
            decodeDict[216] = "offline";
            decodeDict[217] = "proceed";
            decodeDict[218] = "required";
            decodeDict[219] = "sync";
            decodeDict[220] = "elapsed";
            decodeDict[221] = "ip";
            decodeDict[222] = "microsoft";
            decodeDict[223] = "mute";
            decodeDict[224] = "nokia";
            decodeDict[225] = "off";
            decodeDict[226] = "pin";
            decodeDict[227] = "pop_mean_time";
            decodeDict[228] = "pop_plus_minus";
            decodeDict[229] = "port";
            decodeDict[230] = "reason";
            decodeDict[231] = "server-error";
            decodeDict[232] = "silent";
            decodeDict[233] = "timeout";
            decodeDict[234] = "lc";
            decodeDict[235] = "lg";
            decodeDict[236] = "bad-protocol";
            decodeDict[237] = "none";
            decodeDict[238] = "remote-server-timeout";
            decodeDict[239] = "service-unavailable";
            decodeDict[240] = "w:p";
            decodeDict[241] = "w:profile:picture";
            decodeDict[242] = "notification";
            decodeDict[243] = "";
            decodeDict[244] = "";
            decodeDict[245] = "";
            decodeDict[246] = "";
            decodeDict[247] = "";
            decodeDict[248] = "XXX";
            return decodeDict;
        }
    }
}
