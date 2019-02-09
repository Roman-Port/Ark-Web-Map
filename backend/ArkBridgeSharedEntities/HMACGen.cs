﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ArkBridgeSharedEntities
{
    public static class HMACGen
    {
        public static string GenerateHMAC(string path, string method, byte[] salt, byte[] creds)
        {
            byte[] output;
            using (MemoryStream ms = new MemoryStream())
            {
                //Add attributes
                ms.Write(creds, 0, creds.Length);
                WriteString(path, ms);
                WriteString(method, ms);
                ms.Write(salt, 0, salt.Length);

                //Rewind
                ms.Position = 0;

                //Generate
                output = GenerateMAC(ms);
            }

            //Base64 convert
            return Convert.ToBase64String(output);
        }

        private static void WriteString(string s, Stream ms)
        {
            byte[] b = Encoding.UTF8.GetBytes(s);
            ms.Write(b, 0, b.Length);
        }

        private static byte[] GenerateMAC(byte[] input)
        {
            HMAC h = HMAC.Create();
            return h.ComputeHash(input);
        }

        private static byte[] GenerateMAC(Stream s)
        {
            HMAC h = HMAC.Create();
            return h.ComputeHash(s);
        }
    }
}