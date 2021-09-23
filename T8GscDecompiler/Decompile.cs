using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using T8GscDecompiler;
namespace T8GscDecompiler
{
    class Decompile
    {
        public static List<string> DebugGsc = new List<string>();
        public static List<string> GscCode = new List<string>();
        public static StringWriter HashWriter = new StringWriter();

        public struct GscHeader
        {
            public Int32 FileMagic { get; set; }  // 4
            public Int32 FileVersion { get; set; } //4
            public Int64 CheckSum { get; set; }  // 8
            public Int64 FileName { get; set; }
            public Int32 IncludePtr { get; set; }
            public Int16 NumOfString { get; set; }
            public Int16 NumOfExport { get; set; }
            public Int32 ByteCodeOffset { get; set; }
            public Int32 StringTableOffset { get; set; }
            public Int16 NumOfImport { get; set; }
            public Int16 unknowThings { get; set; } //Fixupcount        
            public Int32 DebugStringTableOffset { get; set; }
            public Int32 ExportOffset { get; set; }
            public Int32 UnknowThings2 { get; set; }
            public Int32 ImportOffset { get; set; }
            public Int16 UnknowThings3 { get; set; } //num of animtree
            public Int16 UnknowThings3bis { get; set; }
            public Int32 AnimtreeOffset { get; set; }
            public Int32 AnimtreeOffset2 { get; set; }
            public Int32 FileSize { get; set; }
            public Int32 UnknowThings4 { get; set; }
            public Int16 UnknowThings5 { get; set; }
            public Int16 NumOfDebugString { get; set; }
            public Int16 NumOfIncludes { get; set; } // num of includes à 100%%%%%%%%%
            public Int16 UnknowThings6 { get; set; }
            public Int16 UnknowThings7 { get; set; }

            //TODO REST OF THE HEADER
        }
        

        public struct ExportStruct
        {
            public UInt32 CRC32 { get; set; }
            public UInt32 ExecCodeOffset { get; set; }
            public string FunctionName { get; set; }  // &= 0x7fffffffffffffff;
            public string FunctionNamespace { get; set; }
            public UInt32 FunctionNamespace2 { get; set; }
            public sbyte FunctionParam { get; set; }
            public sbyte FunctionFlag { get; set; }
            public int ExecCodeSize { get; set; }
            public List<ScriptOpStack> FunctionOperands { get; set; } //chaque function à son lot d'opCodes
        }

        public struct ImportStruct
        {
            // 10 bytes padding
            public uint ImportName { get; set; }
            public uint ImportNamespace { get; set; }
            public Int16 ReferenceCount { get; set; }
            public sbyte ImportParameters { get; set; }
            public sbyte Flag { get; set; }
            public List<UInt32> Reference { get; set; }
        }

        public struct StringStruct
        {
            public UInt32 StrOffset { get; set; }
            public UInt16 StrCount { get; set; }
            public List<UInt32> StrReferences { get; set; }
            public string DecryptedString { get; set; }

        }

        public struct IncludeStruct
        {

        }

        public struct AnimTreeStruct
        {
            public Int32 Name { get; set; }
            public UInt32 Adress { get; set; }
        }

        public static List<StringStruct> StringList = new List<StringStruct>();
        public static List<ImportStruct> ImportList = new List<ImportStruct>();
        public static List<ExportStruct> ExportList = new List<ExportStruct>();

