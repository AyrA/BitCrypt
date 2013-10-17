using System;
using System.IO;
using System.Diagnostics;

namespace BitCrypt
{
    class Program
    {
        static int Main(string[] args)
        {
#if DEBUG
            Directory.SetCurrentDirectory(@"C:\temp\BM");
#else
            if (!File.Exists("keys.dat") && !File.Exists(@".\crypt\keys.dat"))
            {
                Console.WriteLine("No local keys.dat found. Assuming APPDATA Folder is used...");
                if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "pyBitmessage")))
                {
                    Directory.SetCurrentDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "pyBitmessage"));
                    if (!File.Exists("keys.dat") && !File.Exists(@".\crypt\keys.dat"))
                    {
                        Console.WriteLine("keys.dat not found. Please start bitmessage at least once.");
                        Exit();
                    }
                }
                else
                {
                    Console.WriteLine("keys.dat not found. Please start bitmessage at least once.");
                    Exit();
                }
            }
#endif
            Console.WriteLine("Enter the Password to use for encryption and decryption");
            string PWD = getPWD();
            if (File.Exists("keys.dat"))
            {
                Console.WriteLine("Unencrypted keys.dat found. Enter the password again to encrypt it.");
                if (PWD == getPWD())
                {
                    doEncrypt(PWD);
                    Console.WriteLine("Please restart BitCrypt");
                }
                else
                {
                    Console.WriteLine("Passwords did not match");
                    Exit();
                }
            }
            else
            {
                if (doDecrypt(PWD))
                {
                    if (File.Exists("bitmessage.exe"))
                    {
                        Console.WriteLine("Bitmessage started. Do not close this Window");
                        Process P=Process.Start("bitmessage.exe");
                        WinAPI.Hide();
                        P.WaitForExit();
                        WinAPI.Show();
                        Console.Clear();
                        Console.WriteLine("Enter new Password to encrypt. Enter nothing to use the same password again");
                        string PWD1 = getPWD();
                        if (PWD1 != PWD && PWD1!=string.Empty)
                        {
                            Console.WriteLine("Enter the same password a second time to verify");
                            if (getPWD() == PWD1)
                            {
                                doEncrypt(PWD1); //new PWD
                            }
                            else
                            {
                                Console.WriteLine("Passwords are not identical");
                                Exit();
                            }
                        }
                        else
                        {
                            doEncrypt(PWD); //old PWD
                        }
                        
                    }
                }
                else
                {
                    foreach (string s in new string[] { "keys.dat", "messages.dat", "knownnodes.dat" })
                    {
                        if (File.Exists(s))
                        {
                            Console.Write("Deleting defective {0,15}",s);
                            defKill(s);
                            Console.WriteLine("[OK]");
                        }
                    }
                    Console.WriteLine("Decryption failed. Please try again");
                    Exit();
                }
            }
#if DEBUG
            Console.WriteLine("#DEBUG KEYPRESS");
            Console.ReadKey(true);
#endif
            return 0;
        }

        private static void Exit()
        {
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey(true);
            Environment.Exit(1);
        }

        private static bool doDecrypt(string PWD)
        {
            bool retValue = true;
            foreach (FileInfo FI in new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, ".\\crypt")).GetFiles("*.dat"))
            {
                if (File.Exists(FI.Name))
                {
                    defKill(FI.Name);
                }
                bool success = Crypt.DecryptFile(FI.FullName, FI.Name, PWD);
                retValue = retValue && success;
                Console.WriteLine("Decryption of {0,20} {1}", FI.Name, success ? "Success" : "Failed. Wrong password?");
            }
            return retValue;
        }

        private static void doEncrypt(string PWD)
        {
            if (!Directory.Exists(".\\crypt"))
            {
                Directory.CreateDirectory(".\\crypt");
            }
            foreach (FileInfo FI in new DirectoryInfo(Environment.CurrentDirectory).GetFiles("*.dat"))
            {
                Console.Write("Encrypting {0,20}", FI.Name);

                if (File.Exists(Path.Combine(".\\crypt", FI.Name)))
                {
                    File.Delete(Path.Combine(".\\crypt", FI.Name));
                }

                bool success = Crypt.EncryptFile(FI.FullName, Path.Combine(".\\crypt", FI.Name), PWD);
                Console.Write(".");

                defKill(FI.Name);

                Console.WriteLine("[{0}]", success ? "Success" : "Failed");
            }
        }

        private static void defKill(string p)
        {
            for (int i = 0; i < 10; i++)
            {
                Crypt.Kill(p);
                Console.Write(".");
            }
            File.Delete(p);
        }

        private static string getPWD()
        {
            string pass = string.Empty;
            bool cont = true;
            char blank = '░';
            char C = '*';
            int curTop = Console.CursorTop;
            Console.Write(@"
   ╔═══════[BitCrypt]═══════╗
   ║                        ║▒
   ║     Enter Password     ║▒
   ║                        ║▒
   ║  ░░░░░░░░░░░░░░░░░░░░  ║▒
   ║                        ║▒
   ╚════════════════════════╝▒
    ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒");
            Console.SetCursorPosition(6, curTop+5);
            while (cont)
            {
                ConsoleKeyInfo K = Console.ReadKey(true);
                switch (K.Key)
                {
                    case ConsoleKey.Backspace:
                        if (pass.Length > 0)
                        {
                            Console.CursorLeft--;
                            Console.Write(blank);
                            Console.CursorLeft--;
                            pass = pass.Substring(0, pass.Length - 1);
                        }
                        break;
                    case ConsoleKey.Escape:
                        return null;
                    case ConsoleKey.Delete:
                        Console.SetCursorPosition(6, curTop+5);
                        Console.Write("░░░░░░░░░░░░░░░░░░░░");
                        Console.SetCursorPosition(6, curTop+5);
                        pass = string.Empty;
                        break;
                    case ConsoleKey.Enter:
                        cont = false;
                        break;
                    default:
                        break;
                }
                if (K.KeyChar >= 32 && K.KeyChar <= 255)
                {
                    if (pass.Length < 20)
                    {
                        pass += K.KeyChar;
                        Console.Write(C);
                    }
                }
            }
            Console.Clear();
            return pass;
        }
    }
}
