using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Services;

namespace Brightbits.BSH.Engine.Services;
public class FileCollectorServiceFactory : IFileCollectorServiceFactory
{
    public IFileCollectorService Create(IConfigurationManager configurationManager)
    {
        return new FileCollectorService(configurationManager);
    }
}
