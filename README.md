# Imagine API & Admin UI – Category Module Overview

This document explains the structure of the **Category module** across the .NET API and the Angular admin UI inside `Imagine/ClientApp`. It also captures the main patterns so you can keep future tasks consistent and in one place.

---

## 1. Solution Structure (High Level)

```text
Api/Imagine/
├── Core/                 # Domain layer (entities, interfaces)
├── Application/          # Use cases (CQRS, DTOs, validators)
├── Infrastructure/       # Persistence, repositories, infrastructure services
├── Imagine/              # ASP.NET Core Web API host + Angular ClientApp
│   ├── Controllers/      # API controllers (Products, Categories, Cart, ...)
│   └── ClientApp/        # Angular 16+ SPA (admin + public UI)
└── README.md             # This document
```

The Category module follows the **same clean architecture** as Products: entities live in `Core`, behavior in `Application`, data access in `Infrastructure`, and HTTP endpoints in `Imagine/Controllers`.

---

## 2. Backend (.NET) – Category Module

### 2.1 Entity

**Path:** `Core/Entities/Category.cs`

```csharp
public class Category : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;

    // Navigation
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
```

`BaseEntity` provides common auditing fields:

```csharp
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
```

### 2.2 Repository

**Path:** `Core/Interfaces/ICategoryRepository.cs`

```csharp
public interface ICategoryRepository : IBaseRepository<Category>
{
}
```

**Implementation:** `Infrastructure/Repositories/CategoryRepository.cs` using `BaseRepository<Category>` and `ApplicationDbContext`.

### 2.3 DTOs

**List/Details DTO** – `Application/Features/Categories/DTOs/CategoryDto.cs`

```csharp
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public int ProductCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

This is used for **both** list and details, to keep things simple.

**Input DTOs** (if needed for forms) are kept minimal in:

```csharp
Application/Features/Categories/DTOs/AddCategoryDTO.cs
// CreateCategoryDto and UpdateCategoryDto share the same shape
```

### 2.4 Commands & Queries

All commands/queries live under:

```text
Application/Features/Categories/
  Commands/
    CreateCategory/
    UpdateCategory/
    DeleteCategory/
  Queries/
    GetCategoriesList/
    GetCategoryById/
```

Key types:

- **GetCategoriesListQuery** – list with search/filters/paging
- **GetCategoryByIdQuery** – single category by Id
- **CreateCategoryCommand** – create new category
- **UpdateCategoryCommand** – update existing category
- **DeleteCategoryCommand** – remove category

Each request returns a `BaseResponse<T>` wrapper from `Application/Common/Models/BaseResponse.cs` (with `success`, `message`, `data`, and optional paging info).

### 2.5 Handlers

Handlers follow the same pattern as `Products`:

- Inject `ICategoryRepository` (and `IQueryService` for list).
- Load entity, validate existence, map to/from DTOs.
- Set `CreatedAt` / `UpdatedAt` appropriately.
- Return `BaseResponse<T>.SuccessResponse(...)` or throw `NotFoundException`.

Examples:

- `GetCategoriesListQueryHandler` – applies search, filter by `IsActive`, sorting, and pagination; projects to `CategoryDto` with `ProductCount`.
- `GetCategoryByIdQueryHandler` – loads one `Category`, maps to `CategoryDto`.
- `CreateCategoryCommandHandler` – creates new `Category` from command fields.
- `UpdateCategoryCommandHandler` – updates fields and sets `UpdatedAt = DateTime.UtcNow`.
- `DeleteCategoryCommandHandler` – deletes the entity if found.

### 2.6 Validators

Validators live in `Application/Features/Categories/Validators`:

- **CreateCategoryValidator** – validates name length, description/image path length, and `DisplayOrder >= 0`.
- **UpdateCategoryValidator** – same rules plus `Id > 0`.

All validators inherit from a shared `BaseValidator<T>`.

### 2.7 Controller

**Path:** `Imagine/Controllers/CategoriesController.cs`

REST endpoints:

```csharp
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    // GET /api/categories
    [HttpGet]
    public async Task<ActionResult<BaseResponse<List<CategoryDto>>>> GetCategories([FromQuery] GetCategoriesListQuery query, CancellationToken ct)

    // GET /api/categories/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BaseResponse<CategoryDto>>> GetCategoryById(int id, CancellationToken ct)

    // POST /api/categories
    [HttpPost]
    public async Task<ActionResult<BaseResponse<int>>> CreateCategory([FromBody] CreateCategoryCommand command, CancellationToken ct)

    // PUT /api/categories/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BaseResponse<bool>>> UpdateCategory(int id, [FromBody] UpdateCategoryCommand command, CancellationToken ct)

    // DELETE /api/categories/{id}
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<BaseResponse<bool>>> DeleteCategory(int id, CancellationToken ct)
}
```

These map 1:1 to what the Angular admin `CategoryService` calls.

---

## 3. Frontend (Angular – Imagine/ClientApp)

The Angular SPA lives under:

```text
Imagine/ClientApp/src/app/
  core/                 # Shared models/services
  layout/               # Shell, navbar, etc.
  Pages/Admin/category/ # Category admin module (header, list, form, etc.)
