using System;
using StompClient;
using WebSocketSharp;

namespace StompTransportWebsocketSharp {

  public class WebSocketTransport : ITransport {

    public event TrasnportEventHandler OnClose;
    public event TrasnportEventHandler OnError;
    public event TrasnportEventHandler OnMessage;
    public event TrasnportEventHandler OnOpen;

    private WebSocket socket;

    public WebSocketTransport(WebSocket ws) {
      socket = ws;
      if (socket != null) {
        bind();
      }
    }

    private void bind() {
      socket.OnOpen += Socket_OnOpen;
      socket.OnClose += Socket_OnClose;
      socket.OnError += Socket_OnError;
      socket.OnMessage += Socket_OnMessage;
    }

    private void Socket_OnMessage(object sender, MessageEventArgs e) {
      OnMessage?.Invoke(this, new TrasnportEventArgs(e.Data));
    }

    private void Socket_OnError(object sender, ErrorEventArgs e) {
      OnError?.Invoke(this, new TrasnportEventArgs(e.Message, e.Exception));
    }

    private void Socket_OnClose(object sender, CloseEventArgs e) {
      OnClose?.Invoke(this, TrasnportEventArgs.Empty);
    }

    private void Socket_OnOpen(object sender, EventArgs e) {
      OnOpen?.Invoke(this, TrasnportEventArgs.Empty);
    }

    public void Connect() {
      socket.Connect();
    }

    public void Send(byte[] data) {
      socket.Send(data);
    }

    public void Send(string data) {
      socket.Send(data);
    }
  }
}
