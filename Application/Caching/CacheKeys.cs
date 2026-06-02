namespace Application.Caching;

public static class CacheKeys
{
    public static string CustomerById(int id) => $"customers:id:{id}";
    public static string OrderById(int id) => $"orders:id:{id}";
}