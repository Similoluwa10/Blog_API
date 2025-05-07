# User Authentication Implementation for the Blog API

This document explains the implementation of user authentication and authorization in the Blog API. The authentication system allows users to register, login, and perform CRUD operations on their own blog posts, while allowing anyone to view all blog posts.

## Table of Contents

0. [Summary of Changes](#summary-of-changes)
1. [Overview](#overview)
2. [Dependencies Added](#dependencies-added)
3. [Model Implementation](#model-implementation)
4. [JWT Authentication Setup](#jwt-authentication-setup)
5. [Authentication Controller](#authentication-controller)
6. [Securing Endpoints](#securing-endpoints)
7. [Swagger Configuration](#swagger-configuration)
8. [Testing the Authentication Flow](#testing-the-authentication-flow)

## Summary of Changes

1. Added JWT authentication packages
2. Fixed the relationship between User and BlogPost models
3. Created login and registration functionality
4. Protected CRUD operations with the [Authorize] attribute
5. Added logic to ensure users can only modify their own blogs
6. Added a new endpoint to get a user's own blogs

These changes implement the requested functionality:
- Any user (logged in or not) can view all blogs
- Only logged-in users can create blogs
- Users can only edit and delete their own blogs

## Overview

The authentication system implements:
- User registration and login functionality
- JWT (JSON Web Token) based authentication
- Role-based access control for API endpoints
- User-specific operations (users can only modify their own blog posts)

The implementation follows these principles:
- **Public access**: Anyone can view blog posts
- **Protected creation**: Only authenticated users can create posts
- **Owner-only modifications**: Only the author of a blog post can edit or delete it

## Dependencies Added

To support authentication, I added the following NuGet packages:

```csharp
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.0.1" />
```

## Model Implementation

### Model Relationships

I established relationships between `UserModel` and `BlogPostModel` to track post ownership:

**BlogPostModel.cs:**
```csharp
public class BlogPostModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required, MaxLength(20)]        
    public string Title { get; set; }

    [Required]
    public string Content { get; set; }

    // Foreign key to User - must be public
    public int UserId { get; set; }

    // Navigation property to User - must be public
    [JsonIgnore]
    public UserModel? User { get; set; }
}
```

**UserModel.cs:**
```csharp
public class UserModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]       
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Username { get; set; }

    [Required]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    
    // Navigation property to BlogPosts - must be public
    [JsonIgnore]
    public ICollection<BlogPostModel> BlogPosts { get; set; } = new List<BlogPostModel>();
}
```

### Authentication Models

I created models for login and registration:

**LoginModel.cs:**
```csharp
public class LoginModel
{
    public string Username { get; set; }
    public string Password { get; set; }
}
```

**RegisterModel.cs:**
```csharp
public class RegisterModel
{
    public string Name { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}
```

### Database Context Configuration

The relationship between User and BlogPost was configured in the DbContext:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Configure the relationship between BlogPost and User
    modelBuilder.Entity<BlogPostModel>()
        .HasOne(b => b.User)
        .WithMany(u => u.BlogPosts)
        .HasForeignKey(b => b.UserId)
        .OnDelete(DeleteBehavior.Cascade);
}
```

## JWT Authentication Setup

### JWT Settings

JWT authentication settings were added to `appsettings.json`:

```json
"JwtSettings": {
    "Key": "MyBlogApiSecretKey12345ThisShouldBeAtLeast32CharactersLong",
    "Issuer": "BlogAPI",
    "Audience": "BlogAPIClients",
    "DurationInMinutes": 120
}
```

### Authentication Configuration in Program.cs

JWT authentication was configured in the `Program.cs` file:

```csharp
// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Set to true in production
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
    };
});
```

And authentication middleware was enabled:

```csharp
// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();
```

## Authentication Controller

I implemented an `AuthController` to handle user registration and login:

```csharp
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly BlogDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(BlogDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        // Check if username already exists
        if (await _context.UserTable.AnyAsync(u => u.Username == model.Username))
        {
            return BadRequest("Username already exists");
        }

        // Check if email already exists
        if (await _context.UserTable.AnyAsync(u => u.Email == model.Email))
        {
            return BadRequest("Email already exists");
        }

        // Create user
        var user = new UserModel
        {
            Name = model.Name,
            Username = model.Username,
            Email = model.Email,
            Password = model.Password // In production, hash the password
        };

        _context.UserTable.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "User registered successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _context.UserTable.SingleOrDefaultAsync(u => 
            u.Username == model.Username && u.Password == model.Password);

        if (user == null)
        {
            return Unauthorized("Invalid username or password");
        }

        var token = GenerateJwtToken(user);
        
        return Ok(new
        {
            token,
            user = new
            {
                id = user.Id,
                name = user.Name,
                username = user.Username,
                email = user.Email
            }
        });
    }

    private string GenerateJwtToken(UserModel user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("id", user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(double.Parse(_configuration["JwtSettings:DurationInMinutes"])),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

## Securing Endpoints

### BlogPostController Updates

The `BlogPostController` was updated to:
- Allow anyone to view blogs
- Require authentication for creating blogs
- Ensure users can only edit and delete their own blogs
- Add an endpoint for users to view their own blogs

```csharp
// Public endpoint - anyone can access
[HttpGet]
public async Task<IEnumerable<BlogPostModel>> GetAllBlogs()
{
    return await _context.BlogPostTable.ToListAsync();
}

// Public endpoint - anyone can access
[HttpGet("{id}")]
public async Task<IActionResult> GetBlogById(int id)
{
    var blog = await _context.BlogPostTable.FirstOrDefaultAsync(t => t.Id == id);

    if (blog == null)
    {
        return NotFound("Item does not exist");
    }
    return Ok(blog);
}

// Protected endpoint - requires authentication
[HttpPost]
[Authorize]
public async Task<ActionResult> CreateBlog([FromBody] BlogPostModel newBlog)
{
    // Get the authenticated user ID
    var userId = GetUserId();
    if (userId == 0)
        return Unauthorized("Invalid user token");

    // Set the blog's author to the authenticated user
    newBlog.UserId = userId;

    _context.BlogPostTable.Add(newBlog);
    await _context.SaveChangesAsync();
    return Ok(newBlog);
}

// Protected endpoint - requires authentication and ownership
[HttpPut]
[Authorize]
public async Task<ActionResult<BlogPostModel>> EditBlog([FromBody] BlogPostModel blog)
{
    var blogToEdit = await _context.BlogPostTable.FirstOrDefaultAsync(t => t.Id == blog.Id);

    if(blogToEdit == null)
    {
        return NotFound("The item you're trying to edit does not exist");
    }

    // Check if the authenticated user owns this blog
    var userId = GetUserId();
    if (userId == 0 || blogToEdit.UserId != userId)
        return Forbid("You can only edit your own blogs");

    blogToEdit.Content = blog.Content;
    blogToEdit.Title = blog.Title;
    
    await _context.SaveChangesAsync();
    return Ok(blogToEdit);
}

// Protected endpoint - requires authentication and ownership
[HttpDelete("{id}")]
[Authorize]
public async Task<ActionResult> DeleteBlog(int id)
{
    var blogToDelete = _context.BlogPostTable.FirstOrDefault(t => t.Id == id);

    if (blogToDelete == null)
    {
        return NotFound("Item does not exist");
    }
    
    // Check if the authenticated user owns this blog
    var userId = GetUserId();
    if (userId == 0 || blogToDelete.UserId != userId)
        return Forbid("You can only delete your own blogs");

    _context.BlogPostTable.Remove(blogToDelete);
    await _context.SaveChangesAsync();
    return Ok("Item successfully deleted");
}

// Protected endpoint - get current user's blogs
[HttpGet]
[Authorize]
public async Task<IActionResult> GetMyBlogs()
{
    var userId = GetUserId();
    if (userId == 0)
        return Unauthorized("Invalid user token");
        
    var blogs = await _context.BlogPostTable
        .Where(b => b.UserId == userId)
        .ToListAsync();
        
    return Ok(blogs);
}

// Helper method to get the authenticated user's ID from claims
private int GetUserId()
{
    var identity = HttpContext.User.Identity as ClaimsIdentity;
    if (identity == null)
        return 0;

    var userIdClaim = identity.FindFirst("id");
    if (userIdClaim == null)
        return 0;

    if (int.TryParse(userIdClaim.Value, out int userId))
        return userId;
        
    return 0;
}
```

## Swagger Configuration

Swagger was configured to support JWT authentication for easier API testing:

```csharp:auth_documentation.md
// Configure Swagger to support JWT Authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Blog API", 
        Version = "v1",
        Description = "A simple blog API with JWT authentication"
    });

    // Define the JWT Bearer Auth scheme for Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n " +
                      "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                      "Example: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\""
    });

    // Require JWT Bearer Auth for all endpoints in Swagger
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
```

## Testing the Authentication Flow

### 1. User Registration

1. Make a POST request to `/api/Auth/register` with:
   ```json:auth_documentation.md
   {
     "name": "Test User",
     "username": "testuser",
     "email": "test@example.com",
     "password": "Password123"
   }
   ```

2. You should receive a successful response:
   ```json
   {
     "message": "User registered successfully"
   }
   ```

### 2. User Login

1. Make a POST request to `/api/Auth/login` with:
   ```json
   {
     "username": "testuser",
     "password": "Password123"
   }
   ```

2. You should receive a JWT token:
   ```json
   {
     "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\""
   }
   ```

### 3. Using the Token

1. For protected endpoints, include an `Authorization` header with:
   ```
   Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\""
   ```

2. Or in Swagger:
   - Click "Authorize" button
   - Enter `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\""`
   - Click "Authorize"

### 4. Testing Access Rules

- Anonymous users can:
  - GET `/api/BlogPost/GetAllBlogs`
  - GET `/api/BlogPost/GetBlogById/{id}`

- Authenticated users can additionally:
  - POST `/api/BlogPost/CreateBlog`
  - GET `/api/BlogPost/GetMyBlogs`
  - PUT `/api/BlogPost/EditBlog` (only their own blogs)
  - DELETE `/api/BlogPost/DeleteBlog/{id}` (only their own blogs)

## Security Considerations

The current implementation has several areas that should be improved for a production environment:

1. **Password Storage**: Currently, passwords are stored in plain text. In a production environment, passwords should always be hashed using a secure algorithm like bcrypt or Argon2.

2. **HTTPS**: Always use HTTPS in production to protect token transmission.

3. **Token Validation**: Consider implementing refresh tokens and shorter-lived access tokens.

4. **JWT Secret Key**: The secret key should be stored securely and not in the appsettings file.

5. **Input Validation**: Add more robust validation for user inputs.

## Conclusion

This implementation provides a basic but functional authentication and authorization system for the Blog API. It allows for public viewing of blogs while restricting creation, editing, and deletion actions to authenticated users and blog owners.

With these changes, the API now satisfies the requirements of allowing any user to view blogs while requiring authentication for CRUD operations and ensuring users can only modify their own blog posts.