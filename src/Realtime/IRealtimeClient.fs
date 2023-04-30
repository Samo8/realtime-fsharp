namespace Realtime

type IRealtimeClient =
    abstract member Connect : unit -> unit
    abstract member Channel : string -> IRealtimeChannel
    abstract member Dispose : unit -> unit
    abstract member Connection : RealtimeConnection
    abstract member Ref: int