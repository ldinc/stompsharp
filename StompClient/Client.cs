using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace StompClient {

  #region Event Defenition

  public delegate void OnConnectedHandler(object sender);

  public delegate void OnMessageHandler(object sender, string message, ContentType cType, string subscription, string destination, string messageId);

  public delegate void OnErrorHandler(object sender, string message, Exception ex);

  #endregion Event Defenition

  public class Client {
    private Client() { }
    private ITransport transport;

    #region Public Properties

    public int Id {
      get;
      private set;
    }

    #region Connect Headers

    // TODO: http://stomp.github.io/stomp-specification-1.2.html#Protocol_Negotiation
    public AcceptVersion AcceptVersion;
    public string Host;
    public string Login;
    public string Passcode;
    public uint OutgoingHeartbeat; // cx - (http://stomp.github.io/stomp-specification-1.2.html#Heart-beating)
    public uint IncomingHeartbeat; // cy - (http://stomp.github.io/stomp-specification-1.2.html#Heart-beating)

    #endregion Connect Headers

    #region Connected Headers

    public AcceptVersion Version { get; private set; }
    public string Session { get; private set; }
    public string Server { get; private set; }

    #endregion Connected Headers

    #region Subscribe Headers

    public Acknowledge Ack;

    #endregion Subscribe Headers

    #endregion Public Properties

    #region Events

    public event OnConnectedHandler OnConnected;
    public event OnMessageHandler OnMessage;
    public event OnErrorHandler OnError;

    #endregion

    public static Client Over(ITransport transport) {
      var client = new Client {
        transport = transport,
        AcceptVersion = AcceptVersion.ALL,
        Host = "unkown",
        Login = null,
        Passcode = null,
        OutgoingHeartbeat = 0,
        IncomingHeartbeat = 0,
        Session = null,
        Server = null,
        Id = 0,
        Ack = Acknowledge.Auto,
      };
      client.transport.OnMessage += client.Transport_OnMessage;
      client.transport.OnError += client.Transport_OnError;

      return client;
    }

    private void Transport_OnError(object sender, TrasnportEventArgs e) {
      OnError?.Invoke(this, e.Message, e.Exception);
    }

    private void Transport_OnMessage(object sender, TrasnportEventArgs e) {
      //Console.WriteLine("<<<\n{0}<<<", e.Message);
      var frame = Frame.Parse(e.Message);
      switch (frame.Type) {
        case FrameType.CONNECTED:
          HandleConnected(frame);
          break;
        case FrameType.ERROR:
          HandleError(frame);
          break;
        case FrameType.MESSAGE:
          HandleNewMessage(frame);
          break;
      }
    }

    #region Frame Handlers

    private void HandleNewMessage(Frame frame) {
      string subscription = frame.Headers["subscription"];
      string messageId = frame.Headers["message-id"];
      string dest = frame.Headers["destination"];
      string cTypeString = frame.Headers["content-type"];
      ContentType cType;
      if (string.IsNullOrEmpty(cTypeString)) {
        cType = new ContentType("text/plain");
      } else {
        cType = new ContentType(cTypeString);
      }
      var handler = OnMessage;
      handler?.Invoke(this, frame.Body, cType, subscription, dest, messageId);
    }

    private void HandleConnected(Frame frame) {
      Version = Version.Parse(frame.Headers["version"]);
      Session = frame.Headers["session"];
      Server = frame.Headers["server"];
      // http://stomp.github.io/stomp-specification-1.2.html#Heart-beating
      string serverHeartbeat = frame.Headers["heart-beat"];
      if (!string.IsNullOrEmpty(serverHeartbeat)) {
        var beats = serverHeartbeat.Split(',');
        int sx = 0;
        int.TryParse(beats[0], out sx);
        int sy = 0;
        int.TryParse(beats[1], out sy);
        if (IncomingHeartbeat == 0 || sy == 0) {
          IncomingHeartbeat = 0;
        } else {
          IncomingHeartbeat = (uint)Math.Max(IncomingHeartbeat, sy);
        }
        if (OutgoingHeartbeat == 0 || sx == 0) {
          OutgoingHeartbeat = 0;
        } else {
          OutgoingHeartbeat = (uint)Math.Max(OutgoingHeartbeat, sx);
        }
      }
      // raise on connected event
      var handlers = OnConnected;
      handlers?.Invoke(this);
    }


    // https://stomp.github.io/stomp-specification-1.2.html#Protocol_Negotiation
    // https://stomp.github.io/stomp-specification-1.2.html#ERROR
    private void HandleError(Frame frame) {
      var message = frame.Headers["message"];
      var extra = frame.Body;
      var handlers = OnError;
      Exception exception = null;
      if (!string.IsNullOrEmpty(extra)) {
        exception = new Exception(extra);
      }
      handlers?.Invoke(this, message, exception);
    }

    #endregion Frame Handlers

    public void Connect() {
      // 2 requiered headers + 3 optional
      var frame = new Frame(5);
      frame.Type = FrameType.CONNECT;
      frame.Headers.Add(AcceptVersion.ToKeyValue());
      frame.Headers["host"] = Host;
      if (!string.IsNullOrWhiteSpace(Login)) {
        frame.Headers["login"] = Login;
      }
      if (!string.IsNullOrWhiteSpace(Passcode)) {
        frame.Headers["passcode"] = Passcode;
      }
      frame.Headers["heart-beat"] = string.Format("{0},{1}", OutgoingHeartbeat, IncomingHeartbeat);
      transport.Connect();
      transport.Send(frame.ToString());
    }

    public int Subscribe(string dest) {
      // 2 requiered headers + 1 semirequired (set auto on null)
      var frame = new Frame(3);
      frame.Type = FrameType.SUBSCRIBE;
      var id = Id++;
      frame.Headers["id"] = string.Format("sub-{0}", id);
      frame.Headers["destination"] = dest;
      frame.Headers.Add(Ack.ToKeyValue());
      transport.Send(frame.ToString());
      return id;
    }

    public void Unsubscribe(int id) {
      // 1 requiered header
      var frame = new Frame(1);
      frame.Type = FrameType.UNSUBSCRIBE;
      frame.Headers["id"] = string.Format("sub-{0}", id);
      transport.Send(frame.ToString());
    }

    public void Send(string dest, string msg) {
      var frame = new Frame(3);
      frame.Type = FrameType.SEND;
      frame.Headers["destination"] = dest;
      if (!string.IsNullOrEmpty(dest)) {
        frame.Headers["content-type"] = "text/plain;charset=UTF-8";
        frame.Body = msg;
        frame.EncodedBody = StringHelper.Encode(msg);
        // idk what len expected here (escaped or original text len)
        // http://stomp.github.io/stomp-specification-1.2.html#Header_content-length
        // as i think - frames SHOULD include a content-length header to ease frame parsing
        // means encoded len
        frame.Headers["content-length"] = frame.EncodedBody.Length.ToString();
      }
      string data = frame.ToString();
      transport.Send(data);
    }

  }
}
