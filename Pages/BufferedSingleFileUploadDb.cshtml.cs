using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using BuzznetApp.Data;
using BuzznetApp.Models;
using BuzznetApp.Utilities;
using System.IO;

namespace BuzznetApp.Pages
{
    public class BufferedSingleFileUploadDbModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly long _fileSizeLimitDB;
        private readonly string _targetModelPath;
        private readonly string[] _permittedExtensions = { ".txt" };

        public BufferedSingleFileUploadDbModel(AppDbContext context, 
            IConfiguration config)
        {
            _context = context;
            _targetModelPath = config.GetValue<string>("StoredModelPath");
            _fileSizeLimitDB = config.GetValue<long>("FileSizeLimitDB");
        }

        [BindProperty]
        public BufferedSingleFileUploadDb FileUpload { get; set; }

        public string Result { get; private set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            // Perform an initial check to catch FileUpload class
            // attribute violations.
            if (!ModelState.IsValid)
            {
                Result = "Please correct the form.";

                return Page();
            }

            var formFileContent = 
                await FileHelpers.ProcessFormFile<BufferedSingleFileUploadDb>(
                    FileUpload.FormFile, ModelState, _permittedExtensions, 
                    _fileSizeLimitDB);
            
            if (!ModelState.IsValid)
            {
                Result = "Please correct the form.";

                return Page();
            }

            var trustedFileNameForFileStorage = Path.GetFileName(FileUpload.FormFile.FileName);
            var filePath = Path.Combine(
                _targetModelPath, trustedFileNameForFileStorage);            

            using (var fileStream = System.IO.File.Create(filePath))
            {
                await fileStream.WriteAsync(formFileContent);               
            }

            var file = new AppFile
            {
                Content = formFileContent,
                UntrustedName = filePath,
                UntrustedNameDataset = null,
                Note = FileUpload.Note,
                Size = FileUpload.FormFile.Length, 
                UploadDT = DateTime.UtcNow
            };

            _context.File.Add(file);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }

    public class BufferedSingleFileUploadDb
    {
        [Required]
        [Display(Name="Model")]
        public IFormFile FormFile { get; set; }

        [Display(Name="Note")]
        [StringLength(50, MinimumLength = 0)]
        public string Note { get; set; }
    }
}
