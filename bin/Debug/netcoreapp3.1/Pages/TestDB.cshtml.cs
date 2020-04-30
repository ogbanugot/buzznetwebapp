using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BuzznetApp.Data;
using BuzznetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BuzznetApp.Pages
{
    public class TestDBModel : PageModel
    {
        private readonly AppDbContext _context;

        public TestDBModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public AppFile MFile { get; private set; }
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return RedirectToPage("/Index");
            }

            MFile = await _context.File.SingleOrDefaultAsync(m => m.Id == id);

            if (MFile == null)
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }        
    }
}