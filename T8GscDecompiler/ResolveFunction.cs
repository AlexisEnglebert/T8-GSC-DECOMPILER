using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace T8GscDecompiler
{
    class ResolveFunction
    {
        public static StringWriter Writer = new StringWriter();
        private List<string> LocalVar = new List<string>();
        private Stack<string> stack = new Stack<string>();
        string CurrentObject = "";
        string CurrentRefference = "";
        bool Devblock = false;
        bool ifElse = false;
        int index = 0;
        int SpecialBlockSize = 0;
        static int StackCall = 0;

        public void Resolve(Decompile.ExportStruct Export)
        {
            bool ValidFunction = true;

            Writer.WriteLine("/*");
            Writer.WriteLine("Function Name:" + Export.FunctionName);
            Writer.WriteLine("Function Namespace:" + Export.FunctionNamespace);
            Writer.WriteLine("Function Namespace2:" + Export.FunctionNamespace2.ToString("X"));
            Writer.WriteLine("Function Param:" + Export.FunctionParam);
            Writer.WriteLine("Function Flag:" + Export.FunctionFlag);
            Writer.WriteLine("Function ByteCode:" + Export.ExecCodeOffset.ToString("X"));
            Writer.WriteLine("Function ByteCodeSize:" + Export.ExecCodeSize.ToString("X"));
            Writer.WriteLine("*/");


            Console.WriteLine(Export.FunctionName);

            //On écrit la début des functions
            Writer.Write("function " + Export.FunctionName + "("); //TODO LES FLAGS
            for (int i = 0; i < Export.FunctionOperands.Count; i++)
            {
                if (Export.FunctionOperands[i].Opcode == Opcodes.OPCodes.OP_Invalid)
                {
           
                }

                if (Export.FunctionOperands[i].Opcode == Opcodes.OPCodes.OP_SafeCreateLocalVariables)
                {
                    LocalVar.Add((string)Export.FunctionOperands[i].Val);
                    CurrentObject = (string)Export.FunctionOperands[i].Val; // pas sur mais bon faut bien test
                    Export.FunctionOperands[i].visited = true;

                }
                if(Export.FunctionOperands[Export.FunctionOperands.Count - 1].Opcode == Opcodes.OPCodes.OP_End &&
                   !Export.FunctionOperands[Export.FunctionOperands.Count - 1].visited) // pour être sur que ça soit le dernier
                {
                    Export.FunctionOperands[Export.FunctionOperands.Count - 1].visited = true;
                }
            }
           
                if (ValidFunction)
                {
                    ResolveFunctionParams(Export);
                    Writer.Write(")\n");
                    Writer.WriteLine("{");
                DetermineIfWhileLoop(Export);
                DetermineForEachLoop(Export);
                for (index = 0; index < Export.FunctionOperands.Count;index++)
                {
                    if (Devblock)
                    {
                        for (int i = 0; i < SpecialBlockSize; i++)
                        {
                            ResolveOpcode(Export.FunctionOperands[index + i], Export);
                        }
                        Devblock = false;
                        Writer.WriteLine("#/\n");
                    }
                    else
                    {
                    //    Writer.WriteLine(Export.FunctionOperands[index].Opcode + "" + Export.FunctionOperands[index].Val);
                     //   Console.WriteLine(Export.FunctionName + " StackCall :  " + StackCall);

                        ResolveOpcode(Export.FunctionOperands[index], Export);
                    }
                }
                    Writer.WriteLine("}");


                }
            
            


        }
        //On aurait pu faire comme scoba mais flemme
        public void ResolveOpcode(ScriptOpStack opstack, Decompile.ExportStruct Export)
        {
            StackCall++; 
            try
            {
                if (!opstack.visited) // Permet d'éviter de lire 2X le même opcode
                {
                    //Writer.WriteLine(opstack.Opcode);
                    switch (opstack.Opcode)
                    {
                        case Opcodes.OPCodes.OP_DecTop:
                            
                            Writer.WriteLine(stack.Pop());
                            break;
                        #region push Opperator
                        case Opcodes.OPCodes.OP_GetUndefined:
                        case Opcodes.OPCodes.OP_GetZero:
                        case Opcodes.OPCodes.OP_GetByte:
                        case Opcodes.OPCodes.OP_GetNegByte:
                        case Opcodes.OPCodes.OP_GetUnsignedShort:
                        case Opcodes.OPCodes.OP_GetNegUnsignedShort:
                        case Opcodes.OPCodes.OP_GetInteger:
                        case Opcodes.OPCodes.OP_GetFloat:
                        case Opcodes.OPCodes.OP_GetString:
                        case Opcodes.OPCodes.OP_GetIString:
                        case Opcodes.OPCodes.OP_GetVector:
                        case Opcodes.OPCodes.OP_GetSelf:
                        case Opcodes.OPCodes.OP_GetLevel:
                        case Opcodes.OPCodes.OP_GetGame:
                        case Opcodes.OPCodes.OP_GetAnim:
                        case Opcodes.OPCodes.OP_GetAnimation:
                        case Opcodes.OPCodes.OP_GetFunction:
                        case Opcodes.OPCodes.OP_GetEmptyArray:
                        case Opcodes.OPCodes.OP_Vector:
                        case Opcodes.OPCodes.OP_GetHash:
                        case Opcodes.OPCodes.OP_GetObjectType:
                        case Opcodes.OPCodes.OP_VectorConstant:
                        case Opcodes.OPCodes.OP_GetWorld: // STACK PUSH
                            {
                                //   opstack.visited = true;
                               
                                if (opstack.Opcode == Opcodes.OPCodes.OP_VectorConstant)
                                {
                                    stack.Push("(" + ((Vector3)opstack.Val).X + "," + ((Vector3)opstack.Val).Y + "," + ((Vector3)opstack.Val).Z + ")");
                                }
                                else if (opstack.Opcode == Opcodes.OPCodes.OP_Vector)
                                {
                                    stack.Push("(" + stack.Pop() + "," + stack.Pop() + "," + stack.Pop() + ")");
                                }
                                else
                                {
                                    stack.Push(opstack.Val.ToString());
                                }
                                break;
                            }
                        #endregion
                        #region Call Oppertator
                        case Opcodes.OPCodes.OP_CallBuiltin:
                        case Opcodes.OPCodes.OP_ScriptFunctionCall:
                        case Opcodes.OPCodes.OP_ScriptFunctionCallPointer:
                        case Opcodes.OPCodes.OP_ScriptMethodCall:
                        case Opcodes.OPCodes.OP_ScriptMethodCallPointer:
                        case Opcodes.OPCodes.OP_ScriptThreadCall:
                        case Opcodes.OPCodes.OP_ScriptThreadCallPointer:
                        case Opcodes.OPCodes.OP_ScriptMethodThreadCall:
                        case Opcodes.OPCodes.OP_RealWait:
                        case Opcodes.OPCodes.OP_IsDefined:
                        case Opcodes.OPCodes.OP_VectorScale:
                        case Opcodes.OPCodes.OP_WaitRealTime:
                        case Opcodes.OPCodes.OP_ClassFunctionCall:
                        case Opcodes.OPCodes.OP_Wait:
                        case Opcodes.OPCodes.OP_CallBuiltinMethod:
                        case Opcodes.OPCodes.OP_WaitFrame:

                        {
                                string result = "";
                                opstack.visited = true;
                                bool nopush = false;
                                switch (opstack.Opcode) // On refait le tris dans tout ça : 
                                {

                                    case Opcodes.OPCodes.OP_ScriptFunctionCall:
                                    case Opcodes.OPCodes.OP_ScriptThreadCall:
                                        {

                                            int Index = (int)opstack.Val;
                                            string FunctionName = Program._32BitHashTable.ContainsKey(Decompile.ImportList[Index].ImportName) ? Program._32BitHashTable.GetValueOrDefault(Decompile.ImportList[Index].ImportName) : Decompile.ImportList[Index].ImportName.ToString("x");
                                            string FunctionNameSpace = Program._32BitHashTable.ContainsKey(Decompile.ImportList[Index].ImportNamespace) ? Program._32BitHashTable.GetValueOrDefault(Decompile.ImportList[Index].ImportNamespace) : Decompile.ImportList[Index].ImportNamespace.ToString("x");
                                            int paramCount = Decompile.ImportList[Index].ImportParameters;
                                            Console.WriteLine((int)paramCount);

                                            if (Export.FunctionNamespace != FunctionNameSpace)
                                            {
                                                FunctionName = FunctionNameSpace + "::" + FunctionName;
                                            }
                                            if (opstack.Opcode == Opcodes.OPCodes.OP_ScriptThreadCall)
                                            {
                                                result = ResolveCalls(FunctionName, paramCount, true, false);
                                            }
                                            else
                                            {
                                                result = ResolveCalls(FunctionName, paramCount, false, false);

                                            }
                                            break;
                                        }
                                    case Opcodes.OPCodes.OP_ScriptMethodThreadCall:
                                    case Opcodes.OPCodes.OP_ScriptMethodCall:
                                        {
                                            int Index = (int)opstack.Val;
                                            string FunctionName = Program._32BitHashTable.ContainsKey(Decompile.ImportList[Index].ImportName) ? Program._32BitHashTable.GetValueOrDefault(Decompile.ImportList[Index].ImportName) : Decompile.ImportList[Index].ImportName.ToString("x");
                                            string FunctionNameSpace = Program._32BitHashTable.ContainsKey(Decompile.ImportList[Index].ImportNamespace) ? Program._32BitHashTable.GetValueOrDefault(Decompile.ImportList[Index].ImportNamespace) : Decompile.ImportList[Index].ImportNamespace.ToString("x");
                                            int paramCount = Decompile.ImportList[Index].ImportParameters;

                                            if (Export.FunctionNamespace != FunctionNameSpace)
                                            {
                                                FunctionName = FunctionNameSpace + "::" + FunctionName;
                                            }
                                            if (opstack.Opcode == Opcodes.OPCodes.OP_ScriptMethodThreadCall)
                                            {
                                                result = ResolveCalls(FunctionName, paramCount, true, true);
                                            }
                                            else
                                            {
                                                result = ResolveCalls(FunctionName, paramCount, false, true);

                                            }

                                            break;
                                        }
                                    case Opcodes.OPCodes.OP_CallBuiltin:
                                    case Opcodes.OPCodes.OP_CallBuiltinMethod:
                                        {
                                            int Index = (int)opstack.Val;
                                            string FunctionName = Program._32BitHashTable.ContainsKey(Decompile.ImportList[Index].ImportName) ? Program._32BitHashTable.GetValueOrDefault(Decompile.ImportList[Index].ImportName) : Decompile.ImportList[Index].ImportName.ToString("x");
                                            string FunctionNameSpace = Program._32BitHashTable.ContainsKey(Decompile.ImportList[Index].ImportNamespace) ? Program._32BitHashTable.GetValueOrDefault(Decompile.ImportList[Index].ImportNamespace) : Decompile.ImportList[Index].ImportNamespace.ToString("x");
                                            int paramCount = Decompile.ImportList[Index].ImportParameters;
                                            if (Export.FunctionNamespace != FunctionNameSpace)
                                            {
                                                FunctionName = FunctionNameSpace + "::" + FunctionName;
                                            }

                                            if (opstack.Opcode == Opcodes.OPCodes.OP_CallBuiltinMethod)
                                            {
                                                result = ResolveCalls(FunctionName, paramCount, false, true);
                                            }
                                            else
                                            {
                                                result = ResolveCalls(FunctionName, paramCount, false, false);

                                            }
                                            break;
                                        }
                                    case Opcodes.OPCodes.OP_Wait:
                                    case Opcodes.OPCodes.OP_WaitFrame:
                                        {
                                            string fName = (string)opstack.Val;
                                            result = ResolveCalls(fName, 1, false, false) + ";";
                                            nopush = true;
                                            break;
                                        }

                                    case Opcodes.OPCodes.OP_VectorScale:
                                        {
                                            string fName = (string)opstack.Val;
                                            result = ResolveCalls(fName, 2, false, false);
                                            break;
                                        }

                                    default:
                                        string functionName = (string)opstack.Val;
                                        result = ResolveCalls(functionName, 1, false, false);
                                        break;
                                }
                                if (!nopush)
                                {
                                    stack.Push(result);
                                }
                                else
                                {
                                    Writer.WriteLine(result);
                                }

                                break;
                            }
                        #endregion
                        case Opcodes.OPCodes.OP_AddToArrayField:
                            {
                                stack.Push(stack.Pop()+":"+stack.Pop());
                                break;
                            }
                        case Opcodes.OPCodes.OP_CreateArrayField:
                            {
                                break;
                            }
                        #region Eval Variable

                        /*
                         * Dans bo4 l'op pour les object ne sont plus là , ça veut dire que chaque stack est considéré comme un object
                         * théorie à test
                         * 
                         */
                        case Opcodes.OPCodes.OP_EvalLocalVariableCached:
                        case Opcodes.OPCodes.OP_EvalFieldVariable:
                        case Opcodes.OPCodes.OP_EvalSelfFieldVariable:
                        case Opcodes.OPCodes.OP_EvalLevelFieldVariable: //STACK PUSH
                        case Opcodes.OPCodes.OP_EvalFieldVariableObject:

                            {


                                switch (opstack.Opcode)
                                {
                                    case Opcodes.OPCodes.OP_EvalLocalVariableCached:
                                        {
                                            stack.Push(LocalVar[LocalVar.Count + ~(short)opstack.Val]);
                                            break;
                                        }
                                    case Opcodes.OPCodes.OP_EvalFieldVariable:
                                        {
                                            stack.Push(stack.Pop() + "." + opstack.Val);
                                           // Writer.WriteLine(stack.Count);
                                           // Writer.WriteLine("test125s");
                                            break;
                                        }
                                    case Opcodes.OPCodes.OP_EvalFieldVariableObject:
                                        {
                                            Writer.WriteLine(CurrentObject + "." + opstack.Val + " = " + stack.Pop()+";");
                                           // Writer.WriteLine(stack.Count);
                                           // Writer.WriteLine("test125s");
                                            break;
                                        }
                                    case Opcodes.OPCodes.OP_EvalSelfFieldVariable:
                                        {
                                            stack.Push("self." + opstack.Val);
                                            break;
                                        }
                                    case Opcodes.OPCodes.OP_EvalLevelFieldVariable:
                                        {
                                            stack.Push("level." + opstack.Val);

                                            break;
                                        }
                                }
                                break;
                            }
                       
                        case Opcodes.OPCodes.OP_CastFieldObject:
                            {
                                CurrentObject = stack.Pop();
                                break;
                            }
                        #endregion
                        case Opcodes.OPCodes.OP_SetVariableField:
                            {
                                string result = "";
                                result += CurrentRefference + " = " + stack.Pop() + ";";
                                Writer.WriteLine(result);
                                break;
                            }
                        /*
                         * Les refs remplacent en partie le setVariable ? théorie à test
                         * Je supose que 3arch on fait leurs systeme unpeu plus intélligmement et check si le prochain
                         * opCode est un opcode "none" alors on write dirrectment l'eval 
                         */
                        case Opcodes.OPCodes.OP_EvalFieldVariableRef:
                        case Opcodes.OPCodes.OP_EvalLocalVariableRefCached:
                        case Opcodes.OPCodes.OP_EvalSelfFieldVariableRef:
                        case Opcodes.OPCodes.OP_EvalLevelFieldVariableRef:
                            {
//                                opstack.visited = true;

                                string result = "";
                            switch (opstack.Opcode)
                            {

                                    case Opcodes.OPCodes.OP_EvalFieldVariableRef:
                                        { 


                                             CurrentRefference = CurrentObject + "." + opstack.Val;
                                             int ind = GetIndexatpos(Export, opstack.Position);
                                         
                                            //stack.Push(CurrentRefference);
                                        


                                        }
                                       // Writer.WriteLine(CurrentRefference+ " = " + stack.Pop() + ";");

                                        break;
                                    case Opcodes.OPCodes.OP_EvalLocalVariableRefCached:
                                        //CurrentRefference = LocalVar[LocalVar.Count - (int)opstack.Val - 1];
                                        //  Writer.WriteLine(LocalVar[LocalVar.Count - (short)opstack.Val - 1]);
                                        string r = LocalVar[LocalVar.Count - (short)opstack.Val - 1] + " = " + stack.Pop() + ";";
                                        result += r;
                                        CurrentRefference = r;
                                        Writer.WriteLine(result);

                                        break;
                                    case Opcodes.OPCodes.OP_EvalSelfFieldVariableRef:
                                        CurrentRefference = "self." + opstack.Val;
                                        // CECI EST UN TEST : 
                                        string getter = "";
                                        if(stack.Count <= 0)
                                        {
                                            getter = "undefined";
                                        }
                                    /*    else
                                        {
                                            getter = stack.Pop();
                                        }*/

                                        result += CurrentRefference + " = " + getter  + ";";
                                      //  Writer.WriteLine(result);

                                        break;
                                    case Opcodes.OPCodes.OP_EvalLevelFieldVariableRef:
                                        CurrentRefference = "level." + opstack.Val;
                                        int index = GetIndexatpos(Export, opstack.Position);

                                        // C'est à test
                                        if (Export.FunctionOperands[index + 1].Opcode == Opcodes.OPCodes.OP_EvalArrayRef)
                                        {
                                            
                                        }else if(Export.FunctionOperands[index - 1].Opcode == Opcodes.OPCodes.OP_AddToArrayField)
                                        {
                                            result += CurrentRefference + " = {";
                                            int count = stack.Count;
                                            for (int i = 0; i < count; i++)
                                            {
                                               
                                                result += stack.Pop() + ",";
                                            }
                                            result += "};";
                                            Writer.WriteLine(result);

                                        }
                                        else
                                        {
                                            if (stack.Count > 0)
                                            {
                                                result += CurrentRefference + " = " + stack.Pop() + ";";
                                            }
                                            else
                                            {
                                                result += CurrentRefference + " = undefined;";
                                            }
                                            Writer.WriteLine(result);
                                        }
                                        break;
                                }
                                break;
                            }
                        case Opcodes.OPCodes.OP_JumpOnFalse:
                        case Opcodes.OPCodes.OP_JumpOnTrue:
                        case Opcodes.OPCodes.OP_JumpOnFalseExpr:
                        case Opcodes.OPCodes.OP_JumpOnTrueExpr:
                        case Opcodes.OPCodes.OP_Jump:
                        case Opcodes.OPCodes.OP_JumpBack:
                        case Opcodes.OPCodes.OP_GreaterThanAndJumpOnFalse:
                        case Opcodes.OPCodes.OP_LessThanAndJumpOnFalse:
                        {
                         //       opstack.visited = true;
                                switch (opstack.Opcode)
                                {
                                    case Opcodes.OPCodes.OP_Jump:
                                        {
                                            if ((short)opstack.Val > 0)
                                            {
                                                //Une loop
                                            }
                                            else
                                            {

                                            }
                                            break;
                                        }
                                    case Opcodes.OPCodes.OP_JumpOnFalse:
                                    case Opcodes.OPCodes.OP_JumpOnTrue:
                                    case Opcodes.OPCodes.OP_GreaterThanAndJumpOnFalse:
                                    case Opcodes.OPCodes.OP_LessThanAndJumpOnFalse:
                                    {
                                            /*
                                             * On check si c'est un jump "spécial" si oui alors on trie
                                             * ausinon c'est un if/else tout simplement
                                             */

                                            if (opstack.isDoWhile || opstack.isFor || opstack.isForeach || opstack.isInfinitefor || opstack.isWhile)
                                            {
                                                if (opstack.isDoWhile)
                                                {
                                                    Writer.WriteLine("isDoWhile");

                                                }
                                                else if (opstack.isFor)
                                                {
                                                    string result = "";
                                                    result += "for( ";
                                                   
                                                    if(opstack.Opcode == Opcodes.OPCodes.OP_LessThanAndJumpOnFalse)
                                                    {
                                                        string var2 = stack.Pop();
                                                        string var1 = stack.Pop();
                                                        stack.Push(var1 + " < " + var2);
                                                    }
                                                    else if(opstack.Opcode == Opcodes.OPCodes.OP_GreaterThanAndJumpOnFalse)
                                                    {
                                                        string var2 = stack.Pop();
                                                        string var1 = stack.Pop();
                                                        stack.Push(var1 + " > " + var2);
                                                    }
                                                    result += CurrentRefference;
                                                    result += "; " + stack.Pop();
                                                    result += ";" + opstack.Saver[0] + ")";
                                                    Writer.WriteLine(result);
                                                    Writer.WriteLine("{");
                                                    Writer.WriteLine(ResolveIf(opstack, Export));
                                                 //   Writer.WriteLine("isFor");

                                                }
                                                else if (opstack.isForeach)
                                                {
                                                    Writer.WriteLine("isForeach");
                                                    string result = "";
                                                    result += "foreach(" + opstack.Saver[0] + " in " + opstack.Saver[1] + ")";
                                                    Writer.WriteLine(result);
                                                    Writer.WriteLine("{");
                                                    Writer.WriteLine(ResolveIf(opstack, Export));


                                                }
                                                else if (opstack.isWhile)
                                                {
                                                    string result = "";
                                                    result += "while(";
                                                    Writer.WriteLine(stack.Count);
                                                    result += CreateCondition(opstack, Export) + ")";
                                                    Writer.WriteLine(result);
                                                    Writer.WriteLine("{");
                                                    Writer.WriteLine(ResolveIf(opstack, Export));
                                                }
                                                else if (opstack.isInfinitefor)
                                                {
                                                    Writer.WriteLine("isInfinitefor");

                                                }

                                            }
                                            else
                                            {
                                                {
                                                 string result = "";
                                                 result += "if(";
                                                if (opstack.Opcode == Opcodes.OPCodes.OP_GreaterThanAndJumpOnFalse)
                                                {
                                                        var val2 = stack.Pop();
                                                        var val1 = stack.Pop();
                                                        stack.Push(val1 + " > " + val2);
                                                }else if ( opstack.Opcode == Opcodes.OPCodes.OP_LessThanAndJumpOnFalse)
                                                    {
                                                        var val2 = stack.Pop();
                                                        var val1 = stack.Pop();
                                                        stack.Push(val1 + " < " + val2);
                                                    }

                                                

                                                    result += CreateCondition(opstack, Export) + ")";
                                                    Writer.WriteLine(result);
                                                    Writer.WriteLine("{");
                                                    Writer.WriteLine(ResolveIf(opstack, Export));
                                                }


                                                if (HasElse(Export, opstack)) // Après avoir traité le if  on regade si il a un else
                                                {
                                                    string result = "";
                                                    Writer.WriteLine("else");
                                                    Writer.WriteLine("{");
                                                    Writer.WriteLine(ResolveElse(opstack, Export));
                                                }

                                            }





                                            break;
                                        }
                                    case Opcodes.OPCodes.OP_JumpOnFalseExpr: // en gros c'est ce qu'il y a dans les parenthèses sur plusieurs opcodes
                                    case Opcodes.OPCodes.OP_JumpOnTrueExpr:
                                        {
                                            string result = (stack.Pop() + (opstack.Opcode == Opcodes.OPCodes.OP_JumpOnFalseExpr ? " && " : " || " ) + CreateExpression(opstack, Export));
                                            stack.Push(result);
                                           // Writer.WriteLine(result);

                                            break;
                                        }

                                }
                                break;
                            }

                        case Opcodes.OPCodes.OP_Plus:
                        case Opcodes.OPCodes.OP_Bit_Or:
                        case Opcodes.OPCodes.OP_Bit_Xor:
                        case Opcodes.OPCodes.OP_ShiftLeft:
                        case Opcodes.OPCodes.OP_ShiftRight:
                        case Opcodes.OPCodes.OP_Minus:
                        case Opcodes.OPCodes.OP_Multiply:
                        case Opcodes.OPCodes.OP_Divide:
                        case Opcodes.OPCodes.OP_Modulus:
                        case Opcodes.OPCodes.OP_Bit_And:
                            {
                               // opstack.visited = true;

                                var val2 = stack.Pop();
                                var val1 = stack.Pop();
                                stack.Push(val1 + GetOpperand(opstack.Opcode) + val2);
                                break;
                            }

                        case Opcodes.OPCodes.OP_Inc:
                        case Opcodes.OPCodes.OP_Dec:
                        case Opcodes.OPCodes.OP_Bit_Not:
                            {
                                switch(opstack.Opcode)
                                {
                                    case Opcodes.OPCodes.OP_Inc:
                                        Writer.WriteLine("" + CurrentRefference + "++;");
                                        break;
                                    case Opcodes.OPCodes.OP_Dec:
                                        Writer.WriteLine("" + CurrentRefference + "--;");

                                        break;
                                    case Opcodes.OPCodes.OP_Bit_Not:
                                        Writer.WriteLine("~" + CurrentRefference + ";");

                                        break;
                                }
                                break;
                            }

                            /*
                             * Je cois que le waittill doit être push dans le stack
                             */
                        case Opcodes.OPCodes.OP_EndOn:
                        case Opcodes.OPCodes.OP_Notify:
                        case Opcodes.OPCodes.OP_WaitTill:
                        case Opcodes.OPCodes.OP_WaitTillMatch:
                        case Opcodes.OPCodes.OP_WaitTillFrameEnd:
                            {
                             //   opstack.visited = true;

                                string result = "";
                                bool push = false;
                                bool hasParam = true;
                                int size = stack.Count;
                                if (opstack.Opcode == Opcodes.OPCodes.OP_EndOn)
                                {
                                    string prearg = stack.Pop();
                                    result += prearg;
                                    result += " endon(";
                                    size = (short)opstack.Val;

                                }
                                else if(opstack.Opcode == Opcodes.OPCodes.OP_Notify)
                                {
                                    string prearg = stack.Pop();
                                    size = stack.Count;
                                    result += prearg;
                                    result += " notify(";

                                }else if(opstack.Opcode == Opcodes.OPCodes.OP_WaitTill)
                                {
                                    string prearg = stack.Pop();
                                    result += prearg;
                                    result += " waittill(";
                                    size = (short)opstack.Val;
                                    push = true;
                                }
                                else if(opstack.Opcode == Opcodes.OPCodes.OP_WaitTillFrameEnd)
                                {
                                    hasParam = false;
                                    result += " waittillframeend(";

                                }
                                else
                                {

                                }
                                if (hasParam)
                                {
                                    for (int i = 0; i < size; i++) // le stack doit être vide 
                                    {
                                        if (i + 1 == stack.Count)
                                        {
                                            result += "" + stack.Pop() + "";

                                        }
                                        else
                                        {
                                            result += "" + stack.Pop() + ",";
                                        }
                                    }
                                }
                                result += ");";
                                if (push)
                                {
                                    stack.Push(result);
                                }
                                else
                                {
                                    Writer.WriteLine(result);
                                }
                                break;
                            }

                        case Opcodes.OPCodes.OP_BoolNot:
                            {
                                var value = stack.Pop();

                                if (value.Contains("&&") || value.Contains("||"))
                                {
                                    value = "(" + value + ")";
                                }

                                stack.Push(("!"+value));
                                break;
                            }
                        case Opcodes.OPCodes.OP_End:
                        case Opcodes.OPCodes.OP_Return:
                            {
                              //  opstack.visited = true;

                                if (Opcodes.OPCodes.OP_Return == opstack.Opcode)
                                {
                                    Writer.WriteLine("return {0};", stack.Pop());
                                }else
                                {
                                    Writer.WriteLine("return;");
                                }
                                break;
                            }
                        case Opcodes.OPCodes.OP_SizeOf:
                         //   opstack.visited = true;
                            stack.Push(stack.Pop() + ".size");
                            break;
                        case Opcodes.OPCodes.OP_LessThan:
                        case Opcodes.OPCodes.OP_GreaterThan:
                        case Opcodes.OPCodes.OP_GreaterThanOrEqualTo:
                        case Opcodes.OPCodes.OP_LessThanOrEqualTo:
                        case Opcodes.OPCodes.OP_NotEqual:
                        case Opcodes.OPCodes.OP_Equal:
                        case Opcodes.OPCodes.OP_SuperEqual:
                        case Opcodes.OPCodes.OP_SuperNotEqual:
                            {
                        //        opstack.visited = true;
                                var val2 = stack.Pop();
                                var val1 = stack.Pop();
                                stack.Push(val1 + GetComparaison(opstack.Opcode) + val2);
                            
                                break;
                            }

                        case Opcodes.OPCodes.OP_EvalArray:
                            {
                         //       opstack.visited = true;
                                var variable = stack.Pop();
                                var key = stack.Pop();
                                stack.Push(variable + "[" + key + "]");
                                break;
                            }
                        case Opcodes.OPCodes.OP_EvalArrayRef: // on ajoute à la ref
                            {
                            //    opstack.visited = true;
                                var key = stack.Pop();
                                CurrentRefference += "[" + key + "]";
                                break;
                            }
                        case Opcodes.OPCodes.OP_DevblockBegin:
                            {
                            //    opstack.visited = true;
                                string result = "";
                                var size = opstack.Val;
                                result += "/# \n";

                                //SpecialBlockSize = GetSizeBlock(Export, opstack.Position, (int)size);
                                //Devblock = true;
                                Writer.WriteLine(result);
                                //Writer.WriteLine("COUNT OF OPCODE IN DEV BLOCK : "+ count);
                                break;
                            }
                        case Opcodes.OPCodes.OP_Invalid:
                            break;


                        /*
                         * ICI C'est la zone de test pour les nouveaux Opcodes
                         */

                              case Opcodes.OPCodes.OP_IsDefinedAndOP_EvalLocalVariableRefCached: // lui on doit le push dans le stack
                                  {
                                      string result = "";
                                      var reference = opstack.Val;
                                    result += "isdefined(" + LocalVar[LocalVar.Count - (short)opstack.Val - 1] + " )";
                                    stack.Push(result);
                                      break;
                                  }
                    }
                }else
                {
                  //  Writer.WriteLine("already visited");
                }
               
            }catch(Exception e)
            {
                Writer.WriteLine("ERROR" + e);
            }

        }

        private string ResolveElse(ScriptOpStack opstack, Decompile.ExportStruct export)
        {
            uint endPos = (uint)Opcodes.AlignValue((int)((short)opstack.Val + opstack.OpSize + opstack.Position), 2);
            int Instruction = GetIndexatpos(export, endPos);
            ScriptOpStack op = export.FunctionOperands[Instruction - 1];

            uint JumpEndPos = (uint)Opcodes.AlignValue((int)((short)op.Val + op.OpSize + op.Position), 2);
            int StartIndex = GetIndexatpos(export, (uint)(op.Position + op.OpSize));
            int EndIndex = GetIndexatpos(export, JumpEndPos);
            for(int i = StartIndex; i < EndIndex; i++)
            {
                ResolveOpcode(export.FunctionOperands[i], export);
                export.FunctionOperands[i].visited = true;
            }

            return "}";
        }

        private bool HasElse(Decompile.ExportStruct export, ScriptOpStack opstack)
        {
            uint endPos = (uint)Opcodes.AlignValue((int)((short)opstack.Val + opstack.OpSize + opstack.Position),2);
            
            int Instruction = GetIndexatpos(export, endPos);
            if (Instruction != 0 ) {
                if (export.FunctionOperands[Instruction - 1].Opcode == Opcodes.OPCodes.OP_Jump)
                {
                    return true;
                }
            }
            return false;
        }
        private bool IsElseIf(Decompile.ExportStruct export, ScriptOpStack opstack)
        {
            short size = (short)opstack.Val;
            uint endPos = (uint)Opcodes.AlignValue((int)((short)opstack.Val + opstack.OpSize + opstack.Position), 2);

            int Instruction = GetIndexatpos(export, endPos);
            return false;
        }
        private string ResolveIf(ScriptOpStack opstack, Decompile.ExportStruct export)
        {
            /*On ajoute la taille pour passer l'opcode et aller aux endpoint*/
            uint endPos = (uint)Opcodes.AlignValue((int)((short)opstack.Val + opstack.OpSize + opstack.Position), 2);
            /*Position de l'opcode avant d'être lu*/
            int StartIndex = GetIndexatpos(export, (uint)(opstack.Position + opstack.OpSize));
            int EndIndex = GetIndexatpos(export, endPos);
         //   Writer.WriteLine(endPos);

          //  Writer.WriteLine(StartIndex + "    " + EndIndex);

            opstack.visited = true;
            for (int i = StartIndex; i < EndIndex ; i++ )
            {
                if (!export.FunctionOperands[i].visited)
                {
                  //  Writer.WriteLine(i);
                  //  Writer.WriteLine(export.FunctionOperands[i].Opcode);
                    ResolveOpcode(export.FunctionOperands[i], export);
                    export.FunctionOperands[i].visited = true;
                }
            }

            return "}";
        }

        private string CreateExpression(ScriptOpStack opstack , Decompile.ExportStruct Export)
        {
            string result = "";
            short size = (short)opstack.Val;
            uint endoffset = (uint)Opcodes.AlignValue((int)(opstack.Position + opstack.OpSize + size),2);
            int start = GetIndexatpos(Export, (uint)(opstack.Position + opstack.OpSize));
            int end = GetIndexatpos(Export, (uint)(endoffset));
            bool multipleExpression = false;
            for (int i = start; i < end; i++)
            {
                ScriptOpStack op = Export.FunctionOperands[i];

                if (op.Opcode == Opcodes.OPCodes.OP_JumpOnFalseExpr || op.Opcode == Opcodes.OPCodes.OP_JumpOnTrueExpr)
                {
                    multipleExpression = true;
                }
                if (!(Export.FunctionOperands[i].Opcode != Opcodes.OPCodes.OP_Wait && Export.FunctionOperands[i].Opcode != Opcodes.OPCodes.OP_WaitRealTime))
                {
                    continue; // on skip
                }

                ResolveOpcode(Export.FunctionOperands[i], Export);
                Export.FunctionOperands[i].visited = true;
            }
            result += stack.Pop();

            if (multipleExpression)
            {
                result = "(" + result + ")";
            }
            return result;
        }
        private string CreateCondition(ScriptOpStack opstack , Decompile.ExportStruct Export)
        {
            string result = "";
            int start = GetIndexatpos(Export, (uint)(opstack.Position));
            bool multipleExpression = false;
            result += stack.Pop();
            for (int i = start; i < Export.FunctionOperands.Count; i++)
            {
                ScriptOpStack op = Export.FunctionOperands[i];
                if (op.Opcode == Opcodes.OPCodes.OP_JumpOnFalseExpr || op.Opcode == Opcodes.OPCodes.OP_JumpOnTrueExpr)
                {
                    multipleExpression = true;
                }
                if (Export.FunctionOperands[i].Opcode == Opcodes.OPCodes.OP_Wait || Export.FunctionOperands[i].Opcode == Opcodes.OPCodes.OP_WaitRealTime ||
                    Export.FunctionOperands[i].Opcode == Opcodes.OPCodes.OP_DecTop || Export.FunctionOperands[i].Opcode == Opcodes.OPCodes.OP_EvalLocalArrayRefCached ||
                    Export.FunctionOperands[i].Opcode == Opcodes.OPCodes.OP_EvalLocalVariableRefCached || Export.FunctionOperands[i].Opcode == Opcodes.OPCodes.OP_SetVariableField ||
                    Export.FunctionOperands[i].Opcode == Opcodes.OPCodes.OP_SafeSetVariableFieldCached || Export.FunctionOperands[i].Opcode == Opcodes.OPCodes.OP_Inc ||
                    Export.FunctionOperands[i].Opcode == Opcodes.OPCodes.OP_Dec || Export.FunctionOperands[i].Opcode == Opcodes.OPCodes.OP_Bit_Not || Export.FunctionOperands[i].Opcode == Opcodes.OPCodes.OP_EndOn ||
                    Export.FunctionOperands[i].Opcode == Opcodes.OPCodes.OP_Notify || Export.FunctionOperands[i].Opcode == Opcodes.OPCodes.OP_WaitTill || Export.FunctionOperands[i].Opcode == Opcodes.OPCodes.OP_Return ||
                    Export.FunctionOperands[i].Opcode == Opcodes.OPCodes.OP_End || Export.FunctionOperands[i].Opcode == Opcodes.OPCodes.OP_EndOn || Export.FunctionOperands[i].Opcode == Opcodes.OPCodes.OP_DevblockBegin)
                {
                    break;  // on  break;
                }
                if(op.Opcode == Opcodes.OPCodes.OP_JumpOnTrue && (short)op.Val > 0 )
                {
                    
                    if(multipleExpression)
                    {
                        result = "(" + result + ")";

                    }
                    result = "!" + result;
                    op.visited = true;
                    break;
                }
             /*   if(op.Opcode == Opcodes.OPCodes.OP_CheckClearParams || op.Opcode == Opcodes.OPCodes.OP_CheckClearParams)
                {
                    result += stack.Pop();
                }*/
                if(!op.visited)
                {
                    //Writer.WriteLine("Condition" + op.Opcode);
                    //ResolveOpcode(op, Export);

                }                

            }

            return result;
        }
        
        public void DetermineForEachLoop(Decompile.ExportStruct Export)
        {
            //On loop à travers tout les opcodes pour détecter des FirstArrayKey (significatif  des foreach mais pas tout le temps)
            for (int i = 0; i < Export.FunctionOperands.Count; i++)
            {
                    // Avant de faire plein de check on regarde si c'est pas déjà un de ces cas-ci question "optimisation"
                    if (!Export.FunctionOperands[i].isDoWhile || !Export.FunctionOperands[i].isFor || !Export.FunctionOperands[i].isWhile ||
                        !Export.FunctionOperands[i].isInfinitefor)
                    {
                            //On regarde si l'opcode est un FirstArrayKey
                            if (Export.FunctionOperands[i].Opcode == Opcodes.OPCodes.OP_FirstArrayKey || Export.FunctionOperands[i].Opcode == Opcodes.OPCodes.OP_NextArrayKey)
                            {
                        //On regarde si les 2 Opcodes d'avant sont ceux-ci , (significatif des foreach)
                        if (Export.FunctionOperands[i - 1].Opcode == Opcodes.OPCodes.OP_EvalLocalVariableRefCached &&
                                    Export.FunctionOperands[i - 2].Opcode == Opcodes.OPCodes.OP_EvalLocalVariableCached)
                                {


                                /*
                                 * Une fois qu'on trouvé le foreach on doit lui trouver son jump comme ça de #1
                                 *  C'est plus facile à parse et aussi pour savoir la taille du foreach :D 
                                 */
                                for (int j = i; j < Export.FunctionOperands.Count; j++)
                                {
                                    /*
                                     * Quand on hit un jump , c'est celui du foreach , le jump du foreach sera 
                                     * TOUJOURS le premier jump hit 
                                     */
                                    if(Export.FunctionOperands[j].Opcode == Opcodes.OPCodes.OP_JumpOnFalse ||
                                        Export.FunctionOperands[j].Opcode == Opcodes.OPCodes.OP_JumpOnTrue ||
                                        Export.FunctionOperands[j].Opcode == Opcodes.OPCodes.OP_GreaterThanAndJumpOnFalse ||
                                        Export.FunctionOperands[j].Opcode == Opcodes.OPCodes.OP_LessThanAndJumpOnFalse)
                                    {
                                        //On marque le jump comme un foeach
                                        Export.FunctionOperands[j].isForeach = true;


                                    /*
                                     * On ne parse pas les arguments par la function ResolveOpcode 
                                     * car c'est un cas spécial.
                                     * 
                                     * On marque les opcodes comme lus pour éviter de lire 2X les Opcodes
                                     */
                                    Export.FunctionOperands[j].Saver.Add(LocalVar[LocalVar.Count - (short)Export.FunctionOperands[i - 1].Val - 1]);
                                    Export.FunctionOperands[j].Saver.Add(LocalVar[LocalVar.Count + ~(short)Export.FunctionOperands[i - 2].Val]);

                                    Export.FunctionOperands[i - 1].visited = true;
                                    Export.FunctionOperands[i - 2].visited = true;


                                    break;

                                    }
                                }
                                }
                            }
                        
                    }
                
            }
        }

        public void DetermineIfWhileLoop(Decompile.ExportStruct Export)
        {
            for (int i = Export.FunctionOperands.Count -1 ; i >= 0; i--)
            {
                // on check si c'est un jump négatif car c'est comme ça qu'une boucle fonctionne elle revient à son endroit de
                // départ pour reitérer.
                if(Export.FunctionOperands[i].Opcode == Opcodes.OPCodes.OP_Jump && (short)Export.FunctionOperands[i].Val < 0 && !Export.FunctionOperands[i].visited)
                {
          

                    //check si on a un jump condition , si oui alors le while a des conditions 
                    uint JumpLocation = (uint)Opcodes.AlignValue((int)((short)Export.FunctionOperands[i].Val + Export.FunctionOperands[i].Position + Export.FunctionOperands[i].OpSize),2);
                    int start = GetIndexatpos(Export, JumpLocation);
                    int end = GetIndexatpos(Export, Export.FunctionOperands[i].Position);
                    int FirstJump = start;
                    bool hasParam = false;
                    if (Export.FunctionOperands[i - 1].Opcode == Opcodes.OPCodes.OP_Inc || Export.FunctionOperands[i - 1].Opcode == Opcodes.OPCodes.OP_Dec)
                    {
                        string itteratorType = "";
                        if(Export.FunctionOperands[i - 1].Opcode == Opcodes.OPCodes.OP_Inc)
                        {
                            itteratorType = "++";
                        }
                        else
                        {
                            itteratorType = "--"; 
                        }
                        /*
                         * On doit trouver le début du for
                         * Pour faire ça, On vas aller à la position du jump
                         * Pour skip tout ce qui est dans les crochets ( { } ) 
                         * L'endroit où on attérit, sera TOUJOUR le début des paramètres du for 
                         * on pourra  stock ça dans l'opcode.
                         * La structure habituelle d'un for est comme ceci : 
                         * 
                         * Avant le jump négatif, on a le type d'incrémentation ( ++ ; --)
                         * à la position du jump , on aura dans la plus pars des cas , 
                         *  la variable i = 0 
                         *  puis un OP_LessThanAndJumpOnFalse ou bien un OP_GreatherThanAndJumpOnFalse
                         *  qui définit la taille de notre block ( ce qu'il y a dans les crochet)
                         */
                        Export.FunctionOperands[i - 1].visited = true; //Pour éviter de lire 2X le même opcode
                        Export.FunctionOperands[i].visited = true; //Pour éviter de lire 2X le même opcode

                        for (int k = start; k < end; k++)
                        {
                            if (Export.FunctionOperands[k].Opcode == Opcodes.OPCodes.OP_GreaterThanAndJumpOnFalse ||
                                Export.FunctionOperands[k].Opcode == Opcodes.OPCodes.OP_LessThanAndJumpOnFalse ||
                                Export.FunctionOperands[k].Opcode == Opcodes.OPCodes.OP_JumpOnFalse ||
                                Export.FunctionOperands[k].Opcode == Opcodes.OPCodes.OP_JumpOnTrue)
                            {
                                Export.FunctionOperands[k].isFor = true;
                                Export.FunctionOperands[k].Saver.Add(itteratorType);
                                break;
                            }
                        }

                    }
                    else
                    {
                        for (int j = start; j < Export.FunctionOperands.Count; j++)
                        {
                            if (!(Export.FunctionOperands[j].Opcode != Opcodes.OPCodes.OP_Wait && Export.FunctionOperands[j].Opcode != Opcodes.OPCodes.OP_WaitRealTime))
                            {
                                break;
                            }

                            if (Export.FunctionOperands[j].Opcode == Opcodes.OPCodes.OP_JumpOnTrue ||
                                Export.FunctionOperands[j].Opcode == Opcodes.OPCodes.OP_JumpOnFalse ||
                                Export.FunctionOperands[j].Opcode == Opcodes.OPCodes.OP_GreaterThanAndJumpOnFalse ||
                                Export.FunctionOperands[j].Opcode == Opcodes.OPCodes.OP_LessThanAndJumpOnFalse)
                            {
                                hasParam = true;
                                FirstJump = j;
                                break;
                            }
                        }
                        if (hasParam)
                        {
                            // Attention on est à l'envers ducoup c'est +1 et non -1
                            if (Export.FunctionOperands[i + 1].Opcode == Opcodes.OPCodes.OP_Inc || Export.FunctionOperands[i + 1].Opcode == Opcodes.OPCodes.OP_Dec)
                            {
                                Export.FunctionOperands[FirstJump].isFor = true;
                            }
                            else
                            {
                                Export.FunctionOperands[FirstJump].isWhile = true;


                            }
                        }
                        else
                        {
                            Export.FunctionOperands[FirstJump].isInfinitefor = true;

                        }
                    }

                }
            }

        }
         public void DetermineIf(Decompile.ExportStruct Export)
         {

         }
        public void DetermineElse(Decompile.ExportStruct Export)
        {

        }
        public void ProcessDevBlock(Decompile.ExportStruct Export)
        {
            /*for (int i = 0; i < 1; i++)
            {
                ResolveOpcode(Export.FunctionOperands[index + i], Export);
            }*/
        }
      
        public int GetSizeBlock(Decompile.ExportStruct Export, uint position,int endpos)
        {
            int count = 0;
            for (int i = index + 1, j = (int)position; j < endpos;i++, count++)
            {
                if(Export.FunctionOperands[i].Position == endpos)
                {
                    break;
                }
                j += Export.FunctionOperands[i].OpSize;
            }
            return count - 1;
            
        }

        public Opcodes.OPCodes GetOpAtPosition(Decompile.ExportStruct Export, uint pos)
        {
            for(int i = 0; i < Export.FunctionOperands.Count;i++)
            {
                if(Export.FunctionOperands[i].Position == pos)
                {
                    return Export.FunctionOperands[i].Opcode;
                }
            }
            return Opcodes.OPCodes.OP_Invalid;
        }
        public int GetIndexatpos(Decompile.ExportStruct Export, uint pos)
        {
            for(int i = 0; i < Export.FunctionOperands.Count;i++)
            {
              //  Writer.WriteLine(Export.FunctionOperands[i].Opcode + "   " + Export.FunctionOperands[i].Position.ToString("X"));
                if(Export.FunctionOperands[i].Position == pos)
                {
                    return i;
                }
            }
            return 0;
        }

        public string GetOpperand(Opcodes.OPCodes op)
        {
            switch(op)
            {
                case Opcodes.OPCodes.OP_Plus:
                    return " + ";
                case Opcodes.OPCodes.OP_Bit_Or:
                    return " | ";
                case Opcodes.OPCodes.OP_Bit_Xor:
                    return " ^ ";    
                case Opcodes.OPCodes.OP_ShiftLeft:
                    return " << ";
                case Opcodes.OPCodes.OP_ShiftRight:
                    return " >> ";
                case Opcodes.OPCodes.OP_Minus:
                    return " - ";
                case Opcodes.OPCodes.OP_Multiply:
                    return " * ";
                case Opcodes.OPCodes.OP_Divide:
                    return " / ";
                case Opcodes.OPCodes.OP_Modulus:
                    return " % ";
                case Opcodes.OPCodes.OP_Bit_And:
                    return " & ";
            }
            return "";
        }
        public string GetComparaison(Opcodes.OPCodes op)
        {
            switch(op)
            {
                case Opcodes.OPCodes.OP_LessThan:
                    return " < ";
                case Opcodes.OPCodes.OP_GreaterThan:
                    return " > ";
                case Opcodes.OPCodes.OP_GreaterThanOrEqualTo:
                    return " >= ";
                case Opcodes.OPCodes.OP_LessThanOrEqualTo:
                    return " <= ";
                case Opcodes.OPCodes.OP_NotEqual:
                    return " != ";
                case Opcodes.OPCodes.OP_Equal:
                    return " == ";
                case Opcodes.OPCodes.OP_SuperEqual:
                    return " === ";
                case Opcodes.OPCodes.OP_SuperNotEqual:
                    return " !== ";
            }
            return "";
        }
        public string ResolveCalls(string function, int paramsCount , bool thread , bool method)
        {
         //  Writer.WriteLine(stack.Count);
            string result = "";
            if(method)
            {
                result += stack.Pop() + " ";

            }
            if (thread)
            {
                result += "thread ";
            }
            result += function + "(";

            for (int i = 0; i < paramsCount; i++)
            {

                if(i+1 == paramsCount)
                {
                    result += stack.Pop();
                }else
                {
                    result += stack.Pop() + ", ";
                }

            }
            result += ")";
            return result;
        }
        public void ResolveFunctionParams(Decompile.ExportStruct Export)
        {
            /*
             * Si la fonction a des params alors le premier OPcode devrait être SafeCreateLocalVar....
             *  Et alors on prends les premières variables qui viennent dans le stack
             */
            if(Export.FunctionParam > 0)
            {
                    for(int i = 0; i < Export.FunctionParam;i++)
                    {
                        if((i+1) == Export.FunctionParam) // Pour éviter la , à la fin
                        {
                            Writer.Write(LocalVar[i]);

                        }
                        else
                        {
                            Writer.Write(LocalVar[i] + ",");
                        }

                    }
                
            }
        }
    }
}
