# Entity Quick Reference Guide

## Entity Properties at a Glance

### 🏷️ Category
```csharp
Id, Name, Description, ImageUrl, IsActive, DisplayOrder
└─► Products (1:*)
```

### 📦 Product
```csharp
Id, CategoryId, Name, Description, BasePrice, MainImageUrl, IsActive, IsFeatured, ViewCount
├─► Category (1:1)
├─► Colors (1:*)
├─► ColorSuggestions (1:*)
├─► CustomProducts (1:*)
├─► Reviews (1:*)
└─► WishlistItems (1:*)
```

### 🎨 ProductColor
```csharp
Id, ProductId, ColorName, ColorHex, Stock, AdditionalPrice, IsAvailable
├─► Product (1:1)
├─► Images (1:*)
├─► CartItems (1:*)
├─► OrderItems (1:*)
└─► WishlistItems (1:*)
```

### 🖼️ ProductImage
```csharp
Id, ProductColorId, ImageUrl, AltText, IsMain, DisplayOrder
└─► ProductColor (1:1)
```

### 👤 User
```csharp
Id, Email, PasswordHash, FirstName, LastName, PhoneNumber, Address, IsActive, IsEmailConfirmed
├─► Carts (1:*)
├─► Orders (1:*)
├─► CustomProducts (1:*)
├─► ColorSuggestions (1:*)
├─► Reviews (1:*)
└─► Wishlists (1:*)
```

### 🎨 CustomProduct
```csharp
Id, UserId?, ProductId?, CustomDesignImageUrl, AIRenderedPreviewUrl, Notes, EstimatedPrice, Status, AdminNotes
├─► User (1:1?)
├─► Product (1:1?)
├─► CustomColors (1:*)
├─► CartItems (1:*)
└─► OrderItems (1:*)
```

### 🌈 CustomProductColor
```csharp
Id, CustomProductId, ColorName, ColorHex, ImageUrl
└─► CustomProduct (1:1)
```

### 💡 UserColorSuggestion
```csharp
Id, UserId?, ProductId, SuggestedColorName, SuggestedColorHex, SuggestedImageUrl, UserNotes, Status, AdminResponse
├─► User (1:1?)
└─► Product (1:1)
```

### 🛒 Cart
```csharp
Id, UserId?, SessionId, ExpiresAt
├─► User (1:1?)
└─► Items (1:*)
```

### 📋 CartItem
```csharp
Id, CartId, ProductColorId?, CustomProductId?, Quantity, UnitPrice, TotalPrice
├─► Cart (1:1)
├─► ProductColor (1:1?)
└─► CustomProduct (1:1?)
```

### 📦 Order
```csharp
Id, UserId?, OrderNumber, OrderDate, SubTotal, ShippingCost, Tax, TotalAmount, Status
ShippingAddress, ShippingCity, ShippingPostalCode, ShippingCountry, ShippingPhone
PaymentMethod, PaymentTransactionId, PaidAt, TrackingNumber, ShippedAt, DeliveredAt
├─► User (1:1?)
└─► OrderItems (1:*)
```

### 📄 OrderItem
```csharp
Id, OrderId, ProductColorId?, CustomProductId?, ProductName, ColorName, ProductImageUrl, Quantity, UnitPrice, TotalPrice
├─► Order (1:1)
├─► ProductColor (1:1?)
└─► CustomProduct (1:1?)
```

### ⭐ Review
```csharp
Id, ProductId, UserId, Rating, Title, Comment, IsVerifiedPurchase, IsApproved
├─► Product (1:1)
└─► User (1:1)
```

### 💝 Wishlist
```csharp
Id, UserId, Name, IsDefault
├─► User (1:1)
└─► Items (1:*)
```

### 📌 WishlistItem
```csharp
Id, WishlistId, ProductId, ProductColorId?
├─► Wishlist (1:1)
├─► Product (1:1)
└─► ProductColor (1:1?)
```

---

## Common Query Patterns

### Get Product with All Details
```csharp
var product = await context.Products
    .Include(p => p.Category)
    .Include(p => p.Colors)
        .ThenInclude(c => c.Images)
    .Include(p => p.Reviews)
    .FirstOrDefaultAsync(p => p.Id == id);
```

