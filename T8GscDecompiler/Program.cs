using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace T8GscDecompiler
{
    class Program
    {
        public static Dictionary<long, string> HashTable = new Dictionary<long, string>();
        public static Dictionary<uint, string> _32BitHashTable = new Dictionary<uint, string>();
        static void Main(string[] args)
        {
            LoadHashTable();
            Load32BitHashTable();
            if (args.Length < 1)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("");
                Console.WriteLine("no file given ");
            }
            string[] files = { };
            if (args[0] == "--f")
            {
                if (args[1].Length > 0)
                {
                    Console.WriteLine("POUET POUET");
                    string folder = args[2];
                   
                        files = Directory.GetFiles(@"C:\\Users\\engle\\Desktop\\Electron Fx Exporter\\Electron Fx Exporter\\Electron Fx Exporter\\bin\\Debug\\exported_files\\zm_escape\\", "*.gscc");
                       /* foreach(string f in files )
                        {
                            Console.WriteLine(f);
                        }*/
                    
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("");
                    Console.WriteLine("no Valid path given  ");
                }
            }else
            {
                files = args;
            }

                for (int i = 0; i < files.Length; i++)
                {
                Console.WriteLine(files[i]);
                    if (File.Exists(files[i]))
                    {
                        ///  LoadScriptEncryption(args[i]);
                        ///  
                        Decompile decompiler = new Decompile();
                        Decompile.DebugGsc.Clear();
                        Decompile.GscCode.Clear();
                        Decompile.StringList.Clear();
                        Decompile.ImportList.Clear();
                        Decompile.ExportList.Clear();
                        Decompile.ExportList.Clear();
                        Decompile.HashWriter.GetStringBuilder().Clear();
                        Opcodes.OPlist.Clear();
                        ResolveFunction.Writer.GetStringBuilder().Clear();
                        Console.WriteLine(ResolveFunction.Writer.ToString());
                        Console.WriteLine("Decompiling File : " + Path.GetFileName(files[i]));
                        decompiler.DecompileFile(files[i]);
                        Console.WriteLine("File Decompiled");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("");
                        Console.WriteLine("File doesn't exist");
                    }
                }
            
            Console.ReadLine();
        }
        public static void LoadScriptEncryption(string filename)
        {
            string filen = Path.GetFileNameWithoutExtension(filename);
            string fastfile;
            ScriptOrigin.ScriptLocation.TryGetValue(filen, out fastfile);
            Console.WriteLine(fastfile);
        }
        public static void LoadHashTable()
        {
            string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HashTable.csv");
            Console.WriteLine(file);
            string[] strings = File.ReadAllLines(file);
            foreach (string str in strings)
            {
                string[] Index = Regex.Split(str, ",");
                string key = Index[0];

                long hash = Int64.Parse(key);
                if (!HashTable.ContainsKey(hash))
                {
                    HashTable.Add(hash, Index[1]);
                }
             //  Console.WriteLine("{0},{1}", hash.ToString("x"), value);
            }

        }

        public static void Load32BitHashTable()
        {
            string _file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_32bitHashTable.csv");
            Console.WriteLine(_file);
            string[] _strings = File.ReadAllLines(_file);
            foreach (string str in _strings)
            {
                string[] Index = Regex.Split(str, ",");
                string key = Index[0];
                //   Console.WriteLine(key);
                uint hash = UInt32.Parse(key);
                if (!_32BitHashTable.ContainsKey(hash))
                {
                    _32BitHashTable.Add(hash, Index[1]);
                }
            }
        }
    }
}
