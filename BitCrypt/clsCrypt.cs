using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace BitCrypt
{
    public static class Crypt
    {
		///<summary>
		///Changing this value will render the software incompatible with existing encrypted files!
		///If you make this dependant on a ysystme value you have an encryption, that is bound to the system.
		///</summary>
        private static readonly byte[] SALT = new byte[] { 0x00, 0x7a, 0xee, 0xaf, 0x4d, 0x08, 0x22, 0x3c, 0xc5, 0x26, 0xdc, 0xff, 0xad, 0xed, 0xfe, 0x07 };
        private static Random r;

        private static byte[] getSizedByte(string source, int destinationLength)
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(source, SALT);
            return pdb.GetBytes(destinationLength);
        }

        ///<summary>
        /// Steve Lydford - 12/05/2008.
        /// Encrypts a file using Rijndael algorithm.
        ///</summary>
        ///<param name="inputFile">input File name</param>
        ///<param name="outputFile">output File name</param>
        ///<param name="password">Obviously, this is the password</param>
        public static bool EncryptFile(string inputFile, string outputFile, string password)
        {
            byte[] buffer = new byte[1024*1024];
            FileStream fsCrypt = File.Create(outputFile);
            RijndaelManaged RMCrypto = new RijndaelManaged();
            CryptoStream cs = new CryptoStream(fsCrypt, RMCrypto.CreateEncryptor(getSizedByte(password, 32), getSizedByte(password, 16)), CryptoStreamMode.Write);
            FileStream fsIn = File.OpenRead(inputFile);

            int data;
            while ((data = fsIn.Read(buffer,0,buffer.Length))>0)
            {
                cs.Write(buffer,0,data);
            }

            fsIn.Close();
            cs.Close();
            fsCrypt.Close();
            return true;
        }

        ///<summary>
        /// Steve Lydford - 12/05/2008.
        /// Decrypts a file using Rijndael algorithm.
        ///</summary>
        ///<param name="inputFile">input File name</param>
        ///<param name="outputFile">output File name</param>
        ///<param name="password">Obviously, this is the password</param>
        public static bool DecryptFile(string inputFile, string outputFile,string password)
        {
            byte[] buffer = new byte[1024*1024];

            FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);
            RijndaelManaged RMCrypto = new RijndaelManaged();
            CryptoStream cs = new CryptoStream(fsCrypt, RMCrypto.CreateDecryptor(getSizedByte(password, 32), getSizedByte(password, 16)), CryptoStreamMode.Read);
            FileStream fsOut = new FileStream(outputFile, FileMode.Create);

            int data;
            try
            {
                while ((data = cs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fsOut.Write(buffer, 0, data);
                }
            }
            catch
            {
                fsOut.Close();
                cs=null;
                fsCrypt.Close();
                return false;
            }
            fsOut.Close();
            cs.Close();
            fsCrypt.Close();
            return true;
        }

        /// <summary>
        /// Overwrites a File with random Data
        /// </summary>
        /// <param name="file">File Name</param>
        public static void Kill(string file)
        {
            long fSize = new FileInfo(file).Length;
            FileStream FS = File.OpenWrite(file);
            byte[] buffer = new byte[1024 * 1024];
            if (r == null)
            {
                r = new Random();
            }
            while (FS.Position < fSize)
            {
                r.NextBytes(buffer);
                if (FS.Position + buffer.Length <= fSize)
                {
                    FS.Write(buffer, 0, buffer.Length);
                }
                else
                {
                    FS.Write(buffer, 0, (int)(fSize - FS.Position));
                }
            }
            FS.Close();
        }
    }
}
