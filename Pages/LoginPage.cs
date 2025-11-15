using Microsoft.Playwright;
using System.Threading.Tasks;
using AutomationExerciseTests.Util;

namespace AutomationExerciseTests.Pages
{
    public class LoginPage
    {
        private readonly IPage Page;
        public LoginPage(IPage page) { Page = page; }

        // Скоуп към лявата форма "Login", за да избегнем strict-mode конфликт със Signup формата
        private ILocator LoginForm => Page.Locator("form").Filter(new() { HasText = "Login" });

        public ILocator Email => LoginForm.GetByPlaceholder("Email Address");
        public ILocator Password => LoginForm.GetByPlaceholder("Password");
        public ILocator LoginBtn => LoginForm.GetByRole(AriaRole.Button, new() { Name = "Login" });
        public ILocator ErrorMsg => Page.GetByText("Your email or password is incorrect!");

        public async Task GoAsync()
        {
            await Page.GotoAsync("https://automationexercise.com/login",
                new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Махни cookie/consent банерите (иначе блокират клика)
            await Ui.DismissConsentAsync(Page);
        }

        public async Task LoginAsync(string email, string password)
        {
            await Email.FillAsync(email);
            await Password.FillAsync(password);
            await LoginBtn.ScrollIntoViewIfNeededAsync();
            await LoginBtn.ClickAsync();
        }
    }
}
