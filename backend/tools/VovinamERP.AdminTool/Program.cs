using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VovinamERP.Infrastructure.DependencyInjection;
using VovinamERP.Infrastructure.Identity;

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var services = new ServiceCollection();

services.AddLogging();
services.AddInfrastructure(
    configuration,
    AppContext.BaseDirectory);

using var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();

var userManager = scope.ServiceProvider
    .GetRequiredService<UserManager<ApplicationUser>>();

var adminUserName = configuration["Bootstrap:AdminUserName"];

if (string.IsNullOrWhiteSpace(adminUserName))
{
    Console.WriteLine("Chua cau hinh Bootstrap:AdminUserName.");
    return;
}

var user = await userManager.FindByNameAsync(adminUserName);

if (user is null)
{
    Console.WriteLine("Khong tim thay tai khoan SuperAdmin.");
    return;
}

if (!await userManager.IsInRoleAsync(user, "SuperAdmin"))
{
    Console.WriteLine("Tai khoan khong co vai tro SuperAdmin.");
    return;
}

Console.WriteLine("Da xac minh tai khoan SuperAdmin.");
Console.WriteLine("Mat khau moi phai co it nhat 8 ky tu, chu hoa, chu thuong va chu so.");

static string ReadHiddenPassword(string prompt)
{
    Console.Write(prompt);
    var password = new System.Text.StringBuilder();

    while (true)
    {
        var key = Console.ReadKey(intercept: true);

        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            break;
        }

        if (key.Key == ConsoleKey.Backspace)
        {
            if (password.Length > 0)
                password.Length--;
            continue;
        }

        if (!char.IsControl(key.KeyChar))
            password.Append(key.KeyChar);
    }

    return password.ToString();
}

var newPassword = ReadHiddenPassword("Nhap mat khau moi: ");
var confirmPassword = ReadHiddenPassword("Nhap lai mat khau moi: ");

if (newPassword != confirmPassword)
{
    Console.WriteLine("Hai mat khau khong khop. Khong thay doi du lieu.");
    return;
}

if (string.IsNullOrWhiteSpace(newPassword))
{
    Console.WriteLine("Mat khau khong hop le.");
    return;
}

Console.Write("Xac nhan doi mat khau SuperAdmin (YES): ");
var confirmation = Console.ReadLine();

if (confirmation != "YES")
{
    Console.WriteLine("Da huy thao tac.");
    return;
}

var token = await userManager.GeneratePasswordResetTokenAsync(user);
var result = await userManager.ResetPasswordAsync(
    user,
    token,
    newPassword);

if (!result.Succeeded)
{
    Console.WriteLine("Doi mat khau that bai.");

    foreach (var error in result.Errors)
        Console.WriteLine($"{error.Code}: {error.Description}");

    return;
}

var verified = await userManager.CheckPasswordAsync(
    user,
    newPassword);

if (!verified)
{
    Console.WriteLine("CANH BAO: Khong xac minh duoc mat khau moi.");
    return;
}

Console.WriteLine("THANH CONG: Da doi va xac minh mat khau SuperAdmin.");
