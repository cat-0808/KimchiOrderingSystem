SELECT o.OrderId, o.UserId, u.FullName, u.Email, o.TotalAmount, o.Status, o.OrderDate
FROM Orders o
INNER JOIN Users u ON o.UserId = u.Id
ORDER BY u.FullName, o.OrderDate;
