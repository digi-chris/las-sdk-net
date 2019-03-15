using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Lucidtech.Las.Filetype
{
    public class FileTests
    {
        //private List<Func<byte[],string>> FileTestList { get; }
        
        public static string WhatFile(string fileName)
        {
            var fileTestList = new List<Func<byte[],string>>()
            {
                TestPdf, 
                TestJpeg
            } ; 
            
            string filetype = "";
            
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    byte[] bytes = new byte[10];
                    int bytesToRead = bytes.Length;
                    int bytesRead = 0;
                    while (bytesToRead > 0)
                    {
                        // Read may return anything from 0 to numBytesToRead.
                        int n = fs.Read(bytes, bytesRead, bytesToRead);

                        // Break when the end of the file is reached.
                        if (n == 0)
                            break;

                        bytesRead += n;
                        bytesToRead -= n;
                    }

                    foreach (var func in fileTestList)
                    {
                        filetype = func(bytes);
                        if (!string.IsNullOrEmpty(filetype))
                        {
                            break;
                        }
                    }
                    
                    return filetype;
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        public static bool Exists(byte[] source, byte[] pattern)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                {
                    return true;
                }
            }

            return false;
        }
        public static byte[] HexStringToByteArray(string hex) 
        {
            return Enumerable.Range(0, hex.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                     .ToArray();
        }
        
        public static string TestPdf(byte[] fileHeader)
        {
            if(Exists(fileHeader,Encoding.UTF8.GetBytes("PDF")))
            {
                return "pdf";
            }
            return "";
        }

        public static string TestJpeg(byte[] fileHeader)
        {
            if (fileHeader.Take(2).SequenceEqual(HexStringToByteArray("FFD8")))
            {
                return "jpeg";
            }

            return "";
        }
    }
}