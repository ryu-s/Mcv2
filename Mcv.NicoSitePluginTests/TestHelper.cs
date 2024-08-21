namespace Mcv.NicoSitePluginTests;

internal static class TestHelper
{
    public static string GetSampleData(string filename)
    {
        var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SampleData\" + filename);
        string sample = "";

        using (var sr = new System.IO.StreamReader(path))
        {
            sample = sr.ReadToEnd();
        }
        return sample;
    }
    public static byte[] GetSampleDataBytes(string filename)
    {
        var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SampleData\" + filename);
        return File.ReadAllBytes(path);
    }
}