using Kiosk.Data;
using Kiosk.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

public class AddProductModel : PageModel
{
    private readonly KioskDbContext _context;
    public AddProductModel(KioskDbContext context) => _context = context;
    [BindProperty]
    public MenuItem MenuItem { get; set; }

    public SelectList CategorySelectList { get; set; }
    public string Message { get; set; }

    public void OnGet()
    {
        CategorySelectList = new SelectList(_context.Categories.OrderBy(c => c.Name), "Id", "Name");

    }

    public IActionResult OnPost()
    {
        // Odœwie¿amy listê kategorii, jeœli walidacja nie przejdzie
        CategorySelectList = new SelectList(_context.Categories.OrderBy(c => c.Name), "Id", "Name");
        ;

        if (!ModelState.IsValid)
        {
            _context.MenuItems.Add(MenuItem);
            _context.SaveChanges();

            Message = "Produkt zosta³ dodany!";
            MenuItem = new MenuItem();
            return Page();
        }

       
        Message = "Formularz zawiera b³êdy!";
        return Page();
    }

}
