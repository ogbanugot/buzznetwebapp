using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.FileProviders;

namespace BuzznetApp.Pages
{
    public class TestPhysicalModel : PageModel
    {
        private readonly IFileProvider _fileProvider;

        public TestPhysicalModel(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
            DataFiles = _fileProvider.GetDirectoryContents("\\data\\");
        }
        public IFileInfo MFile { get; private set; }

        public IDirectoryContents DataFiles { get; private set; }

        public IActionResult OnGet(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return RedirectToPage("/Index");
            }
            fileName = Path.Combine("\\model\\", fileName);
            MFile = _fileProvider.GetFileInfo(fileName);

            if (!MFile.Exists)
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }
    }
}