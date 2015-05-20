open System
open System.Collections.Generic
open System.Text

open Trinity
open Trinity.Extension
open Trinity.Storage

[<EntryPoint>]
let main argv = 
    TrinityConfig.CurrentRunningMode <- RunningMode.Embedded

    // Characters
    let mutable Rachel = Character("Rachel Green", 0uy, true)
    let mutable Monica = Character("Monica Geller", 0uy, true)
    let mutable Phoebe = Character("Phoebe Buffay", 0uy, true)
    let mutable Joey = Character("Joey Tribbiani", 1uy, false)
    let mutable Chandler = Character("Chandler Bing", 1uy, true)
    let mutable Ross = Character("Ross Geller", 1uy, true)

    // Performers
    let mutable Jennifer = new Performer("Jennifer Aniston", 43uy, List<int64>())
    let mutable Courteney = new Performer("Courteney Cox",  48uy, List<int64>())
    let mutable Lisa = new Performer("Lisa Kudrow",  49uy, List<int64>())
    let mutable Matt = new Performer("Matt Le Blanc",  45uy, List<int64>())
    let mutable Matthew = new Performer("Matthew Perry",  43uy, List<int64>())
    let mutable David = new Performer("David Schwimmer",  45uy, List<int64>())

    Rachel.Performer <- Jennifer.CellID
    Rachel.CellID |> Jennifer.Characters.Add 

    Monica.Performer <- Courteney.CellID
    Monica.CellID |> Courteney.Characters.Add 

    Phoebe.Performer <- Lisa.CellID
    Lisa.Characters.Add(Phoebe.CellID)

    Joey.Performer <- Matt.CellID
    Joey.CellID |> Matt.Characters.Add 

    Chandler.Performer <- Matthew.CellID
    Chandler.CellID |> Matthew.Characters.Add 

    Ross.Performer <- David.CellID
    Ross.CellID |> David.Characters.Add

    // Marriage relationship
    Monica.Spouse <- Chandler.CellID
    Chandler.Spouse <- Monica.CellID

    Rachel.Spouse <- Ross.CellID
    Ross.Spouse <- Rachel.CellID

    // Friendship
    let friend_ship = new Friendship(new List<int64>())
    let characters = [Rachel; Monica; Phoebe; Joey; Chandler; Ross]
    let performers = [Jennifer; Courteney; Lisa; Matt; Matthew; David]

    characters |> List.iter(fun c -> c.CellID |> friend_ship.friends.Add )
    performers |> List.iter(Global.LocalStorage.SavePerformer >> ignore)
    characters |> List.iter(Global.LocalStorage.SaveCharacter >> ignore)

    // Dump memory storage to disk for persistence
    Global.LocalStorage.SaveStorage() |> ignore

    let spouse_id = 
        use cm = Global.LocalStorage.UseCharacter Monica.CellID
        if cm.Married then cm.Spouse else -1L

    use cm = Global.LocalStorage.UseCharacter spouse_id
    cm.Name.ToString() |> printf "%s" 

    Console.ReadKey() |> ignore


    0 // return an integer exit code
