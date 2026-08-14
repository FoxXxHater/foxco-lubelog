using CarCareTracker.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace CarCareTracker.Controllers
{
    public class SharedDataController : Controller
    {
        private readonly IFileHelper _fileHelper;
        private readonly IConfigHelper _config;
        private readonly FileExtensionContentTypeProvider _mimeTypeProvider;
        public SharedDataController(IFileHelper fileHelper, IConfigHelper config)
        {
            _fileHelper = fileHelper;
            _config = config;
            _mimeTypeProvider = new FileExtensionContentTypeProvider();
        }
        [AllowAnonymous]
        [Route("/css/theme.css")]
        public IActionResult GetConfiguredTheme()
        {
            string uiTheme = string.Empty;
            string themeContent = string.Empty;
            try
            {
                var userConfig = _config.GetUserConfig(User);
                if (string.IsNullOrWhiteSpace(userConfig.UserTheme))
                {
                    uiTheme = _config.GetServerTheme();
                } else
                {
                    uiTheme = userConfig.UserTheme;
                }
            }
            catch
            {
                uiTheme = _config.GetServerTheme();
            }
            if (!string.IsNullOrWhiteSpace(uiTheme))
            {
                themeContent = _fileHelper.GetTheme(uiTheme);
            }
            return Content(themeContent, "text/css");
        }
        [Authorize]
        [Route("/images/{fileName}")]
        [Route("/documents/{fileName}")]
        [Route("/translations/{fileName}")]
        [Route("/temp/{fileName}")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult GetStaticFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return NotFound();
            }
            var fullFilePath = _fileHelper.GetFullFilePath(Request.Path);
            if (!string.IsNullOrWhiteSpace(fullFilePath))
            {
                var fileBytes = _fileHelper.GetFileBytes(fullFilePath);
                if (_mimeTypeProvider.TryGetContentType(fileName, out string? contentType))
                {
                    return File(fileBytes, contentType);
                }
                return File(fileBytes, "application/octet-stream");
            }
            return NotFound();
        }
    }
}
