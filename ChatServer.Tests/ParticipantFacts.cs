using System;
using System.IO;
using System.Text;
using Xunit;
using System.Threading.Tasks;

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
            var participant = new ParticipantS(memoryStream);
            var size = BitConverter.GetBytes((short)userName.Length);
            memoryStream.Write(size);
            memoryStream.Write(Encoding.ASCII.GetBytes(userName));
            memoryStream.Seek(0, SeekOrigin.Begin);


            // when
            TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();

            participant.StartConversation(m => taskCompletionSource.SetResult(1));
            Task.WaitAny(taskCompletionSource.Task, Task.Delay(1000));

            // then
            Assert.True(taskCompletionSource.Task.Status == TaskStatus.RanToCompletion);
            Assert.Equal(userName, participant.Username);
        }

        [Fact]
        public void ReceivesMessage()
        {
            var user = "username_given"; // 14
            var message = "This is a test string"; // 21
            var ms = new MemoryStream();
            var participant = new ParticipantS(ms);

            ms.Write(BitConverter.GetBytes((short)user.Length));
            ms.Write(Encoding.ASCII.GetBytes(user));
            ms.Write(BitConverter.GetBytes((short)message.Length));
            ms.Write(Encoding.ASCII.GetBytes(message));
            ms.Seek(0, SeekOrigin.Begin);

            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
            int calls = 0;

            participant.StartConversation(m => { calls++; if (calls == 2) tcs.SetResult(5); });
            Task.WaitAny(tcs.Task, Task.Delay(2000));

            Assert.True(tcs.Task.Status == TaskStatus.RanToCompletion);
            Assert.Equal(user, participant.Username);
            Assert.Equal(message, participant.Message);
            Assert.False(participant.isConnected);
        }
    }
}
