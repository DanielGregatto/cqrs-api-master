using System;

namespace Domain.DTO.Responses
{
    public record ProductDto(
        Guid Id,
        string Name,
        string Description,
        string Slug,
        decimal Price,
        string Sku,
        int Stock,
        string ImageFileName,
        bool Active,
        DateTime CreatedAt)
    {
        public static ProductDto FromEntity(Product product)
        {
            if (product == null) return null;

            return new ProductDto(
                product.Id,
                product.Name ?? "",
                product.Description ?? "",
                product.Slug ?? "",
                product.Price,
                product.Sku ?? "",
                product.Stock,
                product.ImageFileName ?? "",
                product.Active,
                product.CreatedAt
            );
        }
    }
}
