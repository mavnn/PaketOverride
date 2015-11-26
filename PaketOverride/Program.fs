open System.IO

let refs (rootDir : DirectoryInfo) =
    rootDir.EnumerateFileSystemInfos("*.references", SearchOption.AllDirectories)
    |> Seq.map (fun fi -> fi.FullName |> Path.GetDirectoryName, fi)
    |> Map.ofSeq

let configs (rootDir : DirectoryInfo) =
    rootDir.EnumerateFileSystemInfos("*.config", SearchOption.AllDirectories)

let projs (rootDir : DirectoryInfo) =
    [
        rootDir.EnumerateFileSystemInfos("*.csproj", SearchOption.AllDirectories)
        rootDir.EnumerateFileSystemInfos("*.vbproj", SearchOption.AllDirectories)
        rootDir.EnumerateFileSystemInfos("*.fsproj", SearchOption.AllDirectories)
    ]
    |> Seq.concat
    |> Seq.map (fun fi -> fi.FullName |> Path.GetDirectoryName, fi)
    |> Map.ofSeq

let ensureRef signal (name : string) (refs : Map<string, FileSystemInfo>) (dir : string) (proj : FileSystemInfo) =
    let add (sw : StreamWriter) =
        sw.WriteLine()
        sw.Write name
        sw.Flush()
        sw.Close()
    if (File.ReadAllText proj.FullName).Contains signal then
        match refs |> Map.tryFind dir with
        | Some ref ->
            let current = File.ReadAllLines ref.FullName
            if not (Array.exists ((=) name) current) then
                printfn "Adding %s to %s" name ref.FullName
                use sw = File.AppendText ref.FullName
                add sw
        | None ->
            use sw = File.CreateText (Path.Combine(dir, "paket.references"))
            add sw

let root = DirectoryInfo(@"C:\PAS\Source")

let fix (signal, name) =
    projs root
    |> Map.iter (ensureRef signal name (refs root))

[
    "15below.DatasourceManager.Messages", "15below.DatasourceManager.Messages"
]
|> List.iter fix
//
//let sanitize refFile =
//    let lines = File.ReadAllLines refFile
//    lines
//    |> Seq.distinctBy (fun l -> l.ToLowerInvariant())
//    |> Seq.filter ((<>) "")
//    |> Seq.sort
//    |> fun ls -> File.WriteAllLines(refFile, ls)
//
//refs root
//|> Map.toSeq
//|> Seq.map (snd >> fun fi -> fi.FullName)
//|> Seq.iter sanitize

//open System.Xml.Linq
//let clearRedirects (configFile : string) =
//    try
//        printfn "Cleansing %s" configFile
//        let doc = XDocument.Load configFile
//        doc.Descendants(XName.Get("assemblyBinding", "urn:schemas-microsoft-com:asm.v1"))
//        |> Seq.toList
//        |> List.iter (fun el -> el.Remove())
//        doc.Save configFile
//        printfn "%s cleansed" configFile
//    with
//    | e -> printfn "Oh noes: %A" e
//    
//configs root
//|> Seq.map (fun c -> c.FullName)
//|> Seq.iter clearRedirects

printfn "Done!"

System.Console.ReadLine() |> ignore