using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StompClient {

  #region header: accept-version

  public enum AcceptVersion {
    ALL, // 1.2,1.1,1.0
    V10, // 1.0
    V11, // 1.1
    V12  // 1.2
  }

  public static class AcceptVersionExtension {
    public static KeyValuePair<string, string> All(this AcceptVersion av) {
      return new KeyValuePair<string, string>("accept-version", "1.2,1.1,1.0");
    }

    public static KeyValuePair<string, string> Version10(this AcceptVersion av) {
      return new KeyValuePair<string, string>("accept-version", "1.0");
    }

    public static KeyValuePair<string, string> Version11(this AcceptVersion av) {
      return new KeyValuePair<string, string>("accept-version", "1.1");
    }

    public static KeyValuePair<string, string> Version12(this AcceptVersion av) {
      return new KeyValuePair<string, string>("accept-version", "1.2");
    }

    public static KeyValuePair<string, string> ToKeyValue(this AcceptVersion av) {
      switch (av) {
        case AcceptVersion.V10:
          return new KeyValuePair<string, string>("accept-version", "1.0");
        case AcceptVersion.V11:
          return new KeyValuePair<string, string>("accept-version", "1.1");
        case AcceptVersion.V12:
          return new KeyValuePair<string, string>("accept-version", "1.2");
        default:
          return new KeyValuePair<string, string>("accept-version", "1.2,1.1,1.0");
      }
    }

    public static AcceptVersion Parse(this AcceptVersion av, string raw) {
      switch (raw) {
        case "1.2":
          return AcceptVersion.V12;
        case "1.1":
          return AcceptVersion.V11;
        default:
          return AcceptVersion.V10;
      }
    }
  }

  #endregion header: accept-version

  #region header: ack

  public enum Acknowledge {
    Auto,            // auto
    Client,          // client
    ClientIndividual // client-individual
  }

  public static class AcknowledgeExtension {

    public static KeyValuePair<string, string> ToKeyValue(this Acknowledge ack) {
      switch (ack) {
        case Acknowledge.Client:
          return new KeyValuePair<string, string>("ack", "client");
        case Acknowledge.ClientIndividual:
          return new KeyValuePair<string, string>("ack", "client-individual");
        default:
          return new KeyValuePair<string, string>("ack", "auto");
      }
    }

  }

  #endregion header: ack

}
