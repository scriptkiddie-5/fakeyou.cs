# fakeyou.cs

## Features
- Authentication
- TTS functionality

## Requirements
- [JamesNK/Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)

## Example Usage
```cs
// See https://aka.ms/new-console-template for more information

FakeYou.Client client = new FakeYou.Client();
client.Login("<username>", "<password>");

FakeYou.TTSModel? ericCartman = client.tts.GetModelByToken("TM:5dr00y37a5b2");
if (ericCartman != null)
{
    Console.WriteLine(ericCartman.title);
    FakeYou.TTSResult? result = ericCartman.Inference("Kenny where did you hide my cookies?");
    if (result != null)
        Console.WriteLine(result.url);
}

client.Logout();
```
