using Microsoft.Playwright;
using System.Threading.Tasks;

namespace AutomationExerciseTests.Pages
{
    public class LoginPage
    {
        private readonly IPage Page;
        public LoginPage(IPage page) { Page = page; }

        private ILocator LoginForm => Page.Locator("form").Filter(new() { HasText = "Login" });

        public ILocator Email => LoginForm.GetByPlaceholder("Email Address");
        public ILocator Password => LoginForm.GetByPlaceholder("Password");
        public ILocator LoginBtn => LoginForm.GetByRole(AriaRole.Button, new() { Name = "Login" });

        public async Task LoginAsync(string email, string password)
        {
            await Email.FillAsync(email);
            await Password.FillAsync(password);
            await LoginBtn.ClickAsync();
        }
    }
}
