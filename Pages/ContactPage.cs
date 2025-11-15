using Microsoft.Playwright;
using System.Threading.Tasks;

namespace AutomationExerciseTests.Pages
{
    public class ContactPage: BasePage
    {
        public ContactPage(IPage page) : base(page) { }

        public ILocator Name => Page.Locator("input[name='name']");
        public ILocator Email => Page.Locator("input[name='email']");
        public ILocator Subject => Page.Locator("input[name='subject']");
        public ILocator Message => Page.Locator("#message");
        public ILocator SubmitBtn => Page.Locator("input[name='submit']");
        public ILocator SuccessAlert => Page.Locator(".status:has-text('Success')");

        public async Task OpenAsync()
        {
            await Page.GotoAsync("https://automationexercise.com/contact_us");
        }
    }
}
