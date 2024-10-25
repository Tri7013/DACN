using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WEBTRUYEN.Data;
using WEBTRUYEN.Data.Users;
using WEBTRUYEN.Repository;
using WEBTRUYEN.VnPay;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped(typeof(IRepository<>), typeof(EFRepository<>));
builder.Services.AddDbContext<ApplicationDbContext>(options =>

options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
 .AddDefaultTokenProviders()
 .AddDefaultUI()
 .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
var configuration = builder.Configuration;
builder.Services.AddAuthentication().AddGoogle(googleOptions =>
{
    googleOptions.ClientId = configuration["Authentication:Google:ClientId"];
    googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"];

    googleOptions.ClaimActions.MapJsonKey("name", "name"); // Lấy tên từ thông tin Google
    googleOptions.Events.OnCreatingTicket = async context =>
    {
        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = context.HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();

        var email = context.User.GetProperty("email").GetString();
        var name = context.User.GetProperty("name").GetString();

        // Tìm người dùng theo email
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // Nếu người dùng không tồn tại, tạo người dùng mới
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = name ?? "Default Name", // Gán giá trị mặc định nếu name là null
                IsVip = false // Hoặc theo mặc định mà bạn muốn
            };

            // Tạo người dùng mới
            var result = await userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                // Xử lý lỗi nếu có
                throw new Exception("Có lỗi xảy ra khi tạo người dùng mới");
            }

            // Thêm vai trò "Customer" cho người dùng mới
            if (!await roleManager.RoleExistsAsync("Customer"))
            {
                await roleManager.CreateAsync(new IdentityRole("Customer"));
            }

            await userManager.AddToRoleAsync(user, "Customer");
        }
        else
        {
            // Nếu người dùng đã tồn tại, cập nhật tên người dùng
            user.FullName = name ?? user.FullName; // Cập nhật chỉ khi name không phải là null
            await userManager.UpdateAsync(user);

            // Đảm bảo người dùng có vai trò "Customer"
            if (!await userManager.IsInRoleAsync(user, "Customer"))
            {
                await userManager.AddToRoleAsync(user, "Customer");
            }
        }
    };
});




builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.LogoutPath = $"/Identity/Account/AccessDenied";


});
// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
//    .AddRoles<IdentityRole>()
//    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IVnPayService, VnPayService>(); // Đăng ký dịch vụ VNPay

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{

    endpoints.MapControllerRoute(name: "Admin", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
    endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
});
app.MapRazorPages();
await SeedRolesAndUsers(app);

async Task SeedRolesAndUsers(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Tạo vai trò
        var roles = new[] { "Admin", "Author", "Customer" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Tạo người dùng
        string email = "admin@admin.com";
        string password = "Tri@123"; // Nên yêu cầu người dùng thay đổi mật khẩu lần đầu
        string fullname = "Admin";
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullname,
                IsVip = true // Thiết lập IsVip là true
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                // Xử lý lỗi nếu người dùng không được tạo thành công
                // Có thể ghi log hoặc thông báo
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Thêm người dùng vào vai trò admin
        if (!await userManager.IsInRoleAsync(user, "Admin"))
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }
}



app.Run();
