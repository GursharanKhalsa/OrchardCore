using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.Modules
{
    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides the file contents
    /// of embedded files in Module assemblies whose path is under a Module 'wwwroot' folder.
    /// </summary>
    public class ModuleEmbeddedStaticFileProvider : IFileProvider
    {
        private readonly IApplicationContext _applicationContext;

        public ModuleEmbeddedStaticFileProvider(IApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return NotFoundDirectoryContents.Singleton;
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (subpath == null)
            {
                return new NotFoundFileInfo(subpath);
            }

            var path = NormalizePath(subpath);

            var index = path.IndexOf('/');

            if (index != -1)
            {
                var application = _applicationContext.Application;
                var module = path.Substring(0, index);

                if (application.Modules.Any(m=> m.Name == module))
                {
                    var fileSubPath = Module.WebRoot + path.Substring(index + 1);

                    if (module != application.Name)
                    {
                        return application.GetModule(module).GetFileInfo(fileSubPath);
                    }

                    fileSubPath = application.Root + fileSubPath;
                    return new PhysicalFileInfo(new FileInfo(fileSubPath));
                }
            }

            return new NotFoundFileInfo(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            return NullChangeToken.Singleton;
        }

        private string NormalizePath(string path)
        {
            return path.Replace('\\', '/').Trim('/').Replace("//", "/");
        }
    }
}