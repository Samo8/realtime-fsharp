namespace Realtime

open Realtime.Common

type IRealtimeChannel =
    abstract member On : RealtimeListenTypes -> ChannelFilter -> IRealtimeChannel
    abstract member Subscribe : unit -> unit