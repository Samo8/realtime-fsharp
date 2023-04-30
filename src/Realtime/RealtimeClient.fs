namespace Realtime

open System
open System.Net.WebSockets
open System.Reactive
open System.Threading
open System.Threading.Channels
open FSharp.Json
open Realtime.Common
open Websocket.Client

type RealtimeClient(connection: RealtimeConnection) =
    let mutable ref = 0
    let mutable channels: IRealtimeChannel list = []
    let mutable isConnected: bool = false
    let client = connection.Websocket
    
    let getRef (): string =
        let nextRef = ref + 1
        if (nextRef < 0) then
            ref <- 0
        else ref <- nextRef
        
        ref.ToString()
    
    let heartBeat () =
        let message =
            { Topic = "phoenix"
              Event = getChannelEvent Heartbeat
              Payload = getEmptyPayload ()
              Ref = getRef ()
              JoinRef = None }
        client.Send (Json.serialize message)
    
    interface IRealtimeClient with
        member this.Ref
            with get() = ref
            
        member this.Connection
            with get() = connection
            
        member this.Connect () =
            printfn "start"
            client.ReconnectionHappened.Subscribe(fun info ->
                // let requst = """{"topic":"realtime:any","event":"access_token","payload":{"access_token":null},"ref":"5","join_ref":"3"}"""
                // client.Send("""{"topic":"realtime:any","event":"access_token","payload":{"access_token":null},"ref":"5","join_ref":"3"}""")
                printfn $"Reconnection happened, type: {info.Type}"
            ) |> ignore
            
            client.DisconnectionHappened.Subscribe(
                fun disconnectionInfo -> printfn $"{disconnectionInfo.CloseStatusDescription}"
            ) |> ignore
            
            client.MessageReceived.Subscribe(fun msg -> printfn $"Message received: {msg}") |> ignore
            try
                client.StartOrFail() |> Async.AwaitTask |> ignore
                isConnected <- true
                
                let interval = TimeSpan.FromSeconds 5.0
                let timer = new Timer(
                    callback = (fun _ -> heartBeat ()),
                    state = null,
                    dueTime = interval,
                    period = interval
                )

                timer.Change(dueTime = TimeSpan.Zero, period = interval) |> ignore
            with e ->
                printfn $"{e}"
            
            // let payload = """{"topic":"realtime:any","event":"phx_join","payload":{"config":{"broadcast":{"ack":false,"self":false},"presence":{"key":""},"postgres_changes":[{"event":"*","schema":"public","table":"test"}]}},"ref":"1","join_ref":"1"}"""
            
            // client.Send(payload)
            printfn "end"

        member this.Channel (name: string): IRealtimeChannel = 
            let channel = RealtimeChannel(name, this)
            channels <- channels |> List.append [channel]
            
            channel

        member this.Dispose () =
            client.Dispose()