### Get User's Cart with Items
```csharp
var cart = await context.Carts
    .Include(c => c.Items)
        .ThenInclude(i => i.ProductColor)
            .ThenInclude(pc => pc.Product)
    .Include(c => c.Items)
        .ThenInclude(i => i.CustomProduct)
    .FirstOrDefaultAsync(c => c.UserId == userId);
```

### Get Order with Full Details
```csharp
var order = await context.Orders
    .Include(o => o.User)
    .Include(o => o.OrderItems)
        .ThenInclude(oi => oi.ProductColor)
    .Include(o => o.OrderItems)
        .ThenInclude(oi => oi.CustomProduct)
    .FirstOrDefaultAsync(o => o.Id == orderId);
```

### Get Custom Product with Colors
```csharp
var customProduct = await context.CustomProducts
    .Include(cp => cp.User)
    .Include(cp => cp.Product)
    .Include(cp => cp.CustomColors)
    .FirstOrDefaultAsync(cp => cp.Id == id);
```

### Get Product Color Suggestions
```csharp
var suggestions = await context.UserColorSuggestions
    .Include(s => s.User)
    .Include(s => s.Product)
    .Where(s => s.Status == SuggestionStatus.Pending)
    .ToListAsync();
```

---

## Business Rules

### Cart Item Rules
- Either `ProductColorId` OR `CustomProductId` must be set (not both)
- `Quantity` must be > 0
- `TotalPrice` = `UnitPrice` × `Quantity`

### Order Item Rules
- Stores snapshot of product details at purchase time
- `ProductName`, `ColorName`, `ProductImageUrl` are denormalized for historical record
- References to `ProductColor` or `CustomProduct` may become null if deleted

### Custom Product Rules
- `Status` workflow: Draft → PendingReview → Approved → InProduction → Completed
- Can be based on existing product (`ProductId`) or completely custom
- Must have at least one `CustomProductColor`

### Color Suggestion Rules
- `Status` workflow: Pending → UnderReview → Approved/Rejected → Implemented
- If approved and implemented, admin should create corresponding `ProductColor`

### Stock Management
- Stock is tracked at `ProductColor` level
- Decrease stock when order is placed
- Increase stock if order is cancelled/refunded

### Price Calculation
- Product final price = `Product.BasePrice` + `ProductColor.AdditionalPrice`
- Custom product price is stored in `CustomProduct.EstimatedPrice`

---

## Validation Rules

### Required Fields
- **Product**: Name, CategoryId, BasePrice
- **ProductColor**: ColorName, ProductId
- **ProductImage**: ImageUrl, ProductColorId
- **User**: Email, PasswordHash, FirstName, LastName
- **Order**: OrderNumber, ShippingAddress, ShippingCity, ShippingPostalCode, ShippingCountry
- **OrderItem**: ProductName, Quantity, UnitPrice, TotalPrice

### String Lengths
- Category.Name: 100
- Product.Name: 200
- ProductColor.ColorName: 100
- ProductColor.ColorHex: 10
- User.Email: 256
- User.FirstName/LastName: 100

### Numeric Constraints
- Review.Rating: 1-5
- Quantity: > 0
- Prices: >= 0
- Stock: >= 0

---

## Status Enums Quick Reference

### OrderStatus
`Pending → Processing → Shipped → Delivered`
`(or Cancelled/Refunded at any point)`

### CustomProductStatus
`Draft → PendingReview → Approved → InProduction → Completed`
`(or Rejected after PendingReview)`

### SuggestionStatus
`Pending → UnderReview → Approved → Implemented`
`(or Rejected after UnderReview)`

---

## Tips for Implementation

1. **Always use transactions** when creating orders from cart
2. **Validate stock** before adding to cart or creating order
3. **Use soft deletes** for products (set IsActive = false)
4. **Generate unique OrderNumber** (e.g., "ORD-20240110-0001")
5. **Store prices at order time** to preserve historical accuracy
6. **Index foreign keys** for better query performance
7. **Add unique constraints** on User.Email, Order.OrderNumber
8. **Implement optimistic concurrency** for stock updates
9. **Use DTOs** for API responses (don't expose entities directly)
10. **Implement caching** for frequently accessed data (categories, products)
