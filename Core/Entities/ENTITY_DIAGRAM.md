# Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          IMAGINE ECOMMERCE ENTITIES                          │
└─────────────────────────────────────────────────────────────────────────────┘

┌──────────────┐
│   Category   │
│──────────────│
│ Id           │
│ Name         │
│ Description  │
│ ImageUrl     │
│ IsActive     │
└──────┬───────┘
       │ 1
       │
       │ *
┌──────▼───────┐
│   Product    │
│──────────────│
│ Id           │
│ CategoryId   │◄────────────────────────┐
│ Name         │                         │
│ Description  │                         │
│ BasePrice    │                         │
│ MainImageUrl │                         │
│ IsActive     │                         │
│ IsFeatured   │                         │
└──────┬───────┘                         │
       │ 1                               │
       │                                 │
       │ *                               │
┌──────▼────────────┐                    │
│  ProductColor     │                    │
│───────────────────│                    │
│ Id                │                    │
│ ProductId         │                    │
│ ColorName         │                    │
│ ColorHex          │                    │
│ Stock             │                    │
│ AdditionalPrice   │                    │
│ IsAvailable       │                    │
└──────┬────────────┘                    │
       │ 1                               │
       │                                 │
       │ *                               │
┌──────▼────────────┐                    │
│  ProductImage     │                    │
│───────────────────│                    │
│ Id                │                    │
│ ProductColorId    │                    │
│ ImageUrl          │                    │
│ AltText           │                    │
│ IsMain            │                    │
│ DisplayOrder      │                    │
└───────────────────┘                    │
                                         │
┌──────────────┐                         │
│     User     │                         │
│──────────────│                         │
│ Id           │                         │
│ Email        │                         │
│ PasswordHash │                         │
│ FirstName    │                         │
│ LastName     │                         │
│ PhoneNumber  │                         │
│ Address      │                         │
│ IsActive     │                         │
└──────┬───────┘                         │
       │ 1                               │
       ├─────────────────┐               │
       │                 │               │
       │ *               │ *             │
┌──────▼───────┐  ┌──────▼──────────┐   │
│     Cart     │  │ CustomProduct   │   │
│──────────────│  │─────────────────│   │
│ Id           │  │ Id              │   │
│ UserId       │  │ UserId          │   │
│ SessionId    │  │ ProductId       │───┘
│ ExpiresAt    │  │ DesignImageUrl  │
└──────┬───────┘  │ PreviewUrl      │
       │ 1        │ Notes           │
       │          │ EstimatedPrice  │
       │ *        │ Status          │
┌──────▼───────┐  └──────┬──────────┘
│  CartItem    │         │ 1
│──────────────│         │
│ Id           │         │ *
│ CartId       │  ┌──────▼──────────────┐
│ ProductColorId│ │ CustomProductColor  │
│ CustomProductId│ │─────────────────────│
│ Quantity     │  │ Id                  │
│ UnitPrice    │  │ CustomProductId     │
│ TotalPrice   │  │ ColorName           │
└──────────────┘  │ ColorHex            │
                  │ ImageUrl            │
                  └─────────────────────┘

┌──────────────┐
│    Order     │
│──────────────│
│ Id           │
│ UserId       │
│ OrderNumber  │
│ OrderDate    │
│ SubTotal     │
│ ShippingCost │
│ Tax          │
│ TotalAmount  │
│ Status       │
│ ShippingInfo │
│ PaymentInfo  │
│ TrackingNo   │
└──────┬───────┘
       │ 1
       │
       │ *
┌──────▼───────┐
│  OrderItem   │
│──────────────│
│ Id           │
│ OrderId      │
│ ProductColorId│
│ CustomProductId│
│ ProductName  │
│ ColorName    │
│ ImageUrl     │
│ Quantity     │
│ UnitPrice    │
│ TotalPrice   │
└──────────────┘

┌─────────────────────┐
│ UserColorSuggestion │
│─────────────────────│
│ Id                  │
│ UserId              │
│ ProductId           │
│ SuggestedColorName  │
│ SuggestedColorHex   │
│ SuggestedImageUrl   │
│ UserNotes           │
│ Status              │
│ AdminResponse       │
└─────────────────────┘

┌──────────────┐
│   Review     │
│──────────────│
│ Id           │
│ ProductId    │
│ UserId       │
│ Rating       │
│ Title        │
│ Comment      │
│ IsVerified   │
│ IsApproved   │
└──────────────┘

┌──────────────┐
│  Wishlist    │
│──────────────│
│ Id           │
│ UserId       │
│ Name         │
│ IsDefault    │
└──────┬───────┘
       │ 1
       │
       │ *
┌──────▼───────┐
│ WishlistItem │
│──────────────│
│ Id           │
│ WishlistId   │
│ ProductId    │
│ ProductColorId│
└──────────────┘
```

## Relationship Summary

### One-to-Many Relationships
- `Category` → `Product`
- `Product` → `ProductColor`
- `ProductColor` → `ProductImage`
- `Product` → `UserColorSuggestion`
- `Product` → `Review`
- `Product` → `CustomProduct`
- `User` → `Cart`
- `User` → `Order`
- `User` → `CustomProduct`
- `User` → `UserColorSuggestion`
- `User` → `Review`
- `User` → `Wishlist`
- `Cart` → `CartItem`
- `Order` → `OrderItem`
- `CustomProduct` → `CustomProductColor`
- `Wishlist` → `WishlistItem`

### Optional Relationships (Nullable FKs)
- `CartItem` → `ProductColor` (OR)
- `CartItem` → `CustomProduct`
- `OrderItem` → `ProductColor` (OR)
- `OrderItem` → `CustomProduct`
- `Cart` → `User` (for guest carts)
- `Order` → `User` (for guest orders)
- `CustomProduct` → `User` (for guest custom products)
- `CustomProduct` → `Product` (base product is optional)
- `UserColorSuggestion` → `User` (for guest suggestions)
- `WishlistItem` → `ProductColor` (color variant is optional)

## Key Points

1. **Product Hierarchy**: Category → Product → ProductColor → ProductImage
2. **Shopping Flow**: Cart → CartItem → Order → OrderItem
3. **Customization**: User → CustomProduct → CustomProductColor
4. **User Engagement**: User → Review, Wishlist, ColorSuggestion
5. **Flexible Items**: CartItem and OrderItem can reference either ProductColor or CustomProduct
