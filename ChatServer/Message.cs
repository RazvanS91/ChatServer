﻿using System;
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
        {
            if (obj is Message msg)
                return this.data == msg.data;
            return base.Equals(obj);
        }

        public byte[] ToByteArray()
        {
            return Encoding.ASCII.GetBytes(data);
        }

    }
}
