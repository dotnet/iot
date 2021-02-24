using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [Flags]
    internal enum InstructionKind
    {
        None,
        Normal,
        Branch = 2,
        BranchTarget = 4,
    }

    internal class IlInstruction
    {
        public IlInstruction(OpCode instruction, int pc)
        {
            OpCode = instruction;
            Pc = pc;
            PreviousInstructions = new List<IlInstruction>();
            IsReachable = false;
        }

        public OpCode OpCode
        {
            get;
        }

        public int Pc
        {
            get;
        }

        /// <summary>
        /// Regularly next instruction. False case for a branch instruction
        /// </summary>
        public IlInstruction? NextInstruction
        {
            get;
            set;
        }

        public IlInstruction? BranchTarget
        {
            get;
            set;
        }

        public List<IlInstruction> PreviousInstructions
        {
            get;
        }

        public bool IsReachable
        {
            get;
            set;
        }
    }
}
