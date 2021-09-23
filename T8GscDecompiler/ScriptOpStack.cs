using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace T8GscDecompiler
{
    class ScriptOpStack
    {
        public Opcodes.OPCodes Opcode { get; set; }
        public object Val { get; set;}

        public bool visited = false;
       public uint Position { get; set; }
       public int OpSize { get; set; }
        public bool isWhile = false;
        public bool isInfinitefor = false;
        public bool isForeach = false;
        public bool isFor = false;
        public bool isDoWhile = false;
        public bool isSimpleIf = false;
        public bool isIfElse = false;
        public List<string> Saver = new List<string>();
     
        public ScriptOpStack(Opcodes.OPCodes Op,uint position,int size)
        {
            Opcode = Op;
            Position = position;
            OpSize = size;
        }
        public ScriptOpStack(Opcodes.OPCodes Op , int val1, uint position, int size)
        {
            Opcode = Op;
            Val = val1;
            Position = position;
            OpSize = size;

        }
        public ScriptOpStack(Opcodes.OPCodes Op , short val1, uint position, int size)
        {
            Opcode = Op;
            Val = val1;
            Position = position;
            OpSize = size;

        }
        public ScriptOpStack(Opcodes.OPCodes Op , float val1, uint position, int size)
        {
            Opcode = Op;
            Val = val1;
            Position = position;
            OpSize = size;

        }
        public ScriptOpStack(Opcodes.OPCodes Op , float x , float y , float z, uint position, int size)
        {
            Opcode = Op;
            Val = new Vector3(x, y, z);
            Position = position;
            OpSize = size;

        }
        public ScriptOpStack(Opcodes.OPCodes Op , string val1, uint position, int size)
        {
            Opcode = Op;
            Val = val1;
            Position = position;
            OpSize = size;

        }
        /* public ScriptOpStack(Opcodes.OPCodes Op)
         {
             Opcode = Op;
         }*/
    }
}
