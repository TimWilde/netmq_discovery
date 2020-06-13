namespace Alpha.Infrastructure.Extensions
{
   using System.Linq;
   using NetMQ;

   public static class NetMQMessageExtensions
   {
      public static string Dump( this NetMQMessage message )
      {
         return $"({message.FrameCount}) {string.Join( ",", message.Select( frame => frame.IsEmpty ? "<empty>" : frame.ConvertToString() ) )}";
      }
   }
}
