# Imagine eCommerce - Domain Entities

## Overview
This directory contains all domain entities for the Imagine eCommerce application following Clean Architecture principles.

## Entity Relationships

### Core Product Entities

#### **Category**
- Root entity for product categorization
- **Relationships:**
  - One-to-Many with `Product`

#### **Product**
- Main product entity
- **Relationships:**
  - Many-to-One with `Category`
  - One-to-Many with `ProductColor`
  - One-to-Many with `UserColorSuggestion`
  - One-to-Many with `CustomProduct`
  - One-to-Many with `Review`
  - One-to-Many with `WishlistItem`

#### **ProductColor**
- Represents different color variants of a product
- **Relationships:**
  - Many-to-One with `Product`
  - One-to-Many with `ProductImage`
  - One-to-Many with `CartItem`
  - One-to-Many with `OrderItem`
  - One-to-Many with `WishlistItem`

#### **ProductImage**
- Images for each product color variant
- **Relationships:**
  - Many-to-One with `ProductColor`

---

### Custom Product Entities

#### **CustomProduct**
- User-created custom products with personalized designs
- **Relationships:**
  - Many-to-One with `User` (optional - for guest users)
  - Many-to-One with `Product` (optional - base product)
  - One-to-Many with `CustomProductColor`
  - One-to-Many with `CartItem`
  - One-to-Many with `OrderItem`

#### **CustomProductColor**
- Custom colors chosen by users for their custom products
- **Relationships:**
  - Many-to-One with `CustomProduct`

#### **UserColorSuggestion**
- User suggestions for new product colors
- **Relationships:**
  - Many-to-One with `User` (optional - for guest suggestions)
  - Many-to-One with `Product`

---

### Shopping Cart & Orders

#### **Cart**
- Shopping cart for each user (or guest session)
- **Relationships:**
  - Many-to-One with `User` (optional - for guest carts)
  - One-to-Many with `CartItem`

#### **CartItem**
- Individual items in a cart (can be regular product or custom product)
- **Relationships:**
  - Many-to-One with `Cart`
  - Many-to-One with `ProductColor` (optional)
  - Many-to-One with `CustomProduct` (optional)

#### **Order**
- Completed order with shipping and payment details
- **Relationships:**
  - Many-to-One with `User` (optional - for guest orders)
  - One-to-Many with `OrderItem`

#### **OrderItem**
- Individual items in an order (snapshot of product at purchase time)
- **Relationships:**
  - Many-to-One with `Order`
  - Many-to-One with `ProductColor` (optional - reference only)
  - Many-to-One with `CustomProduct` (optional - reference only)

---

### User & Engagement

#### **User**
- Application user (customer or admin)
- **Relationships:**
  - One-to-Many with `Cart`
  - One-to-Many with `Order`
  - One-to-Many with `CustomProduct`
  - One-to-Many with `UserColorSuggestion`
  - One-to-Many with `Review`
  - One-to-Many with `Wishlist`

#### **Review**
- Product reviews and ratings
- **Relationships:**
  - Many-to-One with `Product`
  - Many-to-One with `User`

#### **Wishlist**
- User's saved products for later
- **Relationships:**
  - Many-to-One with `User`
  - One-to-Many with `WishlistItem`

#### **WishlistItem**
- Individual products in a wishlist
- **Relationships:**
  - Many-to-One with `Wishlist`
  - Many-to-One with `Product`
  - Many-to-One with `ProductColor` (optional)

---

## Enums

### **OrderStatus**
- `Pending` - Order placed, awaiting processing
- `Processing` - Order is being prepared
- `Shipped` - Order has been shipped
- `Delivered` - Order delivered to customer
- `Cancelled` - Order cancelled
- `Refunded` - Order refunded

### **CustomProductStatus**
- `Draft` - User is still designing
- `PendingReview` - Submitted for admin review
- `Approved` - Approved by admin
- `InProduction` - Being manufactured
- `Completed` - Production completed
- `Rejected` - Rejected by admin

### **SuggestionStatus**
- `Pending` - Awaiting review
- `UnderReview` - Being reviewed by admin
- `Approved` - Approved for implementation
- `Rejected` - Rejected
- `Implemented` - Color added to product

### **UserRole**
- `Customer` - Regular customer
- `Admin` - Administrator
- `SuperAdmin` - Super administrator

---

## Key Design Decisions

1. **BaseEntity**: All entities inherit from `BaseEntity` which provides `Id`, `CreatedAt`, and `UpdatedAt` properties.

2. **Nullable Foreign Keys**: `UserId` is nullable in `Cart`, `Order`, and `CustomProduct` to support guest users.

3. **Product Variants**: `ProductColor` acts as the product variant entity, containing stock and pricing information.

4. **Custom Products**: Users can create custom products with their own designs and color choices, tracked separately from regular products.

5. **Order Snapshots**: `OrderItem` stores product name, color, and image URL to preserve order details even if products change.

6. **Flexible Cart Items**: `CartItem` can reference either a `ProductColor` or a `CustomProduct` (one must be set).

7. **Color Suggestions**: Users can suggest new colors for products, which admins can approve/reject.

8. **Wishlist Support**: Users can save products to wishlists for later purchase.

9. **Review System**: Products can be reviewed and rated by users.

---

## Usage Notes

- These are **domain entities** in the Core layer (domain models).
- The Data layer should contain EF Core configurations for these entities.
- Use DTOs in the application layer for data transfer.
- Apply the Repository pattern in the Infrastructure layer for data access.
