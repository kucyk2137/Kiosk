using Kiosk.Data;
using Kiosk.Extensions;
using Kiosk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kiosk.Pages
{
    public class PaymentModel : PageModel
    {
        private readonly KioskDbContext _context;

        public PaymentModel(KioskDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string SelectedPaymentMethod { get; set; }

        public decimal TotalPrice { get; private set; }

        public string CreatedOrderNumber { get; private set; }

        private List<OrderItem> Cart => HttpContext.Session.GetObjectFromJson<List<OrderItem>>("Cart") ?? new();

        public IActionResult OnGet()
        {
            if (!Cart.Any())
            {
                return RedirectToPage("/Cart");
            }

            CalculateTotals();
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!Cart.Any())
            {
                return RedirectToPage("/Cart");
            }

            CalculateTotals();

            if (string.IsNullOrWhiteSpace(SelectedPaymentMethod))
            {
                ModelState.AddModelError(string.Empty, "Wybierz metodê p³atnoœci.");
                return Page();
            }

            var orderNumber = GenerateOrderNumber();

            var order = new Order
            {
                OrderNumber = orderNumber,
                PaymentMethod = SelectedPaymentMethod,
                OrderType = HttpContext.Session.GetString("OrderType"),
                Items = Cart
                    .Select(ci => new OrderItem
                    {
                        MenuItemId = ci.MenuItemId,
                        Quantity = ci.Quantity,
                        SelectedIngredients = ci.SelectedIngredients
                    })
                    .ToList()
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            HttpContext.Session.Remove("Cart");
            HttpContext.Session.Remove("OrderType");
            HttpContext.Session.Clear();
            CreatedOrderNumber = orderNumber;

            return Page();
        }

        private void CalculateTotals()
        {
            var ids = Cart.Select(ci => ci.MenuItemId).ToList();
            var menuItems = _context.MenuItems.Where(m => ids.Contains(m.Id)).ToDictionary(m => m.Id, m => m);

            TotalPrice = Cart
                .Where(ci => menuItems.ContainsKey(ci.MenuItemId))
                .Sum(ci => menuItems[ci.MenuItemId].Price * ci.Quantity);
        }

        private string GenerateOrderNumber()
        {
            string BuildNumber() => $"#{DateTime.Now:MMdd}-{Random.Shared.Next(1000, 9999)}";

            var orderNumber = BuildNumber();
            var attempts = 0;

            while (_context.Orders.Any(o => o.OrderNumber == orderNumber) && attempts < 5)
            {
                orderNumber = BuildNumber();
                attempts++;
            }

            return orderNumber;
        }
    }
}