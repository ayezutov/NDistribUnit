using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace NDistribUnit.Server.Services
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    internal class DashboardService : IDashboardService
    {
        private static readonly IDictionary<string, string> allowed =
            new Dictionary<string, string>
                {
                    {".html", "text/html"},
                    {".htm", "text/html"},
                    {".js", "text/javascript"},
                    {".css", "text/css"},
                    {".png", "image/png"},
                    {".gif", "image/gif"},
                    {".jpg", "image/jpeg"}
                };

        public ProjectDescription[] GetProjectList()
        {
            return new []
                       {
                           new ProjectDescription
                               {
                                   Name = "O2I Nightly",
                                   UniqueIdentifier = Guid.NewGuid().ToString()
                               },
                           new ProjectDescription
                               {
                                   Name = "O2I Build for 6.0",
                                   UniqueIdentifier = Guid.NewGuid().ToString()
                               },
                           new ProjectDescription
                               {
                                   Name = "O2I Build for 5.2",
                                   UniqueIdentifier = Guid.NewGuid().ToString()
                               },
                           new ProjectDescription
                               {
                                   Name = "BDB Nightly",
                                   UniqueIdentifier = Guid.NewGuid().ToString()
                               },
                       };
        }

        public Stream Get(string fileName)
        {
            Debug.Assert(WebOperationContext.Current != null);

            var physicalPathToFile =
                Path.Combine(
                    Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetEntryAssembly().CodeBase).AbsolutePath),
                                 "../../dashboard"), fileName);
            var response = WebOperationContext.Current.OutgoingResponse;

            if (!File.Exists(physicalPathToFile))
            {
                response.SetStatusAsNotFound();
                return new MemoryStream(Encoding.UTF8.GetBytes("Not found"));
            }

            var extension = Path.GetExtension(physicalPathToFile);
            if (!IsAllowedExtension(extension))
            {
                response.StatusCode = HttpStatusCode.Forbidden;
                return new MemoryStream(Encoding.UTF8.GetBytes("Forbidden"));
            }

            response.ContentType = allowed[extension];
            return new FileStream(physicalPathToFile, FileMode.Open, FileAccess.Read);
        }

        private static bool IsAllowedExtension(string extension)
        {
            return allowed.Keys.Contains(extension);
        }
    }
}