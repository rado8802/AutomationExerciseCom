using Microsoft.Playwright;
using System.Threading.Tasks;

namespace AutomationExerciseTests.Pages
{
    public class ProductDetailsPage : BasePage
    {
        public ProductDetailsPage(IPage page) : base(page) { }

        public ILocator AddToCartButton => Page.GetByRole(AriaRole.Button, new() { Name = "Add to cart" });
        public ILocator QuantityInput => Page.Locator("#quantity");
        public ILocator ViewCartInModal => Page.Locator("a[href='/view_cart']");
        public ILocator ReviewSuccess => Page.Locator("span:has-text('Thank you for your review.')");

        public async Task OpenAsync(string productUrl)
        {
            await Page.GotoAsync(productUrl);
        }

        public async Task SetQuantityAsync(int quantity)
        {
            await QuantityInput.FillAsync(quantity.ToString());
        }

        public async Task AddToCartAsync()
        {
            await AddToCartButton.ClickAsync();
        }

        public async Task SubmitReviewAsync(string name, string email, string message)
        {
            await Page.FillAsync("#name", name);
            await Page.FillAsync("#email", email);
            await Page.FillAsync("#review", message);
            await Page.ClickAsync("#button-review");
        }
    }
}
