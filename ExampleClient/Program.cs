using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleClient {
  class Program {
    static void Main(string[] args) {
      var str = "hello \n my friend\r\n\t";
      var enc = StompClient.StringHelper.Encode(str);
      Console.WriteLine(enc);
      Console.WriteLine(StompClient.StringHelper.Decode(enc));

      var url = "ws://192.168.151.224:8080/sodch-websocket/websocket";
      var ws = new WebSocketSharp.WebSocket(url);
      ws.SetCredentials("-1", "1", true);
      var wstrans = new StompTransportWebsocketSharp.WebSocketTransport(ws);
      var client = StompClient.Client.Over(wstrans);
      client.OnConnected += (_) => {
        client.Subscribe("/topic/pong");
        client.Send("/app/ping", "Jibril!");
      };
      client.OnMessage += Client_OnMessage;
      client.Connect();

      Console.ReadKey(false);
    }

    private static void Client_OnMessage(object sender, string message, System.Net.Mime.ContentType cType, string subscription, string destination, string messageId) {
      Console.WriteLine("<<<\ndest: {0}\nid: {1}\nmsg: {2}\n<<<\n", destination, messageId, message);
    }
  }
}
