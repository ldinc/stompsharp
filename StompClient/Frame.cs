using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StompClient {

  public enum FrameType {
    NONE,
    CONNECT,
    CONNECTED,
    SUBSCRIBE,
    UNSUBSCRIBE,
    SEND,
    MESSAGE,
    ERROR,
  }

  public class Frame {

    public FrameType Type;

    public HeadersSet Headers;

    public string Body;
    public string EncodedBody;

    public Frame(int headersCount) {
      Headers = new HeadersSet(headersCount);
    }

    public override string ToString() {
      string result = TypeToString() + Headers.ToString() + "\r\n";
      if (EncodedBody != null && EncodedBody.Length > 0) {
        result += EncodedBody;
      }
      result += "\0"; // added body & terminate sign
      //Console.WriteLine("Frame:dbg:[{0}]", result);
      return result;
    }

    private string TypeToString() {
      switch(Type) {
        case FrameType.CONNECT:
          return "CONNECT\r\n";
        case FrameType.CONNECTED:
          return "CONNECTED\r\n";
        case FrameType.SUBSCRIBE:
          return "SUBSCRIBE\r\n";
        case FrameType.UNSUBSCRIBE:
          return "UNSUBSCRIBE\r\n";
        case FrameType.SEND:
          return "SEND\r\n";
        case FrameType.MESSAGE:
          return "MESSAGE\r\n";
        case FrameType.ERROR:
          return "ERROR\r\n";
        default:
          return string.Empty;
      }
    }

    private static FrameType StringToFrameType(string raw) {
      switch (raw) {
        case "CONNECT":
          return FrameType.CONNECT;
        case "CONNECTED":
          return FrameType.CONNECTED;
        case "SUBSCRIBE":
          return FrameType.SUBSCRIBE;
        case "UNSUBSCRIBE":
          return FrameType.UNSUBSCRIBE;
        case "SEND":
          return FrameType.SEND;
        case "MESSAGE":
          return FrameType.MESSAGE;
        case "ERROR":
          return FrameType.ERROR;
        default:
          return FrameType.NONE;
      }
    }

    public static Frame Parse(string raw) {
      var elems = raw.Split(new string[] { "\n\r", "\n" }, StringSplitOptions.None);
      // find body delim
      int delim;
      for (delim = 0; delim < elems.Length; delim ++) {
        if (string.IsNullOrEmpty(elems[delim])) {
          break;
        }
      }
      var n = delim - 1;
      var frame = new Frame(n);
      frame.Type = StringToFrameType(elems[0]);
      for (var i = 0; i < n; i++) {
        var keyval = elems[i + 1].Split(':');
        frame.Headers[keyval[0]] = StringHelper.Decode(keyval[1]);
      }
      // parse body
      if (delim == elems.Length - 1) {
        frame.Body = string.Empty;
        frame.EncodedBody = string.Empty;
      } else {
        frame.EncodedBody = elems[delim + 1];
        // delete termination sym
        if (frame.EncodedBody[frame.EncodedBody.Length - 1] == '\0') {
          frame.EncodedBody = frame.EncodedBody.Substring(0, frame.EncodedBody.Length - 1);
        }
        frame.Body = StringHelper.Decode(frame.EncodedBody);
      }

      return frame;
    }

  }
}
