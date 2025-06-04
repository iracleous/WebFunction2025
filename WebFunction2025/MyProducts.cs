using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json; // Required for JSON serialization/deserialization
using System.IO;       // Required for stream operations

namespace MyProductFunctions.Functions;

/// <summary>
/// Represents a product with an ID, Name, Price, and Category.
/// </summary>
public class Product
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
}

/// <summary>
/// A simple in-memory data store for products.
/// In a real-world application, this would be replaced by a database (e.g., Azure Cosmos DB, SQL Database).
/// This static class holds the data for demonstration purposes.
/// </summary>
public static class ProductStore
{
    public static List<Product> Products = new List<Product>
    {
        new Product { Id = "P001", Name = "Laptop", Price = 1200.00m, Category = "Electronics" },
        new Product { Id = "P002", Name = "Mouse", Price = 25.00m, Category = "Electronics" },
        new Product { Id = "P003", Name = "Keyboard", Price = 75.00m, Category = "Electronics" },
        new Product { Id = "P004", Name = "Desk Chair", Price = 150.00m, Category = "Furniture" },
        new Product { Id = "P005", Name = "Monitor", Price = 300.00m, Category = "Electronics" }
    };
}

/// <summary>
/// This class defines five HTTP-triggered Azure Function endpoints (API endpoints)
/// for performing CRUD-like operations on a collection of products.
/// Each method corresponds to a distinct endpoint.
/// </summary>
public class ProductFunctions
{
    private readonly ILogger<ProductFunctions> _logger;

