using Microsoft.VisualStudio.TestTools.UnitTesting;
using CKLunchBot.Core.Requester;

namespace CKLunchBot.Core.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async void TestMethod1Async()
        {
            Assert.IsTrue(await new MenuRequester().RequestData() != null, "��û�� ���� ����");
        }
    }
}
