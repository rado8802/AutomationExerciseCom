using Microsoft.Playwright;
using System.Threading.Tasks;

namespace AutomationExerciseTests.Pages
{
    public class CheckoutPage : BasePage
    {
        public CheckoutPage(IPage page) : base(page) { }

        // --- Core locators ---
        public ILocator ReviewHeader => Page.GetByText("Review Your Order", new() { Exact = false });
        public ILocator AddressHeader => Page.GetByText("Address Details", new() { Exact = false });
        public ILocator PlaceOrderBtn => Page.GetByRole(AriaRole.Link, new() { Name = "Place Order" });

        // --- Error states ---
        public ILocator CartEmptyHeader => Page.GetByText("Cart is empty", new() { Exact = false });
        public ILocator BuyProductsLink => Page.GetByRole(AriaRole.Link, new() { Name = "click here" });

        // --- NAVIGATION ---
        public async Task OpenAsync()
        {
            await Page.GotoAsync(
                "https://automationexercise.com/checkout",
                new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        }

        // --- VALIDATION ---
        public async Task<bool> IsOnReviewPageAsync()
        {
            if (await ReviewHeader.IsVisibleAsync())
                return true;

            if (await AddressHeader.IsVisibleAsync())
                return true;

            return false;
        }

        /// <summary>
        /// Гарантира, че сме на Review/Address или хвърля изключение,
        /// ако количката е празна или checkout е недостъпен.
        /// </summary>
        public async Task EnsureOnCheckoutOrThrowAsync()
        {
            // 1) Ако review page е OK → всичко е наред
            if (await IsOnReviewPageAsync())
                return;

            // 2) Ако количката е празна → хвърляме ясно съобщение
            if (await CartEmptyHeader.IsVisibleAsync() || await BuyProductsLink.IsVisibleAsync())
                throw new System.Exception("❌ CheckoutPage: Cart is empty → cannot proceed to checkout.");

            // 3) Нищо не се е показало → fallback
            throw new System.Exception("❌ CheckoutPage: Unexpected state — review page not visible and no empty-cart message.");
        }

        // --- ACTIONS ---
        public async Task ClickPlaceOrderAsync()
        {
            await EnsureOnCheckoutOrThrowAsync();
            await PlaceOrderBtn.ClickAsync();
        }
    }
}
