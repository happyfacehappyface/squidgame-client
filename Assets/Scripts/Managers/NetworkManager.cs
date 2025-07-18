using UnityEngine;
using NativeWebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;
    private bool _isReady = false;

    private WebSocket websocket;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    private async void Start()
    {
        //Utils.Log("Start NetworkManager");
        //await ConnectToServer("ws://paintingchess.duckdns.org:18000");
        //await ConnectToServer("wss://hachess.duckdns.org:18000");
        


        #if UNITY_WEBGL && !UNITY_EDITOR
        await ConnectToServer("/ws");
        #else
        await ConnectToServer("ws://localhost:8082/ws");
        #endif

    }

    

    public async Task ConnectToServer(string uri)
    {
        websocket = new WebSocket(uri);

        websocket.OnOpen += () =>
        {
            Utils.Log("WebSocket Connected");
            _isReady = true;
            Utils.Log("NetworkManager is now ready");
        };

        websocket.OnError += (e) =>
        {
            Utils.LogError("WebSocket Error: " + e);
        };

        websocket.OnClose += (e) =>
        {
            Utils.Log("WebSocket Closed");
        };

        websocket.OnMessage += (bytes) =>
        {
            string message = Encoding.UTF8.GetString(bytes);
            Utils.Log("서버 응답: " + message);

            ResponsePacketData data = DeserializeResponse(message);
            print(data);

            if (_responseHandlers.TryGetValue(data.GetType(), out var handler))
            {
                handler(true, data);
            }
            else if (data is ResponsePacketData.Error error)
            {
                if (_responseSignalToType.TryGetValue(error.code, out var expectedType) &&
                    _responseHandlers.TryGetValue(expectedType, out var fallbackHandler))
                {
                    fallbackHandler(false, null!);
                }
                else
                {
                    throw new Exception("Unexpected Error Code");
                }
            }

            //OnServerMessageReceived?.Invoke(message);
        };

        await websocket.Connect();
    }

    public bool IsReady()
    {
        return _isReady;
    }

    // Callback
    //public event Action<string> OnServerMessageReceived;

    public async void SendMessageToServer(RequestPacketData data)
    {
#if SUPERSLOW
        await Task.Delay(3000);
#elif SLOW
        await Task.Delay(1000);
#endif

        string message = SerializeRequest(data);

        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.SendText(message);
            Utils.Log("Send Message To Server: " + message);
        }
        else
        {
            Utils.LogWarning("WebSocket is Closed.");
        }
    }



    private void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif



        
        if (_isReady)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SendMessageToServer(new RequestPacketData.Ping());
            }
        }
        
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null)
        {
            await websocket.Close();
        }
    }


    private static readonly Dictionary<Type, Action<bool, ResponsePacketData>> _responseHandlers =
    new()
    {

        {
            typeof(ResponsePacketData.Ping),
            (isSuccess, data) => {
                Utils.Log("Pong");
            }
        },

    };



    private static readonly Dictionary<int, Type> _requestSignalToType = new()
    {
        { 1, typeof(RequestPacketData.Ping) },
        
    };

    private static readonly Dictionary<int, Type> _responseSignalToType = new()
    {
        { 1, typeof(ResponsePacketData.Ping) },

        
    };

    public static Type GetRequestTypeFromSignal(int signal)
    {
        if (_requestSignalToType.TryGetValue(signal, out var type))
            return type;

        throw new ArgumentException($"Unknown request signal code: {signal}");
    }

    public static int GetRequestSignalFromType(Type type)
    {
        foreach (var kv in _requestSignalToType)
        {
            if (kv.Value == type)
                return kv.Key;
        }

        throw new ArgumentException($"Unknown request type: {type.Name}");
    }

    public static Type GetResponseTypeFromSignal(int signal)
    {
        if (_responseSignalToType.TryGetValue(signal, out var type))
            return type;

        throw new ArgumentException($"Unknown request signal code: {signal}");
    }

    public static int GetResponseSignalFromType(Type type)
    {
        foreach (var kv in _responseSignalToType)
        {
            if (kv.Value == type)
                return kv.Key;
        }

        throw new ArgumentException($"Unknown request type: {type.Name}");
    }


    public static RequestPacketData DeserializeRequest(string json)
    {
        var jo = JObject.Parse(json);

        int signal = jo["signal"]?.Value<int>()
            ?? throw new JsonSerializationException("Missing 'signal' field");

        var dataToken = jo["data"];
        if (dataToken == null)
            throw new JsonSerializationException("Missing 'data' field");

        var targetType = GetRequestTypeFromSignal(signal);
        var dataObj = (RequestPacketData)dataToken.ToObject(targetType)!;

        return dataObj;
    }

    public static string SerializeRequest(RequestPacketData data)
    {
        var obj = new
        {
            signal = GetRequestSignalFromType(data.GetType()),
            data = data
        };

        return JsonConvert.SerializeObject(obj, Formatting.Indented);
    }

    public bool IsSuccess(string json)
    {
        var jo = JObject.Parse(json);
        var code = jo["code"]?.Value<int>();
        return (code != null) && (code == 200);
    }
    
    public static ResponsePacketData DeserializeResponse(string json)
    {
        var jo = JObject.Parse(json);

        int signal = jo["signal"]?.Value<int>()
            ?? throw new JsonSerializationException("Missing 'signal' field");

        var dataToken = jo["data"];
        if (dataToken == null)
            throw new JsonSerializationException("Missing 'data' field");

        var code = jo["code"]?.Value<int>()
            ?? throw new JsonSerializationException("Missing 'code' field");

        //var errorCode = jo["errorCode"]?.Value<int>()
        //    ?? throw new JsonSerializationException("Missing 'errorCode' field");

        if (code != 0)
        {
            //return new ResponsePacketData.Error(signal, errorCode);
            return new ResponsePacketData.Error(signal);
        }

        var targetType = GetResponseTypeFromSignal(signal);
        var dataObj = (ResponsePacketData)dataToken.ToObject(targetType)!;

        return dataObj;
    }
    
    public static string SerializeResponse(ResponsePacketData data)
    {
        var obj = new
        {
            signal = GetResponseSignalFromType(data.GetType()),
            data = data
        };

        return JsonConvert.SerializeObject(obj, Formatting.Indented);
    }
}



public record RequestPacket(int signal, RequestPacketData data);
public record ResponsePacket(int signal, ResponsePacketData data);

public abstract record RequestPacketData
{
    public sealed record Ping() : RequestPacketData;
    
}

public abstract record ResponsePacketData
{
    public sealed record Error(int code) : ResponsePacketData;
    public sealed record Ping() : ResponsePacketData;

}



public sealed record RoomInfo(int roomID, string roomName, int playerCount, int maxPlayerCount, int fruitVariation, int fruitCount, int speed);





namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit {}
}