    /// <summary>
    /// Constructor for ProductFunctions. The ILogger is automatically
    /// injected by the Azure Functions host via dependency injection.
    /// </summary>
    /// <param name="logger">The logger instance for logging messages.</param>
    public ProductFunctions(ILogger<ProductFunctions> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Endpoint 1: GET /api/products/{id}
    /// Retrieves a single product by its unique ID.
    /// AuthorizationLevel.Function means a function key is required in the request.
    /// </summary>
    /// <param name="req">The HttpRequest object representing the incoming HTTP request.</param>
    /// <param name="id">The product ID extracted from the URL path (e.g., /api/products/P001).</param>
    /// <returns>
    /// An <see cref="OkObjectResult"/> (HTTP 200) with the product if found.
    /// A <see cref="NotFoundObjectResult"/> (HTTP 404) if the product with the given ID is not found.
    /// </returns>
    [Function("GetProductById")] // Logical name for the function
// Defines the HTTP trigger, method, and route
    public IActionResult GetProductById(
      [HttpTrigger(AuthorizationLevel.Function, "get", Route = "products/{id}")]       HttpRequest req, // The incoming HTTP request
        string id)       // Route parameter 'id'
    {
        _logger.LogInformation($"HTTP GET request received for product ID: {id}.");

        // Find the product in the in-memory store (case-insensitive ID comparison)
        var product = ProductStore.Products.FirstOrDefault(p => p.Id.Equals(id, System.StringComparison.OrdinalIgnoreCase));

        if (product == null)
        {
            _logger.LogWarning($"Product with ID '{id}' not found. Returning 404 Not Found.");
            return new NotFoundObjectResult($"Product with ID '{id}' not found.");
        }

        _logger.LogInformation($"Product '{product.Name}' (ID: {id}) found. Returning 200 OK.");
        return new OkObjectResult(product); // Returns the product as JSON with 200 OK status
    }

    /// <summary>
    /// Endpoint 2: GET /api/products
    /// Retrieves a list of all products.
    /// Supports optional filtering by 'category' using a query parameter (e.g., /api/products?category=Electronics).
    /// AuthorizationLevel.Function means a function key is required.
    /// </summary>
    /// <param name="req">The HttpRequest object.</param>
    /// <returns>
    /// An <see cref="OkObjectResult"/> (HTTP 200) with a list of products (filtered or all).
    /// </returns>
    [Function("GetAllProducts")] // Logical name for the function
    // Defines the HTTP trigger, method, and route
    public IActionResult GetAllProducts(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "products")]     HttpRequest req)
    {
        _logger.LogInformation("HTTP GET request received for all products.");

        // Attempt to get the 'category' query parameter
        string? category = req.Query["category"].FirstOrDefault();

        if (!string.IsNullOrEmpty(category))
        {
            // If category is provided, filter the products
            var filteredProducts = ProductStore.Products
                .Where(p => p.Category.Equals(category, System.StringComparison.OrdinalIgnoreCase))
                .ToList();
            _logger.LogInformation($"Returning products filtered by category: '{category}'. Found {filteredProducts.Count} products.");
            return new OkObjectResult(filteredProducts); // Returns filtered list as JSON
        }

        // If no category, return all products
        _logger.LogInformation($"Returning all {ProductStore.Products.Count} products.");
        return new OkObjectResult(ProductStore.Products); // Returns all products as JSON
    }

    /// <summary>
    /// Endpoint 3: POST /api/products
    /// Creates a new product. The product data is expected in the request body as JSON.
    /// AuthorizationLevel.Function means a function key is required.
    /// </summary>
    /// <param name="req">The HttpRequest object containing the JSON payload for the new product.</param>
    /// <returns>
    /// A <see cref="CreatedAtActionResult"/> (HTTP 201) with the created product if successful.
    /// A <see cref="BadRequestObjectResult"/> (HTTP 400) for invalid input (missing ID/Name or malformed JSON).
    /// A <see cref="ConflictObjectResult"/> (HTTP 409) if a product with the same ID already exists.
    /// A <see cref="StatusCodeResult"/> (HTTP 500) for unexpected server errors.
    /// </returns>
    [Function("CreateProduct")] // Logical name for the function
    // Defines the HTTP trigger, method, and route
    public async Task<IActionResult> CreateProduct(
     [HttpTrigger(AuthorizationLevel.Function, "post", Route = "products")]    HttpRequest req)
    {
        _logger.LogInformation("HTTP POST request received to create a product.");

        try
        {
            // Deserialize the JSON request body into a Product object
            // Using System.Text.Json's JsonSerializer for robust deserialization
            var product = await JsonSerializer.DeserializeAsync<Product>(req.Body);

            // Basic validation: ensure product object and essential properties are not null/empty
            if (product == null || string.IsNullOrWhiteSpace(product.Id) || string.IsNullOrWhiteSpace(product.Name))
            {
                _logger.LogError("Invalid product data received. 'Id' and 'Name' are required for creation.");
                return new BadRequestObjectResult("Invalid product data. 'Id' and 'Name' are required.");
            }

            // Check if a product with the same ID already exists to prevent duplicates
            if (ProductStore.Products.Any(p => p.Id.Equals(product.Id, System.StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning($"Product with ID '{product.Id}' already exists. Returning 409 Conflict.");
                return new ConflictObjectResult($"Product with ID '{product.Id}' already exists.");
            }

            // Add the new product to our in-memory store
            ProductStore.Products.Add(product);
            _logger.LogInformation($"Product '{product.Name}' (ID: {product.Id}) created successfully. Returning 201 Created.");

            // Return 201 Created, indicating successful creation and providing the location
            // of the newly created resource, along with the resource itself in the body.
            return new CreatedAtActionResult(nameof(GetProductById), "ProductFunctions", new { id = product.Id }, product);
        }
        catch (JsonException jsonEx)
        {
            // Handle JSON deserialization errors specifically
            _logger.LogError(jsonEx, "Error deserializing product JSON from request body for creation.");
            return new BadRequestObjectResult($"Invalid JSON format in request body: {jsonEx.Message}");
        }
        catch (System.Exception ex)
        {
            // Catch any other unexpected exceptions during the process
            _logger.LogError(ex, "An unexpected error occurred while attempting to create the product.");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError); // Generic 500 error
        }
    }

    /// <summary>
    /// Endpoint 4: PUT /api/products/{id}
    /// Updates an existing product identified by its ID. The updated product data
    /// is expected in the request body as JSON.
    /// AuthorizationLevel.Function means a function key is required.
    /// </summary>
    /// <param name="req">The HttpRequest object containing the updated product data.</param>
    /// <param name="id">The ID of the product to update, extracted from the URL path.</param>
    /// <returns>
    /// An <see cref="OkObjectResult"/> (HTTP 200) with the updated product if successful.
    /// A <see cref="BadRequestObjectResult"/> (HTTP 400) for invalid input or ID mismatch.
    /// A <see cref="NotFoundObjectResult"/> (HTTP 404) if the product to be updated is not found.
    /// A <see cref="StatusCodeResult"/> (HTTP 500) for internal server errors.
    /// </returns>
    [Function("UpdateProduct")] // Logical name for the function
 // Defines the HTTP trigger, method, and route
    public async Task<IActionResult> UpdateProduct(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "products/{id}")]    HttpRequest req,
        string id)
    {
        _logger.LogInformation($"HTTP PUT request received to update product ID: {id}.");

        try
        {
            // Deserialize the JSON request body into a Product object representing the updates
            var updatedProduct = await JsonSerializer.DeserializeAsync<Product>(req.Body);

            // Validation: Ensure the deserialized product is not null, its ID is valid,
            // and the ID in the body matches the ID from the route.
            if (updatedProduct == null || string.IsNullOrWhiteSpace(updatedProduct.Id) || !updatedProduct.Id.Equals(id, System.StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError("Invalid update data or ID mismatch between route and body. Returning 400 Bad Request.");
                return new BadRequestObjectResult("Invalid product data or ID mismatch. Body 'Id' must match route 'id'.");
            }

            // Find the existing product in the store to update
            var existingProduct = ProductStore.Products.FirstOrDefault(p => p.Id.Equals(id, System.StringComparison.OrdinalIgnoreCase));

            if (existingProduct == null)
            {
                _logger.LogWarning($"Product with ID '{id}' not found for update. Returning 404 Not Found.");
                return new NotFoundObjectResult($"Product with ID '{id}' not found for update.");
            }

            // Update the properties of the found existing product
            existingProduct.Name = updatedProduct.Name;
            existingProduct.Price = updatedProduct.Price;
            existingProduct.Category = updatedProduct.Category;

            _logger.LogInformation($"Product '{existingProduct.Name}' (ID: {existingProduct.Id}) updated successfully. Returning 200 OK.");
            return new OkObjectResult(existingProduct); // Returns the updated product as JSON
        }
        catch (JsonException jsonEx)
        {
            // Handle JSON deserialization errors
            _logger.LogError(jsonEx, "Error deserializing product JSON from request body for update.");
            return new BadRequestObjectResult($"Invalid JSON format in request body: {jsonEx.Message}");
        }
        catch (System.Exception ex)
        {
            // Catch any other unexpected exceptions
            _logger.LogError(ex, "An unexpected error occurred while updating the product.");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError); // Generic 500 error
        }
    }

    /// <summary>
    /// Endpoint 5: DELETE /api/products/{id}
    /// Deletes a product by its unique ID.
    /// AuthorizationLevel.Function means a function key is required.
    /// </summary>
    /// <param name="req">The HttpRequest object.</param>
    /// <param name="id">The ID of the product to delete, extracted from the URL path.</param>
    /// <returns>
    /// An <see cref="OkObjectResult"/> (HTTP 200) with a success message if the product is deleted.
    /// A <see cref="NotFoundObjectResult"/> (HTTP 404) if the product to be deleted is not found.
    /// </returns>
    [Function("DeleteProduct")] // Logical name for the function
   // Defines the HTTP trigger, method, and route
    public IActionResult DeleteProduct(
    [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "products/{id}")]      HttpRequest req,
        string id)
    {
        _logger.LogInformation($"HTTP DELETE request received for product ID: {id}.");

        // Find the product to remove from the store
        var productToRemove = ProductStore.Products.FirstOrDefault(p => p.Id.Equals(id, System.StringComparison.OrdinalIgnoreCase));

        if (productToRemove == null)
        {
            _logger.LogWarning($"Product with ID '{id}' not found for deletion. Returning 404 Not Found.");
            return new NotFoundObjectResult($"Product with ID '{id}' not found for deletion.");
        }

        // Remove the product
        ProductStore.Products.Remove(productToRemove);
        _logger.LogInformation($"Product '{id}' deleted successfully. Returning 200 OK.");
        return new OkObjectResult($"Product with ID '{id}' deleted successfully."); // Returns a success message
    }
}
