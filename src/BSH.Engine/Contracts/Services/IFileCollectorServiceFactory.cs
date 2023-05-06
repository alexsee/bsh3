namespace Brightbits.BSH.Engine.Contracts.Services;
public interface IFileCollectorServiceFactory
{
    IFileCollectorService Create(IConfigurationManager configurationManager);
}
