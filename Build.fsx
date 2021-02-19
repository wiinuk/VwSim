// spell-checker: ignore batchmode nographics globbing
#load "CommandLine.fsx"
open Argu
open CommandLine
open Fake.Core


let defaultUnityExePath =
    if Environment.isWindows
    then "C:/Program Files/Unity/Editor/Unity.exe"
    else "/opt/Unity/Editor/Unity"

let cipherKeyEnvName = "CIPHER_KEY"

// fsharplint:disable UnionCasesNames
type CommandLineArgument =
    | Unity_Exe of path: string
    | Cipher_Key of key: string
    | Skip_Activation
// fsharplint:enable
with
    interface IArgParserTemplate with
        override a.Usage =
            match a with
            | Unity_Exe _ -> $"Unity の実行ファイルへのパス。指定しない場合、この OS では {defaultUnityExePath} を使います。"
            | Cipher_Key _ -> $"暗号化されたライセンスファイルの複合キー。指定しない場合は環境変数 {cipherKeyEnvName} を使います。"
            | Skip_Activation -> $"ライセンス認証をスキップします"

let args =
    ArgumentParser
        .Create(
            programName = __SOURCE_FILE__
        )
        .Parse(fsi.CommandLineArgs.[1..])

let unityExePath =
    args.TryGetResult <@ Unity_Exe @>
    |> Option.defaultValue defaultUnityExePath

let runUnity args = run unityExePath ["-quit"; "-batchmode"; "-nographics"; "-silent-crashes"; "-logFile"; yield! args]

let skipActivation = args.Contains <@ Skip_Activation @>
if not skipActivation then
    let cipherPath = "./Unity_v2019.x.ulf-cipher"
    let licensePath = "./License.ulf"

    let cipherKey =
        args.TryGetResult <@ Cipher_Key @>
        |> Option.defaultWith (fun _ ->
            Environment.environVarOrFail cipherKeyEnvName
        )

    printfn "ライセンスファイルを復号化してファイルに保存"
    run "openssl" ["aes-256-cbc"; "-d"; "-in"; cipherPath; "-k"; cipherKey; "-out"; licensePath; "-iter"; "100"]

    printfn "Unity の認証"
    try runUnity ["-manualLicenseFile"; licensePath] with e -> eprintfn $"{e}"

printfn "ビルド"
try runUnity ["-projectPath"; "./Project"; "-executeMethod"; "Editor.BuildScript.Build"] with e -> eprintfn $"{e}"

