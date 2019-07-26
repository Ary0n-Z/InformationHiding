using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIH_core.Src.Methods
{
    public class MethodBase
    {
        protected static readonly byte[] StegoMark = { 76, 83, 66, 66, 101, 103, 105, 110 };
        protected const int msgSizeLength = sizeof(long);
        protected const int bitsInByte = 8;
        protected BitArray CreateEmbeddedMessage(byte[] message)
        {
            byte[] tmp = new byte[StegoMark.Length + msgSizeLength + message.Length];
            //Add message begin
            StegoMark.CopyTo(tmp, 0);
            //Add size of the message
            byte[] messageSize = BitConverter.GetBytes(message.LongLength);
            if (!BitConverter.IsLittleEndian)
                messageSize.Reverse();
            messageSize.CopyTo(tmp, StegoMark.LongLength);
            //Add message to embed
            message.CopyTo(tmp, StegoMark.Length + messageSize.Length);
            return new BitArray(tmp);
        }

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}
