namespace Realtime

open FSharp.Json

module Common =
    type ChannelEvents =
        | Close
        | Error
        | Join
        | Reply
        | Leave
        | Heartbeat
        | AccessToken  
        | Broadcast
        | Presence
        | PostgresChanges

    type PostgresEvent =
        | All
        | Insert
        | Update
        | Delete
    
    type ChannelFilter = {
        Event: PostgresEvent
        Schema: string option
        Table: string option
        // [<JsonField("filter")>]
        // Filter: string option
    }
    
    type PostgresRequestFilter = {
        [<JsonField("event")>]
        Event: string
        [<JsonField("schema")>]
        Schema: string option
        [<JsonField("table")>]
        Table: string option
        // [<JsonField("filter")>]
        // Filter: string option
    }
    
    type Binding = {
        [<JsonField("type")>]
        Type: string
        [<JsonField("filter")>]
        Filter: ChannelFilter
    }
    
    type RealtimeListenTypes =
        | PostgresChanges
        | Broadcast
        | Presence
    
    type Config = {
        [<JsonField("broadcast")>]
        Broadcast: Map<string, bool>
        [<JsonField("presence")>]
        Presence: Map<string, string>
        [<JsonField("postgres_changes")>]
        PostgresChanges: PostgresRequestFilter list
    }
    
    type Payload = {
        [<JsonField("config")>]
        Config: Config
    }
            
    type Message = {
        [<JsonField("topic")>]
        Topic: string
        [<JsonField("event")>]
        Event: string
        [<JsonField("payload")>]
        Payload: Payload
        [<JsonField("ref")>]
        Ref: string
        [<JsonField("join_ref")>]
        JoinRef: string option
    }
    
    let getPostgresEventValue (event: PostgresEvent): string =
        match event with
        | All -> "*"
        | e   -> e.ToString().ToUpper()
    
    let getEmptyPayload (): Payload =
        { Config = { Broadcast = Map[]
                     Presence = Map[]
                     PostgresChanges = [] } }
    
    let getRealtimeListenType (types: RealtimeListenTypes): string =
        match types with
        | RealtimeListenTypes.PostgresChanges -> "postgres_changes"
        | other                               -> other.ToString().ToLower()
    
    let getChannelEvent (event: ChannelEvents): string =
        match event with
        | AccessToken                   -> "access_token"
        | ChannelEvents.PostgresChanges -> "postgres_changes"
        | ChannelEvents.Broadcast       -> "broadcast"
        | ChannelEvents.Presence        -> "presence"
        | Heartbeat                     -> "heartbeat"
        | e                             -> $"phx_{e.ToString()}"