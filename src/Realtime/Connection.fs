namespace Realtime

open System
open System.Net.Http
open Websocket.Client

/// Contains CE for creating connection
[<AutoOpen>]
module Connection =
    /// Represents current client version
    let version = "0.0.1"

    /// Represents client info header with current version
    let clientInfo = ("X-Client-Info", $"realtime-client-v{version}")
    
    /// Represents base connection
    type RealtimeConnection = {
        // Url: string
        Headers: Map<string, string>
        Websocket: WebsocketClient
    }
    
    type RealtimeConnectionBuilder() =
        member _.Yield(url: string, headers: Map<string, string>) =
            let k, v = clientInfo
            let websocket = new WebsocketClient(Uri(url))
            let headersWithVersion = headers |> Map.add k v
            { Headers = headersWithVersion
              Websocket = websocket }

        [<CustomOperation("headers")>]
        member _.Headers(connection, headers) =
            { connection with Headers = headers }
            
        [<CustomOperation("websocket")>]
        member _.Headers(connection, websocket) =
            { connection with Websocket = websocket }

    let realtimeConnection = RealtimeConnectionBuilder()