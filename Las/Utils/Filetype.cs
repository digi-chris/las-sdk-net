using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lucidtech.Las.Utils
{
    /// <summary>
    /// Help determine the type of a file, inspired by pythons <c>imghdr.what()</c>.
    /// </summary>
    public static class FileType
    {
        /// <summary>
        /// Tests the type of file.
        /// </summary>
        /// <param name="fileName"> The name of the file that is to be classified </param>
        /// <returns> The name of the file type that matches, or an empty string if the file does not match </returns>
        public static string WhatFile(string fileName)
        {
            var fileTestList = new List<Func<byte[],string>>()
            {
                TestPdf, 
                TestJpeg
            }; 
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

        private static bool Exists(byte[] source, byte[] pattern)
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

        private static byte[] HexStringToByteArray(string hex) 
        {
            return Enumerable.Range(0, hex.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                     .ToArray();
        }

        private static string TestPdf(byte[] fileHeader)
        {
            return Exists(fileHeader,Encoding.UTF8.GetBytes("PDF")) ? "pdf" : "";
        }

        private static string TestJpeg(byte[] fileHeader)
        {
            return fileHeader.Take(2).SequenceEqual(HexStringToByteArray("FFD8")) ? "jpeg" : "";
        }
    }
}