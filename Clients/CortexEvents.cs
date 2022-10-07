using UnityEngine;
using System.Collections.Generic;

public class RequestMethods
{
  public static string AuthorizeSession = "authorize";
  public static string QuerySession = "querySessions";
  public static string CreateSession = "createSession";
  public static string StreamMentalCommand = "subscribe";
}

[System.Serializable]
public class Request
{
  public int id = 1;
  public string jsonrpc = "2.0";
  public string method;

  public string SaveToString()
  {
    return JsonUtility.ToJson(this);
  }

  [System.Serializable]
  public class QuerySession : Request {
    public Params @params;
    
    public QuerySession(string method, Params param)
    {
      this.method = method;
      this.@params = param;
    }

    [System.Serializable]
    public class Params 
    {
      public string cortexToken;
      public Params(string cortexToken)
      {
        this.cortexToken = cortexToken;
      }
    }
  }

  [System.Serializable]
  public class AuthorizeSession : Request {
    public Params @params;

    public AuthorizeSession(string method, Params param)
    {
      this.method = method;
      this.@params = param;
    }

    [System.Serializable]
    public class Params
    {
      public string clientId;
      public string clientSecret;

      public Params(string clientId, string clientSecret)
      {
        this.clientId = clientId;
        this.clientSecret = clientSecret;
      }
    }
  }

  public class CreateSession : Request {
    public Params @params;
    
    public CreateSession(string method, Params param)
    {
      this.method = method;
      this.@params = param;
    }

    [System.Serializable]
    public class Params 
    {
      public string cortexToken;
      public string headset;
      public string status;
      public Params(string cortexToken, string headset, string status)
      {
        this.cortexToken = cortexToken;
        this.headset = headset;
        this.status = status;
      }
    }
  }

  public class StreamMentalCommand : Request {
    public Params @params;

    public StreamMentalCommand(string method, Params param)
    {
      this.method = method;
      this.@params = param;
    }

    [System.Serializable]
    public class Params {
      public string cortexToken;
      public string session;
      public List<string> streams;

      public Params(string cortexToken, string session, List<string> streams)
      {
        this.cortexToken = cortexToken;
        this.session = session;
        this.streams = streams;
      }
    }
  }
}

[System.Serializable]
public class Response {
  public string id;
  public string jsonrpc;
  [System.Serializable]
  public class ReceivedAuthorizationToken : Response {
    public Result result;

    [System.Serializable]
    public class Result {
      public string cortexToken;
    }
  }

  [System.Serializable]
  public class ReceivedSessionId : Response {
    public Result result;

    [System.Serializable]
    public class Result
    {
      public string id;
      public string status;
      public string appId;
      public Headset headset;
      public string license;
      public string owner;
      public List<PerformanceMetric> performanceMetrics;
      public List<object> recordIds;
      public bool recording;
      public string started;
      public string stopped;
      public List<object> streams;
    }

    [System.Serializable]
    public class Settings 
    {
      public int eegRate;
      public int eegRes;
      public int memsRate;
      public int memsRes;
      public string mode;
    }

    [System.Serializable]
    public class Headset 
    {
      public string connectedBy;
      public List<string> dfuTypes;
      public string dongle;
      public string firmware;
      public string id;
      public bool isDfuMode;
      public bool isVirtual;
      public List<string> motionSensors;
      public List<string> sensors;
      public Settings settings;
      public string status;
      public string virtualHeadsetId;
    }
    [System.Serializable]
    public class PerformanceMetric
    {
      public string apiName;
      public string displayName;
      public string name;
      public string shortDispalyName;
    }
  }
}

[System.Serializable]
public class MentalCommand {
  // com = ['commandType', magnitude between 0.0 - 1.0]
  public List<string> com;
  public string sid;
  public float time;
}
