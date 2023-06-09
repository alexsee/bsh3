using Brightbits.BSH.Engine.Contracts.Storage;
using Brightbits.BSH.Engine.Storage;

namespace BSH.Test.Mocks;
public class StorageFactoryMock : IStorageFactory
{
    public IStorage GetCurrentStorageProvider() => new StorageMock();
}
