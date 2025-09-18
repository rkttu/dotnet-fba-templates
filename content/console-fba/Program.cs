#!/usr/bin/env dotnet

#if (Framework != "")
#:property TargetFramework=NET_TFM_PARAM
#endif

// You can use AOT compilation by changing PublishAot to True.
// Please note that AOT compilation may break some functionalities like reflection.
#if (EnableAot)
#:property PublishAot=True
#else
#:property PublishAot=False
#endif

Console.WriteLine("Hello, World!");