        public void DecompileFile(string file)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(file, FileMode.Open)))
            {
                LoadStuff(reader);
                LoadOpCode(reader);
                WriteDebug(file);
             //   Resolve(file);
                hahsfile();
            }

        }

        private void hahsfile()
        {
            if (File.Exists("Str.txt"))
            { 

            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName("string\\Str.txt"));
            }

            File.AppendAllText("string\\Str.txt", HashWriter.ToString());
                  }

        public void Resolve(string file)
        {
            for (int i = 0; i < ExportList.Count; i++)
            {
                ResolveFunction resolver = new ResolveFunction();

                resolver.Resolve(ExportList[i]);
            }

            string filen = file.Replace("gscc", "gsc").Replace("cscc", "csc");

            Directory.CreateDirectory(Path.GetDirectoryName("Decompiled_Files\\" + Path.GetFileName(filen)));
            File.WriteAllText("Decompiled_Files\\" + Path.GetFileName(filen), ResolveFunction.Writer.ToString());

        }
        public void LoadOpCode(BinaryReader reader)
        {
            for (int i = 0; i < ExportList.Count; i++)
            {
                ExportStruct Export = ExportList[i];

                Opcodes.OPlist.Clear();

                DebugGsc.Add("/*");
                DebugGsc.Add("Function Name:" + ExportList[i].FunctionName);
                DebugGsc.Add("Function Namespace:" + ExportList[i].FunctionNamespace);
                DebugGsc.Add("Function Namespace2:" + ExportList[i].FunctionNamespace2.ToString("X"));
                DebugGsc.Add("Function Param:" + ExportList[i].FunctionParam);
                DebugGsc.Add("Function Flag:" + ExportList[i].FunctionFlag);
                DebugGsc.Add("Function ByteCode:" + ExportList[i].ExecCodeOffset.ToString("X"));
                DebugGsc.Add("Function ByteCodeSize:" + ExportList[i].ExecCodeSize.ToString("X"));
                DebugGsc.Add("*/");
                string FunctionData = "";

                FunctionData += "function " + ExportList[i].FunctionName + "(";
                for (int k = 0; k < ExportList[i].FunctionParam; k++) //Les paramètres sont les safe create local variable 
                {
                    if (k == ExportList[i].FunctionParam)
                    {
                        FunctionData += "PARAM" + k ;
                    }
                    else
                    {
                        FunctionData += "PARAM" + k + ", ";

                    }
                }
                FunctionData += ")";
                DebugGsc.Add(FunctionData);
                DebugGsc.Add("{");
                DebugGsc.Add("/*");


                
                reader.BaseStream.Position = ExportList[i].ExecCodeOffset;
                long EndStream = ExportList[i].ExecCodeOffset + ExportList[i].ExecCodeSize;
                DebugGsc.Add("START : " + reader.BaseStream.Position.ToString("X"));

                while (reader.BaseStream.Position < EndStream)
                {
                    
                    UInt16 OpCode_index = reader.ReadUInt16();
                    if (OpCode_index <= 0x4000)
                    {
                        var opcode = Opcodes.opCodeTable[OpCode_index];
                        Opcodes.ProcessOpcodes(opcode, reader,(UInt16)OpCode_index,Export);
                        continue;
                    }else
                    {
                        DebugGsc.Add("Invalid : " + OpCode_index.ToString("X") + "  Pos : "+ (reader.BaseStream.Position - 2 ).ToString("X"));
                    }
                    

                }
                Export.FunctionOperands = new List<ScriptOpStack>();
                Export.FunctionOperands.AddRange(Opcodes.OPlist.ToArray());

                DebugGsc.Add("*/");
                DebugGsc.Add("}");
                ExportList[i] = Export;
               // Export.FunctionOP
            }
        }
        public void LoadStuff(BinaryReader reader)
        {
            GscHeader header = new GscHeader();

            header.FileMagic = reader.ReadInt32();
            if (header.FileMagic != 0x43534780)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("");
                Console.WriteLine("File isn't a GSC/CSC");
                
            }
            header.FileVersion = reader.ReadInt32(); //4
            header.CheckSum = reader.ReadInt64(); //8
            header.FileName = reader.ReadInt64(); //8
            header.IncludePtr = reader.ReadInt32(); //4
            header.NumOfString = reader.ReadInt16(); //2
            header.NumOfExport = reader.ReadInt16(); //2
            header.ByteCodeOffset = reader.ReadInt32(); //4
            header.StringTableOffset = reader.ReadInt32(); //4
            header.NumOfImport = reader.ReadInt16(); //2
            header.unknowThings = reader.ReadInt16(); //2 num of profile
            header.DebugStringTableOffset = reader.ReadInt32(); //4
            header.ExportOffset = reader.ReadInt32(); //4
            header.UnknowThings2 = reader.ReadInt32(); //4
            header.ImportOffset = reader.ReadInt32(); //4
            header.UnknowThings3 = reader.ReadInt16(); //2 UINT16 NUM OF SOMETHING SHOULD BE NUM OF ANIMTREE
            header.UnknowThings3bis = reader.ReadInt16(); //2 UINT16
            header.AnimtreeOffset = reader.ReadInt32(); //32 + 32 not the key MAYBE ANIMTREE OFFSET
            header.AnimtreeOffset2 = reader.ReadInt32(); //32 + 32 not the key MAYBE ANIMTREE OFFSET
            header.FileSize = reader.ReadInt32(); //4
            header.UnknowThings4 = reader.ReadInt32(); //4
            header.UnknowThings5 = reader.ReadInt16(); //4
            header.NumOfDebugString = reader.ReadInt16(); //4
            header.UnknowThings7 = reader.ReadInt16(); //2
            header.UnknowThings6 = reader.ReadInt16(); //4
            header.NumOfIncludes = reader.ReadInt16();
            DebugGsc.Add("/*");
            DebugGsc.Add("File Magic : " + header.FileMagic.ToString("x"));
            DebugGsc.Add("File Version : " + header.FileVersion.ToString("x"));
            DebugGsc.Add("File CheckSum : " + header.CheckSum.ToString("x"));
            DebugGsc.Add("File name : " + header.FileName.ToString("x"));
            DebugGsc.Add("IncludePtr : " + header.IncludePtr.ToString("x"));
            DebugGsc.Add("NumOfString : " + header.NumOfString);
            DebugGsc.Add("NumOfExport : " + header.NumOfExport);
            DebugGsc.Add("ByteCodeOffset : " + header.ByteCodeOffset.ToString("x"));
            DebugGsc.Add("StringTableOffset : " + header.StringTableOffset.ToString("x"));
            DebugGsc.Add("NumOfImport : " + header.NumOfImport);
            DebugGsc.Add("unknowThings : " + header.unknowThings);
            DebugGsc.Add("DebugStringTableOffset : " + header.DebugStringTableOffset.ToString("x"));
            DebugGsc.Add("ExportOffset : " + header.ExportOffset.ToString("x"));
            DebugGsc.Add("UnknowThings2 : " + header.UnknowThings2.ToString("x"));
            DebugGsc.Add("ImportOffset : " + header.ImportOffset.ToString("x"));
            DebugGsc.Add("UnknowThings3 : " + header.UnknowThings3.ToString("x"));
            DebugGsc.Add("AnimtreeOffset : " + header.AnimtreeOffset.ToString("x"));
            DebugGsc.Add("UnknowThings4 : " + header.UnknowThings4.ToString("x"));
            DebugGsc.Add("UnknowThings5 : " + header.UnknowThings5.ToString("x"));
            DebugGsc.Add("NumOfIncludes : " + header.NumOfIncludes);

            DebugGsc.Add("UnknowThings6 : " + header.UnknowThings6.ToString("x"));
            DebugGsc.Add("*/");

            load_includes(header, reader);
            load_String(header, reader);
            DecryptString(reader);
            load_debugString(header, reader); // on load après pour éviter de décrypter des strings qui n'existent pas 
            Load_Functions(header, reader);
            ResolveEnd(header, reader);
            LoadImport(header, reader);
            DebugLoadParameter(reader);

        }
        private void ResolveEnd(GscHeader header, BinaryReader reader)
        {
            for(int i = 0;  i < ExportList.Count; i++)
            {
                ExportStruct export = ExportList[i];

                int CodeSize = 0;

                if (i + 1 < ExportList.Count)
                {
                  CodeSize = (int)(((ExportList[i + 1].ExecCodeOffset - export.ExecCodeOffset) - 8)); //On retire les 8 bytes null
                  reader.BaseStream.Position = (export.ExecCodeOffset + CodeSize);
                  reader.BaseStream.Position -= 2; // Get le dernier Opcode
                    while (true)
                    {
                        var OpcodeIndex = reader.ReadInt16();
                        if (OpcodeIndex < 0x4000)
                        {
                            var opcode = Opcodes.opCodeTable[OpcodeIndex];
                            if (opcode == Opcodes.OPCodes.OP_End || opcode == Opcodes.OPCodes.OP_Return)
                            {
                                break;
                            }
                        }
                        reader.BaseStream.Position -= 4; // Get le dernier Opcode
                    }

                    CodeSize = (int)(reader.BaseStream.Position - export.ExecCodeOffset);

                }
                else // c'est la dernière fonction
                {
                    CodeSize = (int)(header.ExportOffset - export.ExecCodeOffset);
                }
                export.ExecCodeSize = CodeSize;
                ExportList[i] = export;
            }
        }
        private void load_debugString(GscHeader header, BinaryReader reader)
        {
            reader.BaseStream.Position = header.DebugStringTableOffset;
            for(int i = 0; i < header.NumOfDebugString; i++) //TODO Num of DEBUG STRING
            {
                StringStruct strStruct = new StringStruct();
                strStruct.StrOffset = reader.ReadUInt32();
                strStruct.StrCount = reader.ReadByte();
                strStruct.DecryptedString = "Dev String aren't supported";
                strStruct.StrReferences = new List<uint>();
                reader.BaseStream.Position += 3;
                for (int a = 0; a < strStruct.StrCount; a++)
                {
                    UInt32 reference = reader.ReadUInt32();
                    strStruct.StrReferences.Add(reference); //ptr where string is used
                }

                StringList.Add(strStruct);

                DebugGsc.Add("/*");
                DebugGsc.Add("StrOffset : " + strStruct.StrOffset.ToString("x"));
                DebugGsc.Add("StrCount : " + strStruct.StrCount);
                for (int j = 0; j < strStruct.StrReferences.Count; j++)
                {
                    DebugGsc.Add("Ref :" + strStruct.StrReferences[j].ToString("X"));

                }
                DebugGsc.Add("*/");
                DebugGsc.Add(" ");

            }
        }
        public void DebugLoadParameter(BinaryReader reader)
        {
        
            for(int i = 0; i < ExportList.Count;i++)
            {
                DebugGsc.Add("/*");
                DebugGsc.Add("FunctionName = " + ExportList[i].FunctionName);
                DebugGsc.Add("ByteCode offset : " + ExportList[i].ExecCodeOffset.ToString("X"));
                DebugGsc.Add("Param : " + ExportList[i].FunctionParam);
                DebugGsc.Add("Flag : " + ExportList[i].FunctionFlag);
                reader.BaseStream.Position = ExportList[i].ExecCodeOffset;
                if (ExportList[i].FunctionParam > 0)
                {
                    for (int j = 0; j < ExportList[i].FunctionParam; j++)
                    {
                        Console.WriteLine(reader.ReadUInt16().ToString("X"));
                     //   UInt32 parameter = reader.ReadUInt32(); /*reader.ReadUInt64() & 0x7FFFFFFFFFFFFFFF;*/
                       // DebugGsc.Add(parameter.ToString("X"));
                    }
                }
                
                DebugGsc.Add("*/");

            }
        }
        public void LoadImport(GscHeader header,BinaryReader reader)
        {
            reader.BaseStream.Position = header.ImportOffset;
            for(int i =  0; i <header.NumOfImport;i++)
            {
                ImportStruct import = new ImportStruct();

                import.ImportName = reader.ReadUInt32();
                import.ImportNamespace = reader.ReadUInt32();
                import.ReferenceCount = reader.ReadInt16(); // num of params
                import.ImportParameters = reader.ReadSByte();
                import.Flag = reader.ReadSByte();
                import.Reference = new List<UInt32>();

                DebugGsc.Add("/*");
                DebugGsc.Add("ImportName : "+ import.ImportName.ToString("X"));
                DebugGsc.Add("ImportNamespace : "+ import.ImportNamespace.ToString("x"));
                DebugGsc.Add("ReferenceCount : " + import.ReferenceCount);
                DebugGsc.Add("ImportParameters : "+ import.ImportParameters);
                DebugGsc.Add("Flag : "+ import.Flag);
                for(int j = 0; j < import.ReferenceCount; j++)
                {
                    UInt32 refe = reader.ReadUInt32();
                    import.Reference.Add(refe);
                    DebugGsc.Add("Reference : " + refe.ToString("X"));
                }

                ImportList.Add(import);
                DebugGsc.Add("*/");
                DebugGsc.Add("");
            }
        }
        public void WriteDebug(string filename)
        {
            string filen = filename.Replace("gscc", "gsc").Replace("cscc", "csc");

            Directory.CreateDirectory(Path.GetDirectoryName("Decompiled_Files\\debug_" + Path.GetFileName(filen)));
            File.WriteAllLines("Decompiled_Files\\debug_" + Path.GetFileName(filen), DebugGsc);
        }
        public void Load_Functions(GscHeader header,BinaryReader reader)
        {
            reader.BaseStream.Position = header.ExportOffset;
            for (int i = 0; i < header.NumOfExport; i++)
            {
                ExportStruct function = new ExportStruct();
                function.CRC32 = reader.ReadUInt32();
                function.ExecCodeOffset = reader.ReadUInt32();
                var _FunctionName = reader.ReadUInt32();
                function.FunctionName = Program._32BitHashTable.ContainsKey(_FunctionName) ? Program._32BitHashTable.GetValueOrDefault(_FunctionName) : "func_" + _FunctionName.ToString("x");

                var namseSpace = reader.ReadUInt32(); 
                function.FunctionNamespace = Program._32BitHashTable.ContainsKey(namseSpace) ? Program._32BitHashTable.GetValueOrDefault(namseSpace) : namseSpace.ToString("x");
                function.FunctionNamespace2 = reader.ReadUInt32();
                function.FunctionParam = reader.ReadSByte();
                function.FunctionFlag = reader.ReadSByte();

                var pos = reader.BaseStream.Position + 2;
                reader.BaseStream.Position = function.ExecCodeOffset;
                int CodeSize = 0;

                var crc32 = new CRC32();
                bool found = false;
                long endPos = 0;

                Console.WriteLine(function.ExecCodeOffset.ToString("X"));
                function.ExecCodeSize = CodeSize;

                /*  if (i != header.NumOfExport - 1) // le dernier block de fonction ne bénéficie pas des 8 bytes nuls
                  {
                      while (!found)
                      {
                          var byte_ = reader.ReadUInt16();
                          if (byte_ < 0x4000)
                          {
                              var opcode = Opcodes.opCodeTable[byte_];
                              switch (opcode)
                              {
                                  case Opcodes.OPCodes.OP_Invalid:
                                      {
                                          break;
                                      }

                                  case Opcodes.OPCodes.OP_End:
                                      {
                                          endPos = reader.BaseStream.Position;
                                          var Skip = reader.ReadInt32();
                                          reader.BaseStream.Position += Opcodes.ComputePadding((int)reader.BaseStream.Position, 4);

                                          var NullCheck = reader.ReadInt32();


                                          if (NullCheck == 0x0)  //Permet d'éviter les faux positifs
                                          {
                                              found = true;
                                          }
                                          Console.WriteLine("FOUND AT : " + reader.BaseStream.Position.ToString("X"));
                                          break;
                                      }
                                  case Opcodes.OPCodes.OP_Jump:
                                  case Opcodes.OPCodes.OP_JumpOnFalse:
                                  case Opcodes.OPCodes.OP_JumpOnTrue: // permet d'aller plus vitte
                                      {
                                          var position = reader.ReadInt16();
                                          if (position > 0) // on check si on jump vers l'avant et non vers l'arrière 
                                          {
                                              Console.WriteLine("JUMP TO : " + (position + reader.BaseStream.Position).ToString("X"));

                                             // reader.BaseStream.Position += position;
                                          }
                                          break;
                                      }

                                  case Opcodes.OPCodes.OP_Return: // IMPROVE THIS SHIT AVEC LES JUMPS
                                      {
                                          Console.WriteLine("FOUND AT : " + reader.BaseStream.Position.ToString("X"));
                                          endPos = reader.BaseStream.Position;
                                          var Skip = reader.ReadInt32();
                                          reader.BaseStream.Position += Opcodes.ComputePadding((int)reader.BaseStream.Position, 4);
                                          var NullCheck = reader.ReadInt32();
                                          if (NullCheck == 0x0) //Permet d'éviter les faux positifs
                                          {
                                              found = true;
                                          }
                                          break;
                                      }
                                  default:
                                      break;
                              }
                          }

                      }

                      CodeSize = ((int)endPos) - (int)function.ExecCodeOffset; 

                  }
                  else
                  {
                      CodeSize = -((int)function.ExecCodeOffset) + (int)(header.ExportOffset);
                  }
                  */


                ExportList.Add(function);
                reader.BaseStream.Position = pos;

                /*
                 FLAGS : 

                Flag : 2 => autoexec
                 
                 */
            }
        }
        public void load_String(GscHeader header, BinaryReader reader)
        {
            reader.BaseStream.Position = header.StringTableOffset;  
            for (int i = 0; i <header.NumOfString;i++)
            {
               
                StringStruct strStruct = new StringStruct();
                strStruct.StrOffset = reader.ReadUInt32();
                strStruct.StrCount = reader.ReadUInt16();
                strStruct.StrReferences = new List<uint>();
                reader.BaseStream.Position += 2;
                for (int a = 0; a < strStruct.StrCount;a ++)
                {
                   
                    UInt32 reference = reader.ReadUInt32();

                    //long save = reader.BaseStream.Position; // modified

                    strStruct.StrReferences.Add(reference); //ptr where string is used


                    // reader.BaseStream.Position = reference-2;
                 /*     UInt32 opcode = reader.ReadUInt16();
                    if (Opcodes.OpCodeTable.Length < opcode)
                    {
                        Console.WriteLine("Out of Bound opCodes" + ((int)opcode).ToString("x"));
                    }
                    else
                    {
                        if (Opcodes.OpCodeTable[opcode] != Opcodes.OPCodes.OP_GetString)
                        {
                            DebugGsc.Add("OpCode :" + ((int)opcode).ToString("x"));
                            Console.WriteLine(((int)opcode).ToString("x"));
                        }
                    }*/
                    //  reader.BaseStream.Position = save;

                }

                StringList.Add(strStruct);
          
                DebugGsc.Add("/*");
                DebugGsc.Add("StrOffset : "+ strStruct.StrOffset.ToString("x"));
                DebugGsc.Add("StrCount : "+ strStruct.StrCount);
                for(int j = 0; j < strStruct.StrReferences.Count;j++)
                {
                    DebugGsc.Add("Ref :" + strStruct.StrReferences[j].ToString("X"));

                }
                DebugGsc.Add("*/");
                DebugGsc.Add(" ");
            }
        }
        public void LoadDebugString(GscHeader header, BinaryReader reader)
        {
            reader.BaseStream.Position = header.DebugStringTableOffset;
            for (int i = 0; i < header.NumOfString; i++)
            {

                StringStruct strStruct = new StringStruct();
                strStruct.StrOffset = reader.ReadUInt32();
                strStruct.StrCount = reader.ReadUInt16();
                strStruct.StrReferences = new List<uint>();
                reader.BaseStream.Position += 2;
                for (int a = 0; a < strStruct.StrCount; a++)
                {

                    UInt32 reference = reader.ReadUInt32();

                    //   long save = reader.BaseStream.Position;

                    strStruct.StrReferences.Add(reference); //ptr where string is used


                    /*  reader.BaseStream.Position = reference;
                       UInt32 opcode = reader.ReadUInt32();
                       DebugGsc.Add("OpCode :"+((int)opcode).ToString("x"));
                       Console.WriteLine(((int)opcode).ToString("x"));   
                       reader.BaseStream.Position = save;*/

                }

                StringList.Add(strStruct);

                DebugGsc.Add("/*");
                DebugGsc.Add("StrOffset : " + strStruct.StrOffset.ToString("x"));
                DebugGsc.Add("StrCount : " + strStruct.StrCount);
                for (int j = 0; j < strStruct.StrReferences.Count; j++)
                {
                    DebugGsc.Add("Ref :" + strStruct.StrReferences[j].ToString("X"));

                }
                DebugGsc.Add("*/");
                DebugGsc.Add(" ");
            }
        }
        public void DecryptString(BinaryReader reader)
        {

            for(int j = 0; j < StringList.Count;j++)
            {
                StringStruct stringstruct = StringList[j];
                reader.BaseStream.Position = stringstruct.StrOffset;
                int EncryptionID = reader.ReadSByte() & 0xff;
                int StringSize = reader.ReadSByte() - 1;
                List<int> string_ = new List<int>();
                for (int i = 0; i < StringSize; i++)
                {
                    string_.Add(reader.ReadSByte() & 0xff);
                }
                DebugGsc.Add("/*");
                DebugGsc.Add("Decryption ID : " + EncryptionID.ToString("x"));
                string result = Decryption.DecryptString(EncryptionID, string_.Count, string_.ToArray());
                stringstruct.DecryptedString = result;
                StringList[j] = stringstruct;

                //  StringList[j].DecryptedString = result;
                DebugGsc.Add("Decrypted String: " + result);
                HashWriter.WriteLine(result);
                DebugGsc.Add("Offset :" + stringstruct.StrOffset.ToString("X"));
                DebugGsc.Add("*/");



            }



            // Console.WriteLine("StringPos" +StringPos.ToString("x"));
            //  Console.WriteLine("EncryptionID" + EncryptionID.ToString("x"));
            //  Console.WriteLine("StringSize" + StringSize.ToString("x"));

        }
        public void load_includes (GscHeader header, BinaryReader reader)
        {
 
            // pour get le nom    => uVar10 =? i
            //uVar11 = *(ulonglong *)(InlcudeOffset + uVar10 * 8) & 0x7fffffffffffffff;
            reader.BaseStream.Position = header.IncludePtr;
            for(int i = 0; i < header.NumOfIncludes;i++)
            {
                
                 /*long inc = reader.ReadInt64();
                DebugGsc.Add("#using "+(inc & 0x7fffffffffffffff).ToString("x"));*/
            }
        }

    }
}
