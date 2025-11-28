using Microsoft.Playwright;
using System.Threading.Tasks;

namespace AutomationExerciseTests.Pages
{
    public class ReviewPage
    {
        private readonly IPage _page;

        public ReviewPage(IPage page)
        {
            _page = page;
        }

        public async Task VerifyReviewSectionVisibleAsync()
        {
            await _page.Locator("#review").WaitForAsync();
        }

        public async Task FillReviewAsync(string name, string email, string review)
        {
            await _page.Locator("#name").FillAsync(name);
            await _page.Locator("#email").FillAsync(email);
            await _page.Locator("#review").FillAsync(review);
            await _page.GetByRole(AriaRole.Button, new() { Name = "Submit" }).ClickAsync();
        }
    }
}
