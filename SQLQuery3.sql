INSERT INTO OrderDetails (OrderId, ProductId, Quantity, Price)
SELECT @OrderId, Cart.ProductId, Cart.Quantity, Cart.Quantity * P.Price
FROM Cart
INNER JOIN Products P ON Cart.ProductId = P.Id
WHERE Cart.UserId = @UserId;