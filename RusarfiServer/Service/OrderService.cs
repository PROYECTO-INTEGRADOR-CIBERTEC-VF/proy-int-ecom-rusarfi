using Microsoft.EntityFrameworkCore;
using RusarfiServer.Data;
using RusarfiServer.Dtos.Orders;
using RusarfiServer.Models;

namespace RusarfiServer.Service;

public sealed class OrderService(AppDbContext db) : IOrderService
{
    public async Task<ServiceResult<ConfirmOrderResponse>> ConfirmOrderAsync(ConfirmOrderRequest request, CancellationToken cancellationToken)
    {
        if (request.UserId <= 0)
        {
            return ServiceResult<ConfirmOrderResponse>.Fail("El usuario es obligatorio", 400);
        }

        var userExists = await db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            return ServiceResult<ConfirmOrderResponse>.Fail("El usuario no existe", 404);
        }

        var cartItems = await db.CartItems
            .Where(c => c.UserId == request.UserId)
            .Join(
                db.Products,
                cart => cart.ProductId,
                product => product.Id,
                (cart, product) => new
                {
                    CartItem = cart,
                    Product = product
                })
            .OrderBy(x => x.Product.Name)
            .ToListAsync(cancellationToken);

        if (cartItems.Count == 0)
        {
            return ServiceResult<ConfirmOrderResponse>.Fail("No tienes productos en tu carrito", 400);
        }

        foreach (var item in cartItems)
        {
            if (!item.Product.IsActive)
            {
                return ServiceResult<ConfirmOrderResponse>.Fail($"El producto '{item.Product.Name}' no está disponible", 409);
            }

            if (item.Product.Stock < item.CartItem.Quantity)
            {
                return ServiceResult<ConfirmOrderResponse>.Fail($"No hay stock suficiente para '{item.Product.Name}'", 409);
            }
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var total = cartItems.Sum(x => x.Product.Price * x.CartItem.Quantity);

            var order = new Order
            {
                UserId = request.UserId,
                Total = total,
                Status = "Pendiente",
                CreatedAtUtc = DateTime.UtcNow
            };

            db.Orders.Add(order);
            await db.SaveChangesAsync(cancellationToken);

            var orderItems = cartItems.Select(x => new OrderItem
            {
                OrderId = order.Id,
                ProductId = x.Product.Id,
                ProductName = x.Product.Name,
                ImageUrl = x.Product.ImageUrl,
                Quantity = x.CartItem.Quantity,
                UnitPrice = x.Product.Price,
                Subtotal = x.Product.Price * x.CartItem.Quantity
            }).ToList();

            db.OrderItems.AddRange(orderItems);

            foreach (var item in cartItems)
            {
                item.Product.Stock -= item.CartItem.Quantity;
            }

            db.CartItems.RemoveRange(cartItems.Select(x => x.CartItem));

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var response = new ConfirmOrderResponse
            {
                OrderId = order.Id,
                Total = order.Total,
                Status = order.Status,
                CreatedAtUtc = order.CreatedAtUtc
            };

            return ServiceResult<ConfirmOrderResponse>.Ok(
                $"Compra realizada con éxito. Pedido {response.OrderNumber} generado correctamente",
                response,
                201);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            return ServiceResult<ConfirmOrderResponse>.Fail("Ocurrió un error al confirmar la compra", 500);
        }
    }

    public async Task<ServiceResult<List<OrderDto>>> GetOrdersByUserAsync(
    int userId,
    string? status,
    DateTime? fromDate,
    DateTime? toDate,
    CancellationToken cancellationToken)
    {
        if (userId <= 0)
        {
            return ServiceResult<List<OrderDto>>.Fail("El usuario es obligatorio", 400);
        }

        var userExists = await db.Users.AnyAsync(u => u.Id == userId, cancellationToken);
        if (!userExists)
        {
            return ServiceResult<List<OrderDto>>.Fail("El usuario no existe", 404);
        }

        var query = db.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim().ToLower();
            query = query.Where(o => o.Status.ToLower().Contains(normalizedStatus));
        }

        if (fromDate.HasValue)
        {
            query = query.Where(o => o.CreatedAtUtc.Date >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(o => o.CreatedAtUtc.Date <= toDate.Value.Date);
        }

        var orders = await query
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => new OrderDto
            {
                OrderId = o.Id,
                UserId = o.UserId,
                Total = o.Total,
                Status = o.Status,
                CreatedAtUtc = o.CreatedAtUtc,
                Items = o.Items
                    .OrderBy(i => i.ProductName)
                    .Select(i => new OrderItemDto
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        ImageUrl = i.ImageUrl,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        Subtotal = i.Subtotal
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        if (orders.Count == 0)
        {
            return ServiceResult<List<OrderDto>>.Ok("No tienes pedidos en tu historial", orders, 200);
        }

        return ServiceResult<List<OrderDto>>.Ok("Pedidos obtenidos correctamente", orders, 200);
    }

    public async Task<ServiceResult<OrderDto>> GetOrderByIdAsync(int userId, int orderId, CancellationToken cancellationToken)
    {
        if (userId <= 0)
        {
            return ServiceResult<OrderDto>.Fail("El usuario es obligatorio", 400);
        }

        if (orderId <= 0)
        {
            return ServiceResult<OrderDto>.Fail("El pedido es obligatorio", 400);
        }

        var order = await db.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId && o.Id == orderId)
            .Select(o => new OrderDto
            {
                OrderId = o.Id,
                UserId = o.UserId,
                Total = o.Total,
                Status = o.Status,
                CreatedAtUtc = o.CreatedAtUtc,
                Items = o.Items
                    .OrderBy(i => i.ProductName)
                    .Select(i => new OrderItemDto
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        ImageUrl = i.ImageUrl,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        Subtotal = i.Subtotal
                    })
                    .ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            return ServiceResult<OrderDto>.Fail("Pedido no encontrado", 404);
        }

        return ServiceResult<OrderDto>.Ok("Pedido obtenido correctamente", order, 200);
    }
}