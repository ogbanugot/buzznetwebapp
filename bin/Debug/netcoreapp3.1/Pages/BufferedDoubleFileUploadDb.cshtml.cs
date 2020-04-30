using System;
using System.Collections.Generic;
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
    public class BufferedDoubleFileUploadDbModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly long _fileSizeLimitDB;
        private readonly long _fileSizeLimitPhy;
        private readonly string[] _permittedExtensions = { ".txt" };
        private readonly string _targetModelPath;
        private readonly string _targetDataPath;
        private readonly IList<string> paths = new List<string>();


        public BufferedDoubleFileUploadDbModel(AppDbContext context, 
            IConfiguration config)
        {
            _context = context;
            _fileSizeLimitDB = config.GetValue<long>("FileSizeLimitDB");
            _fileSizeLimitPhy = config.GetValue<long>("FileSizeLimitPhy");
            _targetModelPath = config.GetValue<string>("StoredModelPath");
            _targetDataPath = config.GetValue<string>("StoredDataPath");
        }

        [BindProperty]
        public BufferedDoubleFileUploadDb FileUpload { get; set; }

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
            //Store files in physical storage
            var formFiles = new List<IFormFile>()
            {
                FileUpload.FormFile1,
                FileUpload.FormFile2
            };

            var targetpaths = new List<string>()
            {
                _targetModelPath,
                _targetDataPath 
            };
            int i = 0;

            foreach (var formFile in formFiles)
            {
                var formFileContent =
                    await FileHelpers
                        .ProcessFormFile<BufferedDoubleFileUploadPhysical>(
                            formFile, ModelState, _permittedExtensions,
                            _fileSizeLimitPhy);

                if (!ModelState.IsValid)
                {
                    Result = "Please correct the form.";

                    return Page();
                }

                var trustedFileNameForFileStorage = Path.GetFileName(formFile.FileName);
                var filePath = Path.Combine(
                    targetpaths[i], trustedFileNameForFileStorage);                

                using (var fileStream = System.IO.File.Create(filePath))
                {
                    await fileStream.WriteAsync(formFileContent);                    
                }
                paths.Add(filePath);
                i++;
            }

            var file = new AppFile
            {
                Content = null,
                UntrustedName = paths[0],
                UntrustedNameDataset = paths[1],
                Note = FileUpload.Note,
                Size = FileUpload.FormFile1.Length,
                UploadDT = DateTime.UtcNow
            };

            _context.File.Add(file);
            await _context.SaveChangesAsync();            

            return RedirectToPage("./Index");
        }
    }

    public class BufferedDoubleFileUploadDb
    {
        [Required]
        [Display(Name="Model")]
        public IFormFile FormFile1 { get; set; }

        [Required]
        [Display(Name="Dataset")]
        public IFormFile FormFile2 { get; set; }

        [Display(Name="Note")]
        [StringLength(50, MinimumLength = 0)]
        public string Note { get; set; }
    }
}
