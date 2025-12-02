using Microsoft.AspNetCore.Mvc.RazorPages;
using Kiosk.Data;
using Kiosk.Models;
using Kiosk.Extensions; // konieczne dla GetObjectFromJson
using Microsoft.AspNetCore.Http; // konieczne dla ISession
using System.Collections.Generic;
using System.Linq;

namespace Kiosk.Pages
{
    public class MenuModel : PageModel
    {
        private readonly KioskDbContext _context;

        public MenuModel(KioskDbContext context)
        {
            _context = context;
        }

        public List<Category> Categories { get; set; } = new();

        public void OnGet()
        {
            Categories = _context.Categories.ToList();
        }
    }
}
