using IBApi;
using System;
using System.Threading;

namespace ZeroTWS
{
    class Program
    {
        static void Main()
        {
            /// 创建Wrapper，并且获取创建好的clientSocket以及readerSignal
            ZeroEWrapper wrapper = new ZeroEWrapper();
            EClientSocket clientSocket = wrapper.ClientSocket;
            EReaderSignal readerSignal = wrapper.Signal;

            // 连接至服务器，IP地址，端口号，本客户端ID，默认为0
            // 关于0有什么用，我们在后面的文章里再详细说明
            clientSocket.eConnect("127.0.0.1", 7497, 0);

            // 创建一个reader，用于处理消息事件
            var reader = new EReader(clientSocket, readerSignal);
            reader.Start();

            // 创建后台线程，监控来自TWS的消息
            new Thread(() => {
                while (clientSocket.IsConnected())
                {
                    readerSignal.waitForSignal();
                    reader.processMsgs();
                }
            })
            { IsBackground = true }.Start();


            // 这一步会一直循环，直到我们获得了可用的OrderID，>0 时表示着此时开始，可以向TWS发送命名了。
            while (wrapper.NextOrderId <= 0) { }

            // 啥事也不干，就睡10s
            Thread.Sleep(10000);

            // 与TWS关闭连接
            Console.WriteLine("Disconnecting from TWS...");
            clientSocket.eDisconnect();

            // 类似于断点，我们可以查看到代码的输出
            Console.ReadKey();
        }
    }
}
