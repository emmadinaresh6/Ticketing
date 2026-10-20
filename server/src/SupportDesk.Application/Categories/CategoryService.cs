using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstractions;
using SupportDesk.Application.Common;
using SupportDesk.Domain.Entities;

namespace SupportDesk.Application.Categories;

public record CategoryDto(Guid Id, string Name, bool HasRouting);

public record AddCategoryRequest([Required, MaxLength(100)] string Name);

public record AddCategoryResult(CategoryDto Category, bool Created);

public class CategoryService(IApplicationDbContext db, ICurrentUser currentUser, TimeProvider clock)
{
    public async Task<IReadOnlyList<CategoryDto>> ListAsync(CancellationToken ct) =>
        await db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.DefaultGroupId != null))
            .ToListAsync(ct);

    // Anyone can add a category. A duplicate (by normalized name) returns the existing one.
    public async Task<AddCategoryResult> AddAsync(AddCategoryRequest request, CancellationToken ct)
    {
        var name = string.Join(' ', request.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        var normalized = Category.Normalize(name);
        if (normalized.Length == 0)
        {
            throw new AppValidationException("Category name must contain letters or numbers.");
        }

        var existing = await FindAsync(normalized, ct);
        if (existing is not null)
        {
            return new AddCategoryResult(existing, Created: false);
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            NormalizedName = normalized,
            CreatedById = currentUser.UserId,
            CreatedAt = clock.GetUtcNow()
        };
        db.Categories.Add(category);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Someone added the same name at the same moment; the unique index caught it.
            db.Categories.Entry(category).State = EntityState.Detached;
            var winner = await FindAsync(normalized, ct);
            if (winner is null)
            {
                throw;
            }
            return new AddCategoryResult(winner, Created: false);
        }

        return new AddCategoryResult(new CategoryDto(category.Id, category.Name, HasRouting: false), Created: true);
    }

    private Task<CategoryDto?> FindAsync(string normalized, CancellationToken ct) =>
        db.Categories
            .Where(c => c.NormalizedName == normalized)
            .Select(c => new CategoryDto(c.Id, c.Name, c.DefaultGroupId != null))
            .FirstOrDefaultAsync(ct);
}
