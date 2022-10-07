using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;

public enum State
{
  Inactive,
  AuthorizationTokenReceived,
  Activated,
  Error,
}

public class CortexClient : MonoBehaviour {
  WebSocket websocket;
  [SerializeField] string clientId = "";
  [SerializeField] string clientSecret = "";
  [SerializeField] string headset = "";
  [SerializeField] string cortexServerUrl = "wss://localhost:6868";
  string cortexToken = "";
  string sessionId = "";
  int debit;
  bool receivedCortexToken;
  State sessionState = State.Inactive;
  public string command = "";
  public float magnitude = 0.0f;
  [SerializeField] PhysicsPropertyManager physicsManager;

  async void Start()
  {
    websocket = new WebSocket(this.cortexServerUrl);

    websocket.OnOpen += async () => 
    {
      if (!HasValidConfig())
      {
        Debug.LogError("Missing credentials: Please ensure you have the correct configuration.");
        
        return;
      }

      string message = new Request.AuthorizeSession(
        RequestMethods.AuthorizeSession,
        new Request.AuthorizeSession.Params(
          this.clientId,
          this.clientSecret
        )
      ).SaveToString();

       Debug.Log("[Websocket.OnStart] Constructed Message: " + message);

      await websocket.SendText(message);
    };

    websocket.OnError += (error) =>
    {
      Debug.LogError(
        "Connection Error! Please double check your URL and ensure the cortex API is running"
        + "URL: " + this.cortexServerUrl + " " 
        + "Error: " + error
      );

      this.sessionState = State.Error;
    };

    websocket.OnClose += (e) =>
    {
      Debug.Log("Connection closed!");
    };

    websocket.OnMessage += async (bytes) => 
    {
      var response = System.Text.Encoding.UTF8.GetString(bytes);
      if (!(this.sessionState == State.Activated))
      {
        Debug.Log("[Websocket.OnMessage] Recieved raw input" + response);
      }

      if (this.sessionState == State.Inactive)
      {
        this.cortexToken = this.cortexToken = JsonUtility.FromJson<Response.ReceivedAuthorizationToken>(response)
          .result
          .cortexToken;

        string message = new Request.CreateSession(
          RequestMethods.CreateSession,
          new Request.CreateSession.Params(
            this.cortexToken,
            this.headset,
            "active"
          )
        ).SaveToString();

        Debug.Log("[Websocket.OnMessage] Constructed Message: " + message);

        await websocket.SendText(message);
        
        this.sessionState = State.AuthorizationTokenReceived;

        return;
      }

      if (this.sessionState == State.AuthorizationTokenReceived)
      {
        this.sessionId = JsonUtility.FromJson<Response.ReceivedSessionId>(response)
          .result
          .id;
        Debug.Log("SESSION ID " + sessionId);

        string message = new Request.StreamMentalCommand(
          RequestMethods.StreamMentalCommand,
          new Request.StreamMentalCommand.Params(
            this.cortexToken,
            this.sessionId,
            new List<string> { "com" }
          )
        ).SaveToString();
        
        Debug.Log("[Websocket.OnMessage] Constructed Message: " + message);

        await websocket.SendText(message);

        this.sessionState = State.Activated;

        return;
      }

      if (this.sessionState == State.Activated)
      {
        MentalCommand command = JsonUtility.FromJson<MentalCommand>(response);

        string commandType = command.com[0];
        string commandMagnitude = command.com[1];

        this.command = commandType;
        this.magnitude = float.Parse(commandMagnitude, System.Globalization.CultureInfo.InvariantCulture);
        
        Debug.Log("[Websocket.OnMessage] Received Mental Command. Type " + commandType + " . Strength: " + this.magnitude);

        if (this.command != "neutral" && this.magnitude > 0) 
        {
          if (!physicsManager.mentalCommandActivated)
          {
            physicsManager.mentalCommandActivated = true;
          }
        }
        else {
          if (physicsManager.mentalCommandActivated)
          {
            physicsManager.mentalCommandActivated = false;
          }
        }
      }
    };
    
    await websocket.Connect();
  }

  void Update()
  {
    websocket.DispatchMessageQueue();
  }

  bool HasValidConfig()
  {
    bool clientIdExists = this.clientId.Length > 0;
    bool clientSecretExists = this.clientSecret.Length > 0;
    bool headsetExists = this.headset.Length > 0;

    return clientIdExists && clientSecretExists && headsetExists;
  }

  private async void OnApplicationQuit()
  {
    await websocket.Close();
  }

}
