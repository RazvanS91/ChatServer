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
            Assert.Equal(userName, participant.UserName);
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
            Assert.Equal(user, participant.UserName);
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
            Assert.Equal(userName, participant.UserName);
            Assert.Equal(message, participant.Message);

        }
    }
}
