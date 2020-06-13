namespace Alpha.Infrastructure
{
   using System.Linq;
   using NetMQ;

   public static class NetMQMessageExtensions
   {
      public static string Dump( this NetMQMessage message )
      {
         return $"{string.Join( ",", message.Select( frame => frame.IsEmpty ? "<empty>" : frame.ConvertToString() ) )}";
      }
   }
}
