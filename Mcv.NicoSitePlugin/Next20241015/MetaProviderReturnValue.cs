using NicoSitePlugin.Metadata;
using System;

namespace Mcv.NicoSitePlugin.Next20241015;

interface IMetaProviderReturnValue
{

}
class MetaProviderReturnValueErrorMessage(string message) : IMetaProviderReturnValue
{
    public string Message { get; } = message;
}
class MetaProviderReturnValueException(Exception ex) : IMetaProviderReturnValue
{
    public Exception Exception { get; } = ex;
}
class MetaProviderReturnValueMessage(IMetaMessage message) : IMetaProviderReturnValue
{
    public IMetaMessage Message { get; } = message;
}
