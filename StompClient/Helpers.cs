using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StompClient {
  public class StringHelper {
    public static string Encode(string raw) {
      if (raw == null) {
        return null;
      }
      string result = string.Empty;
      using (var memStream = new MemoryStream(raw.Length)) {
        using (var writer = new StreamWriter(memStream)) {
          for (var i = 0; i < raw.Length; i++) {
            switch (raw[i]) {
              case '\r':
                writer.Write("\\r");
                break;
              case '\n':
                writer.Write("\\n");
                break;
              case (Char)58:
                writer.Write("\\c");
                break;
              case '\\':
                writer.Write("\\\\");
                break;
              default:
                writer.Write(raw[i]);
                break;
            }
          }
          writer.Flush();
          memStream.Position = 0;
          using (var reader = new StreamReader(memStream)) {
            result = reader.ReadToEnd();
          }
        }
      }
      return result;
    }

    public static string Decode(string raw) {
      if (raw == null) {
        return null;
      }
      string result = string.Empty;
      using (var memStream = new MemoryStream(raw.Length)) {
        using (var writer = new StreamWriter(memStream)) {
          for (var i = 0; i < raw.Length; i++) {
            Char fc = raw[i];
            if (fc == '\\' && i < raw.Length - 1) {
              Char sc = raw[i + 1];
              if (sc == 'r') {
                writer.Write('\r');
                i++;
                continue;
              } else if (sc == 'n') {
                writer.Write('\n');
                i++;
                continue;
              } else if (sc == 'c') {
                writer.Write((Char)58);
                i++;
                continue;
              } else if (sc == '\\') {
                writer.Write('\\');
                i++;
                continue;
              }
            } else {
              writer.Write(fc);
            }
          }
          writer.Flush();
          memStream.Position = 0;
          using (var reader = new StreamReader(memStream)) {
            result = reader.ReadToEnd();
          }
        }
      }
      return result;
    }

  }
}
