using WareStockApi.Domain.Constants;
using WareStockApi.Domain.Entities;
using WareStockApi.Domain.Enums;
using WareStockApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WareStockApi.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            // See https://jasontaylor.dev/ef-core-database-initialisation-strategies
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default roles
        var administratorRole = new IdentityRole(Roles.Administrator);

        if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
        {
            await _roleManager.CreateAsync(administratorRole);
        }

        // Default users
        var administrator = new ApplicationUser
        {
            UserName = "administrator",
            Email = "administrator@localhost",
            FirstName = "System",
            LastName = "Administrator",
            DisplayName = "System Administrator",
            PhoneNumber = "0800000000",
            Status = UserStatus.Active
        };

        if (_userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            await _userManager.CreateAsync(administrator, "Administrator1!");
            if (!string.IsNullOrWhiteSpace(administratorRole.Name))
            {
                await _userManager.AddToRolesAsync(administrator, new[] { administratorRole.Name });
            }
        }

        // A couple of extra demo users so lists, dashboard counts and chat seeding have real data.
        var demoUsers = new[]
        {
            new ApplicationUser
            {
                UserName = "jane.doe", Email = "jane.doe@warestock.example.com",
                FirstName = "Jane", LastName = "Doe", DisplayName = "Jane Doe",
                PhoneNumber = "0811111111", Status = UserStatus.Active
            },
            new ApplicationUser
            {
                UserName = "john.smith", Email = "john.smith@warestock.example.com",
                FirstName = "John", LastName = "Smith", DisplayName = "John Smith",
                PhoneNumber = "0822222222", Status = UserStatus.Invited
            }
        };

        foreach (var demoUser in demoUsers)
        {
            if (_userManager.Users.All(u => u.UserName != demoUser.UserName))
            {
                await _userManager.CreateAsync(demoUser, "Password1!");
            }
        }

        // ----- Product categories / units -----

        if (!_context.ProductCategories.Any())
        {
            _context.ProductCategories.AddRange(
                new ProductCategory { Label = "Electronics" },
                new ProductCategory { Label = "Office Supplies" },
                new ProductCategory { Label = "Raw Materials" });
        }

        if (!_context.ProductUnits.Any())
        {
            _context.ProductUnits.AddRange(
                new ProductUnit { Label = "pcs" },
                new ProductUnit { Label = "box" },
                new ProductUnit { Label = "kg" });
        }

        await _context.SaveChangesAsync();

        // ----- Products -----

        if (!_context.Products.Any())
        {
            var products = new[]
            {
                new Product { Sku = "SKU-0001", Name = "Wireless Mouse", Category = "Electronics", Unit = "pcs", Quantity = 120, MinStock = 20, Location = "A1-01", CostPrice = 150m },
                new Product { Sku = "SKU-0002", Name = "Mechanical Keyboard", Category = "Electronics", Unit = "pcs", Quantity = 8, MinStock = 10, Location = "A1-02", CostPrice = 890m },
                new Product { Sku = "SKU-0003", Name = "A4 Paper Ream", Category = "Office Supplies", Unit = "box", Quantity = 300, MinStock = 50, Location = "B2-01", CostPrice = 95m },
                new Product { Sku = "SKU-0004", Name = "Steel Sheet", Category = "Raw Materials", Unit = "kg", Quantity = 15, MinStock = 25, Location = "C3-01", CostPrice = 45m }
            };

            _context.Products.AddRange(products);
            await _context.SaveChangesAsync();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            _context.StockTransactions.AddRange(
                new StockTransaction
                {
                    Type = TransactionType.Receive, ProductId = products[0].Id, ProductName = products[0].Name, Sku = products[0].Sku,
                    Quantity = 50, Date = today.AddDays(-2), Counterparty = "Acme Supplies", PerformedBy = administrator.UserName!
                },
                new StockTransaction
                {
                    Type = TransactionType.Withdraw, ProductId = products[1].Id, ProductName = products[1].Name, Sku = products[1].Sku,
                    Quantity = 2, Date = today.AddDays(-1), Counterparty = "IT Department", PerformedBy = administrator.UserName!
                },
                new StockTransaction
                {
                    Type = TransactionType.Receive, ProductId = products[2].Id, ProductName = products[2].Name, Sku = products[2].Sku,
                    Quantity = 100, Date = today, Counterparty = "Office World", PerformedBy = administrator.UserName!
                });

            await _context.SaveChangesAsync();
        }

        // ----- Work tasks -----

        if (!_context.WorkTasks.Any())
        {
            _context.WorkTasks.AddRange(
                new WorkTask { Title = "Count warehouse A stock", Status = WorkTaskStatus.Todo, Label = TaskLabel.Documentation, Priority = TaskPriority.Medium },
                new WorkTask { Title = "Fix barcode scanner bug", Status = WorkTaskStatus.InProgress, Label = TaskLabel.Bug, Priority = TaskPriority.High },
                new WorkTask { Title = "Add CSV export for tasks", Status = WorkTaskStatus.Done, Label = TaskLabel.Feature, Priority = TaskPriority.Low },
                new WorkTask { Title = "Investigate low stock alerts", Status = WorkTaskStatus.Backlog, Label = TaskLabel.Feature, Priority = TaskPriority.Critical });

            await _context.SaveChangesAsync();
        }

        // ----- Integrations (read-only, seeded) -----

        if (!_context.Integrations.Any())
        {
            _context.Integrations.AddRange(
                new Integration { Id = "github", Name = "GitHub", Desc = "Connect your GitHub account to sync issues and pull requests.", Connected = true },
                new Integration { Id = "slack", Name = "Slack", Desc = "Get notified in Slack when stock levels change.", Connected = false },
                new Integration { Id = "notion", Name = "Notion", Desc = "Sync warehouse documentation with Notion.", Connected = false },
                new Integration { Id = "google-drive", Name = "Google Drive", Desc = "Back up reports to Google Drive automatically.", Connected = true });

            await _context.SaveChangesAsync();
        }

        // ----- Conversations + messages -----
        // No "create message" endpoint exists in the API, so demo data is seeded here to make the
        // Chats pages show something meaningful out of the box.

        if (!_context.Conversations.Any())
        {
            var janeId = _userManager.Users.First(u => u.UserName == "jane.doe").Id;
            var johnId = _userManager.Users.First(u => u.UserName == "john.smith").Id;

            var conversationWithJane = new Conversation
            {
                ParticipantId = janeId,
                Username = "jane.doe",
                FullName = "Jane Doe",
                Title = "Warehouse Supervisor",
                Profile = string.Empty,
                LastMessageAt = DateTimeOffset.UtcNow.AddMinutes(-5)
            };

            var conversationWithJohn = new Conversation
            {
                ParticipantId = johnId,
                Username = "john.smith",
                FullName = "John Smith",
                Title = "Procurement Officer",
                Profile = string.Empty,
                LastMessageAt = DateTimeOffset.UtcNow.AddHours(-3)
            };

            _context.Conversations.AddRange(conversationWithJane, conversationWithJohn);
            await _context.SaveChangesAsync();

            _context.Messages.AddRange(
                new Message { ConversationId = conversationWithJane.Id, SenderId = janeId, Content = "Hi! The A1 shelf recount is done.", Timestamp = DateTimeOffset.UtcNow.AddMinutes(-30) },
                new Message { ConversationId = conversationWithJane.Id, SenderId = administrator.Id, Content = "Thanks Jane, I'll check the numbers.", Timestamp = DateTimeOffset.UtcNow.AddMinutes(-10) },
                new Message { ConversationId = conversationWithJane.Id, SenderId = janeId, Content = "Sounds good, let me know if anything looks off.", Timestamp = DateTimeOffset.UtcNow.AddMinutes(-5) },
                new Message { ConversationId = conversationWithJohn.Id, SenderId = johnId, Content = "The new steel sheet order should arrive Friday.", Timestamp = DateTimeOffset.UtcNow.AddHours(-3) });

            await _context.SaveChangesAsync();
        }
    }
}
