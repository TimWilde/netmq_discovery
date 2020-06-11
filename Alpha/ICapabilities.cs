namespace Alpha
{
   using System.Collections.Immutable;

   public interface ICapabilities
   {
      ImmutableArray<Capability> Capabilities { get; }
   }
}
