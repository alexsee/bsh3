using Brightbits.BSH.Engine.Contracts.Services;

namespace Brightbits.BSH.Engine.Services.FileCollector;
public class FileCollectorServiceFactory : IFileCollectorServiceFactory
{
    public IFileCollectorService Create()
    {
        return new FileCollectorService();
    }
}
