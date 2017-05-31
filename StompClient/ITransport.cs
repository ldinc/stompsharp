using System;

namespace StompClient {

  public delegate void TrasnportEventHandler(object sender, TrasnportEventArgs e);

  public class TrasnportEventArgs {
    public static readonly TrasnportEventArgs Empty;

    public string Message { get; private set; }
    public Exception Exception { get; private set; }

    public TrasnportEventArgs() { }

    public TrasnportEventArgs(string message) {
      Message = message;
    }

    public TrasnportEventArgs(string message, Exception ex) {
      Message = message;
      Exception = ex;
    }

  }

  public interface ITransport {

    event TrasnportEventHandler OnOpen;
    event TrasnportEventHandler OnClose;
    event TrasnportEventHandler OnError;
    event TrasnportEventHandler OnMessage;

    void Connect();

    void Send(string data);
    void Send(byte[] data);

  }
}
