using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager
{
    public interface IFileManagerService
    {
         Task<bool> UploadAsync(dynamic data);
        Task<string> GenerateUrl();
        Task<string> FolderExistAsync();
    }
}
