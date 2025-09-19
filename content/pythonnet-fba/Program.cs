#:property TargetFramework=NET_TFM
#:package pythonnet@3.0.5

// Pythonnet requires DLR; Cannot use Native AOT.
#:property PublishAot=False

using Python.Runtime;

// You must set Runtime.PythonDLL property or PYTHONNET_PYDLL environment variable starting with version 3.0,
// otherwise you will receive BadPythonDllException (internal, derived from MissingMethodException) upon calling Initialize. Typical values are python38.dll (Windows), libpython3.8.dylib (Mac), libpython3.8.so (most other Unix-like operating systems).
/*
Runtime.PythonDLL = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    @$"Programs\Python\Python312\python312.dll");
*/
RuntimeData.FormatterType = typeof(NoopFormatter);

try
{
    PythonEngine.Initialize();

    using (_ = Py.GIL())
    {
        dynamic np = Py.Import("numpy");
        Console.WriteLine(np.cos(np.pi * 2));

        dynamic sin = np.sin;
        Console.WriteLine(sin(5));

        double c = (double)(np.cos(5) + sin(5));
        Console.WriteLine(c);

        dynamic a = np.array(new List<float> { 1, 2, 3 });
        Console.WriteLine(a.dtype);

        dynamic b = np.array(new List<float> { 6, 5, 4 }, dtype: np.int32);
        Console.WriteLine(b.dtype);

        Console.WriteLine(a * b);
    }
}
finally
{
    PythonEngine.Shutdown();
}