```

### 3.1 Shared API response interface

**Path:** `src/app/core/IApiResponse.ts`

```ts
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  currentPage?: number;
  pageSize?: number;
  totalItems?: number;
  totalPages?: number;
}
```

This mirrors `BaseResponse<T>` on the backend and is reused by all HTTP services.

### 3.2 Category core (admin)

```text
src/app/Pages/Admin/category/Core/
  Interface/ICategory.ts         # Category model
  Service/category.service.ts    # Category API service
```

**ICategory** – `Core/Interface/ICategory.ts`:

```ts
export interface ICategory {
  id: number;
  name: string;
  description: string;
  imageUrl: string;
  isActive: boolean;
  displayOrder: number;
  productCount: number;
  createdAt: string;
}
```

**CategoryService** – `Core/Service/category.service.ts`:

```ts
@Injectable({ providedIn: 'root' })
export class CategoryService {
  private baseUrl = environment.apiUrl + '/api/categories';

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<ICategory[]>> {
    return this.http.get<ApiResponse<ICategory[]>>(this.baseUrl);
  }

  getById(id: number): Observable<ApiResponse<ICategory>> {
    return this.http.get<ApiResponse<ICategory>>(`${this.baseUrl}/${id}`);
  }

  create(data: any): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(this.baseUrl, data);
  }

  update(id: number, category: any): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.baseUrl}/${id}`, category);
  }

  delete(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/${id}`);
  }
}
```

### 3.3 Category admin components

Path: `src/app/Pages/Admin/category/Components/`

Key components:

- **`category.ts`** – main Page component, wires header + list + empty state.
- **`category-header`** – filters, search, view toggle, “Add category” button.
- **`category-list`** – renders grid/list of categories; handles delete/edit via modals.
- **`category-form`** – NG-Bootstrap modal for create/edit with image upload preview.
- **`category-empty-state`** – shown when no categories exist.

#### CategoryForm

- Uses `CategoryService` to call `create` / `update`.
- Binds a `categoryobj` model for name, description, `imagePath`, `isActive`, `displayOrder`.
- Displays SweetAlert2 dialogs for validation, success, and error states.

Import at top:

```ts
import Swal from 'sweetalert2';
```

Image validation example:

```ts
if (!file.type.startsWith('image/')) {
  this.clearImage();
  Swal.fire({
    icon: 'warning',
    title: 'Invalid image',
    text: 'Please select a valid image file (PNG, JPG, JPEG)',
  });
}
```

Save logic example:

```ts
if (this.category && this.category.id) {
  this.CategoryService.update(this.category.id, payload).subscribe({
    next: (res) => {
      Swal.fire({
        icon: 'success',
        title: 'Category updated',
        text: res.message || 'Category updated successfully',
      }).then(() => this.activeModal.close(res.success));
    },
    error: (err) => {
      Swal.fire({
        icon: 'error',
        title: 'Update failed',
        text: 'Error: ' + (err?.error?.message || JSON.stringify(err)),
      });
    }
  });
} else {
  this.CategoryService.create(payload).subscribe({
    next: (res) => {
      Swal.fire({
        icon: 'success',
        title: 'Category created',
        text: res.message || 'Category created successfully',
      }).then(() => this.activeModal.close(res.success));
    },
    error: (err) => {
      Swal.fire({
        icon: 'error',
        title: 'Create failed',
        text: 'Error: ' + (err?.error?.message || JSON.stringify(err)),
      });
    }
  });
}
```

Category list uses a shared `ConfirmationModal` for delete confirmation and then calls `CategoryService.delete(id)`.

---

## 4. Auth & Identity (Login / Register)

The API exposes simple JWT-based authentication endpoints under `api/Auth`:

- `POST /api/Auth/register` – accepts a `RegisterRequestDto` (fullName?, email, phoneNumber, password, confirmPassword) and returns `BaseResponse<string>` where `data` is the new user id.
- `POST /api/Auth/login` – accepts a `LoginRequestDto` (identifier, password) and returns `BaseResponse<LoginResultDto>` with:
  - `token` – JWT access token
  - `userId` – Identity user id
  - `email`, `phoneNumber`, `fullName`
  - `roles` – list of role names (e.g. `Admin`, `Client`)

Implementation details:

- Uses ASP.NET Core Identity (`ApplicationUser : IdentityUser`) with roles `Admin` and `Client` seeded via `DatabaseSeeder`.
- Commands/queries live under `Application/Features/Users` (e.g. `RegisterUserCommand`, `LoginUserQuery`).
- JWT generation is centralized in `IJwtService` / `JwtService` and wired in `Infrastructure/DependencyInjection`.
- Tokens are currently configured **without an explicit expiration** and validation uses `ValidateLifetime = false`, which makes tokens effectively non-expiring.

> ⚠ **Security note**: Non-expiring tokens are not recommended for production. For real deployments you should:
> - Add token expiry (`exp` claim) and enable `ValidateLifetime`.
> - Consider short-lived access tokens with refresh tokens and rotation.
> - Revoke tokens on password change / logout where appropriate.

The Angular client uses `AuthService` in `ClientApp/src/app/core/auth.service.ts` to call these endpoints and store the token + roles in `localStorage`, and a simple route guard protects the `/client` area.

---

## 5. SweetAlert2 Setup

To use SweetAlert2 in the Angular ClientApp:

1. Go to the **ClientApp** directory:

   ```bash
   cd Imagine/ClientApp
   ```

2. Install the package:

   ```bash
   npm install sweetalert2 --save
   ```

3. Import in any component where you need it:

   ```ts
   import Swal from 'sweetalert2';
   ```

4. Use `Swal.fire({ ... })` for user feedback instead of `window.alert(...)`.

---

## 5. How to Add a New Module Using This Pattern

1. **Backend**
   - Create entity in `Core/Entities`.
   - Add repository interface in `Core/Interfaces` and implementation in `Infrastructure/Repositories`.
   - Create DTO(s) under `Application/Features/<Module>/DTOs`.
   - Add Commands & Queries under `Application/Features/<Module>/Commands` and `Queries`.
   - Implement handlers that return `BaseResponse<T>`.
   - Add validators in `Application/Features/<Module>/Validators`.
   - Create a controller in `Imagine/Controllers` mirroring the Products/Categories style.

2. **Frontend (ClientApp)**
   - Add shared models (if global) in `src/app/core`.
   - For admin modules, add `Core/Interface` and `Core/Service` under the specific page folder.
   - Keep Components under `Pages/Admin/<module>/Components` and wire them in the page file.
   - Use SweetAlert2 and NG-Bootstrap for UX and modals.

Following this layout keeps all tasks consistent, makes the code easy to navigate, and aligns the Angular admin UI tightly with the .NET clean architecture on the backend.
