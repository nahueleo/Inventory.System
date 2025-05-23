namespace Inventory.Domain.Constants;

public enum RoutingKey
{
    ProductCreated,
    ProductUpdated,
    ProductDeleted
} 

public static class RoutingKeys
{
    public const string ProductCreated = "product.created";
    public const string ProductUpdated = "product.updated";
    public const string ProductDeleted = "product.deleted";

    public static string ToRoutingKeyString(this RoutingKey key)
    {
        return key switch
        {
            RoutingKey.ProductCreated => ProductCreated,
            RoutingKey.ProductUpdated => ProductUpdated,
            RoutingKey.ProductDeleted => ProductDeleted,
            _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
        };
    }

    public static RoutingKey ToRoutingKeyEnum(this string key)
    {
        return key switch
        {
            ProductCreated => RoutingKey.ProductCreated,
            ProductUpdated => RoutingKey.ProductUpdated,
            ProductDeleted => RoutingKey.ProductDeleted,
            _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
        };
    }
} 