using System.Collections.Generic;

namespace StompClient {

  public class HeadersSet {

    private Dictionary<string, string> map;

    public HeadersSet(int cap) {
      map = new Dictionary<string, string>(cap);
    }

    public string this[string key] {
      get {
        string value = null;
        if (map.TryGetValue(key, out value)) {
          return value;
        } else {
          return null;
        }
      }

      set {
        map[key] = value;
      }
    }

    public void Add(KeyValuePair<string, string> kv) {
      map[kv.Key] = kv.Value;
    }

    public override string ToString() {
      string result = string.Empty;
      foreach (var key in map.Keys) {
        result += string.Format("{0}:{1}\r\n", key, StringHelper.Encode(map[key]));
      }
      return result;
    }

  }
}
