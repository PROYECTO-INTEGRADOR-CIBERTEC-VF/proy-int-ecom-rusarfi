using Microsoft.EntityFrameworkCore;
using RusarfiServer.Data;
using RusarfiServer.Dtos.Cart;
using RusarfiServer.Models;

namespace RusarfiServer.Service;

public sealed class CartService(AppDbContext db) : ICartService
{
    public async Task<ServiceResult<CartSummaryDto>> AddProductAsync(CartAddRequest request, CancellationToken cancellationToken)
    {
        if (request.UserId <= 0)
        {
            return ServiceResult<CartSummaryDto>.Fail("El usuario es obligatorio", 400);
        }

        if (request.ProductId <= 0)
        {
            return ServiceResult<CartSummaryDto>.Fail("El producto es obligatorio", 400);
        }

        if (request.Quantity <= 0)
        {
            return ServiceResult<CartSummaryDto>.Fail("La cantidad debe ser mayor a 0", 400);
        }

        var userExists = await db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            return ServiceResult<CartSummaryDto>.Fail("El usuario no existe", 404);
        }

        var product = await db.Products
            .SingleOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null || !product.IsActive)
        {
            return ServiceResult<CartSummaryDto>.Fail("El producto no existe o no está disponible", 404);
        }

        if (product.Stock <= 0)
        {
            return ServiceResult<CartSummaryDto>.Fail("No hay stock disponible para este producto", 409);
        }

        var cartItem = await db.CartItems
            .SingleOrDefaultAsync(c => c.UserId == request.UserId && c.ProductId == request.ProductId, cancellationToken);

        var finalQuantity = request.Quantity;

        if (cartItem is not null)
        {
            finalQuantity = cartItem.Quantity + request.Quantity;
        }

        if (finalQuantity > product.Stock)
        {
            return ServiceResult<CartSummaryDto>.Fail("No hay stock suficiente para la cantidad solicitada", 409);
        }

        if (cartItem is null)
        {
            cartItem = new CartItem
            {
                UserId = request.UserId,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            db.CartItems.Add(cartItem);
        }
        else
        {
            cartItem.Quantity = finalQuantity;
            cartItem.UpdatedAtUtc = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);

        var summary = await BuildCartSummaryAsync(request.UserId, cancellationToken);

        return ServiceResult<CartSummaryDto>.Ok("Producto agregado al carrito", summary, 200);
    }

    public async Task<ServiceResult<CartSummaryDto>> GetCartAsync(int userId, CancellationToken cancellationToken)
    {
        if (userId <= 0)
        {
            return ServiceResult<CartSummaryDto>.Fail("El usuario es obligatorio", 400);
        }

        var userExists = await db.Users.AnyAsync(u => u.Id == userId, cancellationToken);
        if (!userExists)
        {
            return ServiceResult<CartSummaryDto>.Fail("El usuario no existe", 404);
        }

        var summary = await BuildCartSummaryAsync(userId, cancellationToken);

        if (summary.Items.Count == 0)
        {
            return ServiceResult<CartSummaryDto>.Ok("No tienes productos en tu carrito", summary, 200);
        }

        return ServiceResult<CartSummaryDto>.Ok("Carrito obtenido correctamente", summary, 200);
    }

    public async Task<ServiceResult<CartSummaryDto>> UpdateQuantityAsync(CartUpdateRequest request, CancellationToken cancellationToken)
    {
        if (request.UserId <= 0)
        {
            return ServiceResult<CartSummaryDto>.Fail("El usuario es obligatorio", 400);
        }

        if (request.ProductId <= 0)
        {
            return ServiceResult<CartSummaryDto>.Fail("El producto es obligatorio", 400);
        }

        if (request.Quantity < 0)
        {
            return ServiceResult<CartSummaryDto>.Fail("La cantidad no puede ser negativa", 400);
        }

        var cartItem = await db.CartItems
            .SingleOrDefaultAsync(c => c.UserId == request.UserId && c.ProductId == request.ProductId, cancellationToken);

        if (cartItem is null)
        {
            return ServiceResult<CartSummaryDto>.Fail("El producto no está en el carrito", 404);
        }

        if (request.Quantity == 0)
        {
            db.CartItems.Remove(cartItem);
            await db.SaveChangesAsync(cancellationToken);

            var emptyOrUpdatedSummary = await BuildCartSummaryAsync(request.UserId, cancellationToken);

            if (emptyOrUpdatedSummary.Items.Count == 0)
            {
                return ServiceResult<CartSummaryDto>.Ok("No tienes productos en tu carrito", emptyOrUpdatedSummary, 200);
            }

            return ServiceResult<CartSummaryDto>.Ok("Carrito actualizado correctamente", emptyOrUpdatedSummary, 200);
        }

        var product = await db.Products
            .SingleOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null || !product.IsActive)
        {
            return ServiceResult<CartSummaryDto>.Fail("El producto no existe o no está disponible", 404);
        }

        if (request.Quantity > product.Stock)
        {
            return ServiceResult<CartSummaryDto>.Fail("No hay stock suficiente para la cantidad solicitada", 409);
        }

        cartItem.Quantity = request.Quantity;
        cartItem.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        var summary = await BuildCartSummaryAsync(request.UserId, cancellationToken);

        return ServiceResult<CartSummaryDto>.Ok("Carrito actualizado correctamente", summary, 200);
    }

    private async Task<CartSummaryDto> BuildCartSummaryAsync(int userId, CancellationToken cancellationToken)
    {
        var items = await db.CartItems
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .Join(
                db.Products.AsNoTracking(),
                cart => cart.ProductId,
                product => product.Id,
                (cart, product) => new CartItemDto
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ImageUrl = product.ImageUrl,
                    Quantity = cart.Quantity,
                    UnitPrice = product.Price,
                    Subtotal = product.Price * cart.Quantity
                })
            .OrderBy(i => i.ProductName)
            .ToListAsync(cancellationToken);

        return new CartSummaryDto
        {
            UserId = userId,
            Items = items,
            Total = items.Sum(i => i.Subtotal)
        };
    }
}