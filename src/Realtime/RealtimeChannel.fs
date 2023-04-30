namespace Realtime

open FSharp.Json
open Realtime.Common

type RealtimeChannel(topic: string, socket: IRealtimeClient) =
    let mutable bindings = Map<string, Binding list>[]
    
    interface IRealtimeChannel with
        member this.On (types: RealtimeListenTypes) (filter: ChannelFilter) =
            let typeLower = getRealtimeListenType types
            
            let binding = { Type = typeLower
                            Filter = filter }
            
            bindings <- bindings |> Map.change typeLower (fun k ->
                            match k with
                            | Some v -> Some (v @ [binding])
                            | None   -> Some [binding])
            this

        member this.Subscribe() =
            let postgresChanges =
                match bindings |> Map.tryFind (getRealtimeListenType RealtimeListenTypes.PostgresChanges) with
                | Some b ->
                    b
                    |> List.map (fun r -> r.Filter)
                    |> List.map (fun filter ->
                        { Event = getPostgresEventValue filter.Event
                          Schema = filter.Schema
                          Table = filter.Table } )
                | _      -> []
                
            let payload =
                { Config = { Broadcast =
                                Map<string, bool>[
                                    "ack", false
                                    "self", false
                                ]
                             Presence = Map<string, string>[ "key", "" ]
                             PostgresChanges = postgresChanges } }
                
            let message =
                { Topic = $"realtime:{topic}"
                  Event = "phx_join"
                  Ref = socket.Ref.ToString()
                  JoinRef = None
                  Payload = payload }
            
            printfn $"Subscribe message: {Json.serialize message}"
            socket.Connection.Websocket.Send(Json.serialize message)