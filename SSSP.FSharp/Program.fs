open System
open System.Linq
open System.Collections.Generic
open System.Text

open Trinity
open Trinity.Extension
open Trinity.Storage
open Trinity.Network.Messaging


[<AutoOpen>]
module ParameterHelpers = 

    let (|Start|Generate|Compute|Output|Unknown|) (args : string array) = 
        if args.Length >= 1 && args.[0].StartsWith("-s") then Start
        elif args.Length >= 2 && args.[0].StartsWith("-g") then Generate
        elif args.Length >= 2 && args.[0].StartsWith("-c") then Compute
        elif args.Length >= 2 && args.[0].StartsWith("-q") then  Output
        else Unknown

module SSSPServer = 
    let private sendDistanceUpdatingMessage (cell : SSSPCell_Accessor) = 
        let sorter = new MessageSorter(List<int64>(cell.neighbors)) 
        [0 .. Global.ServerCount - 1] |> List.iter(fun i ->
            let msg = new DistanceUpdatingMessageWriter(cell.CellID.Value, cell.distance, sorter.GetCellRecipientList i)
            Global.CloudStorage.DistanceUpdatingToSSSPServer(i , msg)
        )
        

    let distanceUpdatingHandler (request :DistanceUpdatingMessageReader) = 
        request.recipients |> Seq.iter(fun cellId-> 
            use cell = cellId |> Global.LocalStorage.UseSSSPCell 
            if cell.distance > request.distance + 1 then
                cell.distance <- request.distance + 1
                cell.parent <- request.senderId
                printf "%d " cell.distance
                sendDistanceUpdatingMessage cell
                
        )

    let startSSSPHandler (request : StartSSSPMessageReader) = 
        if Global.CloudStorage.IsLocalCell(request.root) then
            use cell = request.root |> Global.LocalStorage.UseSSSPCell
            cell.distance <- 0
            cell.parent <- -1L
            sendDistanceUpdatingMessage cell

    let create () =
        { new SSSPServerBase() with
            member this.DistanceUpdatingHandler request = distanceUpdatingHandler request
            member this.StartSSSPHandler request = startSSSPHandler request }


[<EntryPoint>]
let main args =
    TrinityConfig.AddServer(Trinity.Network.ServerInfo("127.0.0.1", 5304, Global.MyAssemblyPath, Trinity.Diagnostics.LogLevel.Error))

    match args with
    | Start -> let server = SSSPServer.create ()
               server.Start true
    | Compute -> TrinityConfig.CurrentRunningMode <- RunningMode.Client
                 [0 .. Global.ServerCount - 1] |> List.iter(fun i ->
                     use msg = args.[1].Trim() |> Int64.Parse |> fun n -> new StartSSSPMessageWriter(n)
                     Global.CloudStorage.StartSSSPToSSSPServer(i, msg);
                 )
    | Output -> TrinityConfig.CurrentRunningMode <- RunningMode.Client
                let rec load (cellId : int64) = 
                    let cell = cellId |> Global.CloudStorage.LoadSSSPCell
                    printfn "Current vertex is %d, the distance to the source vertex is %d." cell.CellID cell.distance
                    if cell.distance > 0 then load cell.parent 

                args.[1].Trim() |> Int64.Parse |> load
    | Generate -> TrinityConfig.CurrentRunningMode <- RunningMode.Client
                  let random = Random()
                  let count = args.[1].Trim() |> Int32.Parse
                  [0L .. (count - 1) |> int64] |> List.iter (fun i -> 
                      let neighbors = HashSet<Int64>()
                      [0 .. 9] |> List.iter (fun j ->
                          let neighor = random.Next(0, count) |> int64 
                          if neighor <> i then neighor |> neighbors.Add |> ignore
                      )
                      Global.CloudStorage.SaveSSSPCell(i, Int32.MaxValue, -1L, neighbors.ToList()) |> ignore
                  )
    | Unknown -> printfn "Bad parameters"
        
        
        


        
    
    0 // return an integer exit code
