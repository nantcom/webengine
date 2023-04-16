namespace NC.WebEngine.Core;
public static class CookieKeys
{

    public const string LOGIN_BEFORE_LOGIN_URL = "x-ncwebengine-beforelogin-url";

    public const string LOGIN_TOKEN = "x-ncwebengine-token";

    public const string MEMBER_REFERER_USER_ID = "x-ncwebengine-referer-user-id";

    public const string LOGIN_SESSION_ID = "x-ncwebengine-sessionid";

    public const string GENERIC_SYSTEM_MESSAGE = "x-ncwebengine-system-message";

}

public static class ConfigKeys
{
    public const string AzureADB2C = "AzureAdB2C";
}

public static class QueryStringKeys
{
    public const string LOGIN_NEXT_PAGE = "nextpage";
    public const string LOGIN_NEXT_PAGE_ALT = "next";
}
