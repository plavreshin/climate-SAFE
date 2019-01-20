#r @"packages/build/FAKE/tools/FakeLib.dll"

open System

open Fake

let serverPath = "./src/Server" |> FullName
let clientPath = "./src/Client" |> FullName
let deployDir = "./deploy" |> FullName

let platformTool tool winTool =
  let tool = if isUnix then tool else winTool
  tool
  |> ProcessHelper.tryFindFileOnPath
  |> function Some t -> t | _ -> failwithf "%s not found" tool

let nodeTool = platformTool "node" "node.exe"

type JsPackageManager = 
  | NPM
  | YARN
  member this.Tool =
    match this with
    | NPM -> platformTool "npm" "npm.cmd"
    | YARN -> platformTool "yarn" "yarn.cmd"
   member this.ArgsInstall =
    match this with
    | NPM -> "install"
    | YARN -> "install --frozen-lockfile"

let jsPackageManager = 
  match getBuildParam "jsPackageManager" with
  | "npm" -> NPM
  | "yarn" | _ -> YARN

let mutable dotnetCli = "dotnet"

let run cmd args workingDir =
  let result =
    ExecProcess (fun info ->
      info.FileName <- cmd
      info.WorkingDirectory <- workingDir
      info.Arguments <- args) TimeSpan.MaxValue
  if result <> 0 then failwithf "'%s %s' failed" cmd args

Target "Clean" (fun _ -> 
  CleanDirs [deployDir]
)

Target "InstallClient" (fun _ ->
  printfn "Node version:"
  run nodeTool "--version" __SOURCE_DIRECTORY__
  run jsPackageManager.Tool jsPackageManager.ArgsInstall  __SOURCE_DIRECTORY__
  run dotnetCli "restore" clientPath
)

Target "Run" (fun () ->
  let server = async { run dotnetCli "watch run" serverPath }
  let client = async { run dotnetCli "fable webpack-dev-server" clientPath }
  let browser = async {
    Threading.Thread.Sleep 5000
    Diagnostics.Process.Start "http://localhost:8080" |> ignore
  }

  [ server; client; browser]
  |> Async.Parallel
  |> Async.RunSynchronously
  |> ignore
)

"Clean"
  ==> "InstallClient"
  ==> "Run"

RunTargetOrDefault "Run"