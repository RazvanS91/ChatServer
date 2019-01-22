using System;
using System.IO;
using System.Text;
using Xunit;

namespace ChatServer.Tests
{
    public class ParticipantFacts
    {
        [Fact]
        public void ReceivesTheUserName()
        {
            // given
            var userName = "test";
            var memoryStream = new MemoryStream();
            var size = BitConverter.GetBytes((short)userName.Length);
            memoryStream.Write(size);
            memoryStream.Write(Encoding.ASCII.GetBytes(userName));
            memoryStream.Seek(0, SeekOrigin.Begin);

            var participant = new ParticipantS(memoryStream);

            // when
            participant.ReceiveUsername();

            // then
            Assert.Equal(userName, participant.Username);
        }

        [Fact]
        public void ReceivesMessage()
        {
            var user = "username_given"; // 14
            var message = "This is a test string"; // 21
            var ms = new MemoryStream();

            ms.Write(BitConverter.GetBytes((short)user.Length));
            ms.Write(Encoding.ASCII.GetBytes(user));
            ms.Write(BitConverter.GetBytes((short)message.Length));
            ms.Write(Encoding.ASCII.GetBytes(message));
            ms.Seek(0, SeekOrigin.Begin);

            var participant = new ParticipantS(ms);

            participant.ReceiveUsername();
            participant.RetrieveMessage();

            Assert.Equal(message, participant.Message);
            Assert.Equal(user, participant.Username);
        }

        [Fact]
        public void ReceivesTheUserNameWhenTheResponseIsChuncked()
        {
            // given
            var userName = "test";
            var message = "message";
            var memoryStream = new ChunkedStringStream(userName, message);
            var participant = new ParticipantS(memoryStream);

            // when
            participant.ReceiveUsername();
            participant.RetrieveMessage();

            // then
            Assert.Equal(userName, participant.Username);
            Assert.Equal(message, participant.Message);

        }

        [Fact]
        public void ServerReceivesZeroBytes()
        {
            var username = "test";
            var message = "";
            var ms = new MemoryStream();
            var ms2 = new MemoryStream();

            ms.Write(BitConverter.GetBytes((short)username.Length));
            ms.Write(Encoding.ASCII.GetBytes(username));
            ms.Write(BitConverter.GetBytes((short)message.Length));
            ms.Write(Encoding.ASCII.GetBytes(message));
            ms.Seek(0, SeekOrigin.Begin);

            var participant = new ParticipantS(ms);
            var participant2 = new ParticipantS(ms2);
            var chatRoom = new ChatRoomS();

            chatRoom.Add(participant2);
            chatRoom.Join(participant);

            Assert.Equal("test lost connection !", participant2.Message);
            Assert.False(participant.isConnected);
        }

        [Fact]
        public void OneParticipantLosesConnection()
        {
            var username = "abc";
            var ms = new MemoryStream();
            var ms2 = new MemoryStream();
            var ms3 = new MemoryStream();

            ms.Write(BitConverter.GetBytes((short)username.Length));
            ms.Write(Encoding.ASCII.GetBytes(username));
            ms.Seek(0, SeekOrigin.Begin);

            var participant = new ParticipantS(ms);
            var participant2 = new ParticipantS(ms2);
            var participant3 = new ParticipantS(ms3);

            var chatRoom = new ChatRoomS();

            chatRoom.Add(participant);
            chatRoom.Add(participant2);
            chatRoom.Add(participant3);

            participant.ReceiveUsername();
            participant.Disconnect();

            chatRoom.SendMessageToAllClients(new Message("test"));
            Assert.False(participant.isConnected);
            Assert.Equal("abc lost connection !", participant2.Message);
            Assert.Equal("abc lost connection !", participant3.Message);
        }
    }
}
