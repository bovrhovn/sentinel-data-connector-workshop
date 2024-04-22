namespace DCW.Web.Helpers;

public static class MenuExtensions
{
    public static string GetActiveUrlCssClass(this string pageUrl, params string[] comparerUrl) =>
        comparerUrl.Contains(pageUrl) ? "nav-link actively" : "nav-link";
}