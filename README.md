# Matrix-Signup
Basic .NET program for creating Matrix.org accounts. Supports proxies and captcha solving.
Replicates browser acivity using pure requests with `System.Net.Http.HttpClient` in order to bypass any filters and keep a decent performance.

This is a rewrite of a previous script I made with Python3.9

### Requirements
It requires any version of `Newtonsoft.Json` for JSON data parsing.

### Example code

This is an example of how to create a Matrix account using RiotManager class.
It is required at least a 2captcha key for captcha solving. Proxy is optional.

```csharp
using System;
 
class Program
{
        static async System.Threading.Tasks.Task Main(string[] args)
       {
           string home_server = "https://homeserver.example";
           string captchakey = "2CAPTCHA-API-KEY";
           string proxy_str = "user:pass@host:port";

           RiotManager manager = new RiotManager(home_server, captchakey,
                                                 use_proxy: true, use_auth: true, 
                                                 proxy_string: proxy_str);
           string token = await manager.CreateRiotAccount("username", "password");
           Console.WriteLine(token);
       }
}
```

The expected result should look like this:

```console
@username:homeserver.example|SoMeAuThOrIzAtIoN_ToKeN
```
