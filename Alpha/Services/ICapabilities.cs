namespace Alpha.Services
{
   using System.Collections.Immutable;
   using Models;

   public interface ICapabilities
   {
      ImmutableArray<Capability> Capabilities { get; }
   }
}
