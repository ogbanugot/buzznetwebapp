using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using BuzznetApp.Data; 
using BuzznetApp.Models;
using Microsoft.Extensions.Configuration;

namespace BuzznetApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IFileProvider _fileProvider;
        private readonly string _targetDataPath;
        private readonly string _targetModelPath;

        public IndexModel(IConfiguration config, AppDbContext context, IFileProvider fileProvider)
        {
            _context = context;
            _fileProvider = fileProvider;            
        }

        public IList<AppFile> DatabaseFiles { get; private set; }
        public IDirectoryContents PhysicalFiles { get; private set; }
        public IDirectoryContents ModelFiles { get; private set; }
        public IDirectoryContents DataFiles { get; private set; }



        public async Task OnGetAsync()
        {
            DatabaseFiles = await _context.File.AsNoTracking().ToListAsync();
            PhysicalFiles = _fileProvider.GetDirectoryContents(string.Empty);
            ModelFiles = _fileProvider.GetDirectoryContents("\\model\\");
            DataFiles = _fileProvider.GetDirectoryContents("\\data\\");
        }

        public async Task<IActionResult> OnGetDownloadDbAsync(int? id)
        {
            if (id == null)
            {
                return Page();
            }

            var requestFile = await _context.File.SingleOrDefaultAsync(m => m.Id == id);

            if (requestFile == null)
            {
                return Page();
            }

            // Don't display the untrusted file name in the UI. HTML-encode the value.
            return File(requestFile.Content, MediaTypeNames.Application.Octet, WebUtility.HtmlEncode(requestFile.UntrustedName));
        }

        public IActionResult OnGetDownloadPhysical(string physicalPath)
        {
            //var downloadFile = _fileProvider.GetFileInfo(fileName);
            string filename = Path.GetFileName(physicalPath);
            return PhysicalFile(physicalPath, MediaTypeNames.Application.Octet, filename);
        }
    }
}
