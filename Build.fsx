// spell-checker: ignore batchmode nographics globbing
#load "CommandLine.fsx"
open Argu
open CommandLine
open Fake.Core
open System


let cipherKeyEnvName = "CIPHER_KEY"

type CommandLineArgument =
    | [<CustomCommandLine "--unity-exe">] UnityExe of path: string
    | [<CustomCommandLine "--cipher-key">] CipherKey of key: string
    | [<CustomCommandLine "--skip-activation">] SkipActivation
with
    interface IArgParserTemplate with
        override a.Usage =
            match a with
            | UnityExe _ -> $"Unity の実行ファイルへのパス。指定しない場合は OS ごとに推測されたパスを検索します。"
            | CipherKey _ -> $"暗号化されたライセンスファイルの複合キー。指定しない場合は環境変数 {cipherKeyEnvName} を使います。"
            | SkipActivation -> $"ライセンス認証をスキップします"

let args =
    ArgumentParser
        .Create(
            programName = __SOURCE_FILE__
        )
        .Parse(fsi.CommandLineArgs.[1..])

let findUnityExePath() =
    if Environment.isWindows
    then "C:/Program Files/Unity/Editor/Unity.exe"
    else "/opt/Unity/Editor/Unity"

let unityExePath =
    args.TryGetResult <@ UnityExe @>
    |> Option.defaultWith findUnityExePath

let cipherPath = "./Unity_v2019.x.ulf-cipher"
let licensePath = "./License.ulf"
let runUnity args = run unityExePath ["-quit"; "-batchmode"; "-nographics"; "-silent-crashes"; "-logFile"; yield! args]

let skipActivation = args.Contains <@ SkipActivation @>

if not skipActivation then
    let cipherKey =
        args.TryGetResult <@ CipherKey @>
        |> Option.defaultWith (fun _ ->
            Environment.environVarOrFail cipherKeyEnvName
        )

    printfn "ライセンスファイルを復号化してファイルに保存"
    run "openssl" ["aes-256-cbc"; "-d"; "-in"; cipherPath; "-k"; cipherKey; "-out"; licensePath; "-iter"; "100"]

    // 正常終了でも exitCode が 0 でない場合があるらしい
    printfn "Unity の認証"
    runUnity ["-manualLicenseFile"; licensePath]

printfn "ビルド"
runUnity ["-projectPath"; "./Project"; "-noUpm"; "-buildWindows64Player"; $"{Environment.CurrentDirectory}/Windows/windows.exe"]
