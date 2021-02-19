#r "nuget: Argu, 6.1.1"
#r "nuget: Fake.Core.Process, 5.20.3"
open Fake.Core
module Proc = CreateProcess


let run fileName args =
    Proc.fromRawCommand fileName args
    |> Proc.withStandardError (UseStream(true, System.Console.OpenStandardError()))
    |> Proc.withStandardOutput (UseStream(true, System.Console.OpenStandardOutput()))
    |> Proc.ensureExitCode
    |> Proc.run
    |> ignore
