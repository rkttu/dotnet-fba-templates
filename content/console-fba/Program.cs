#!/usr/bin/env dotnet

#:property TargetFramework=NET_TFM

// You can use AOT compilation by changing PublishAot to True.
// Please note that AOT compilation may break some functionalities like reflection.
#if (EnableAot)
#:property PublishAot=True
#else
#:property PublishAot=False
#endif

Console.WriteLine("Hello, World!");
