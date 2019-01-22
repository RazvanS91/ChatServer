using System;
using System.Collections.Generic;
using System.Text;

namespace ChatServer
{
    public class Message
    {
        private readonly string data;
        public short Length { get; }

        public Message(string message)
        {
            data = message;
            Length = (short)data.Length;
        }

        public Message(byte[] message) : this(Encoding.ASCII.GetString(message))
        {
        }

        public Message(string user, Message msg)
        {
            data = $"{user} : {msg.data}";
            Length = (short)data.Length;
        }

        public override bool Equals(object obj)
            => obj is Message message ? data == message.data : base.Equals(obj);

        public byte[] ToByteArray()
        {
            return Encoding.ASCII.GetBytes(data);
        }

    }
}
