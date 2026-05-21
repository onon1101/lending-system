namespace LendingSystem.Lending.Application.Media;

internal static class MediaStorageHelper
{
    public static string RewritePublicMediaHost(string url)
    {
        var builder = new UriBuilder(url)
        {
            Scheme = Uri.UriSchemeHttps,
            Host = "lending-minio.onon1101.org",
            Port = -1
        };
        return builder.Uri.ToString();
    }
}
