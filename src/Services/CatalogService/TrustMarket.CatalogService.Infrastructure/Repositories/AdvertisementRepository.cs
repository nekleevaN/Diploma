using Microsoft.EntityFrameworkCore;
using TrustMarket.CatalogService.Application.Abstractions;
using TrustMarket.CatalogService.Domain.Entities;
using TrustMarket.CatalogService.Infrastructure.Persistence;

namespace TrustMarket.CatalogService.Infrastructure.Repositories;

public class AdvertisementRepository : IAdvertisementRepository
{
    private readonly CatalogDbContext _context;

    public AdvertisementRepository(CatalogDbContext context) => _context = context;

    public Task<Advertisement?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Advertisements.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<(List<Advertisement> Items, int Total)> GetPagedAsync(
        string? category, string? search, int page, int pageSize, CancellationToken ct = default,
        string? categorySub = null, string? categoryItem = null,
        string? condition = null, string? brand = null,
        decimal? priceMin = null, decimal? priceMax = null,
        string? size = null, string? color = null,
        string? sortBy = null)
    {
        var query = _context.Advertisements
            .Where(a => a.Status == AdvertisementStatus.Active)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(a => a.Category == category);

        if (!string.IsNullOrWhiteSpace(categorySub))
            query = query.Where(a => a.CategorySub == categorySub);

        if (!string.IsNullOrWhiteSpace(categoryItem))
            query = query.Where(a => a.CategoryItem == categoryItem);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = "%" + EscapeLike(search.Trim()) + "%";
            query = query.Where(a =>
                EF.Functions.ILike(a.Title, term) ||
                EF.Functions.ILike(a.Description, term) ||
                (a.Brand != null && EF.Functions.ILike(a.Brand, term)));
        }

        if (!string.IsNullOrWhiteSpace(condition))
        {
            var conditions = condition.Split(',', StringSplitOptions.RemoveEmptyEntries);
            query = query.Where(a => a.Condition != null && conditions.Contains(a.Condition));
        }

        if (!string.IsNullOrWhiteSpace(brand))
            query = query.Where(a => a.Brand != null && EF.Functions.ILike(a.Brand, $"%{brand}%"));

        if (priceMin.HasValue)
            query = query.Where(a => a.Price >= priceMin.Value);

        if (priceMax.HasValue)
            query = query.Where(a => a.Price <= priceMax.Value);

        if (!string.IsNullOrWhiteSpace(size))
        {
            var sizes = size.Split(',', StringSplitOptions.RemoveEmptyEntries);
            query = query.Where(a => a.Size != null && sizes.Contains(a.Size));
        }

        if (!string.IsNullOrWhiteSpace(color))
        {
            var colors = color.Split(',', StringSplitOptions.RemoveEmptyEntries);
            query = query.Where(a => a.Color != null && colors.Contains(a.Color));
        }

        var total = await query.CountAsync(ct);

        query = sortBy switch
        {
            "price_asc"  => query.OrderBy(a => a.Price),
            "price_desc" => query.OrderByDescending(a => a.Price),
            _            => query.OrderByDescending(a => a.CreatedAt)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task AddAsync(Advertisement advertisement, CancellationToken ct = default)
        => await _context.Advertisements.AddAsync(advertisement, ct);

    public void Update(Advertisement advertisement)
        => _context.Advertisements.Update(advertisement);

    public async Task UpdateSellerNameAsync(Guid sellerId, string displayName, CancellationToken ct = default)
    {
        await _context.Advertisements
            .Where(a => a.SellerId == sellerId)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.SellerName, displayName), ct);
    }

    public Task<string?> GetSellerSubMerchantIdAsync(Guid sellerId, CancellationToken ct = default)
        => _context.Advertisements
            .Where(a => a.SellerId == sellerId && a.SellerSubMerchantId != null)
            .Select(a => a.SellerSubMerchantId)
            .FirstOrDefaultAsync(ct);

    public async Task UpdateSellerSubMerchantIdAsync(Guid sellerId, string? subMerchantId, CancellationToken ct = default)
    {
        await _context.Advertisements
            .Where(a => a.SellerId == sellerId)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.SellerSubMerchantId, subMerchantId), ct);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);

    private static string EscapeLike(string s)
        => s.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
