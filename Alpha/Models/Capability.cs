namespace Alpha.Models
{
   using System;

   /// <summary>
   ///    Describes a <see cref="System.Type" /> supported by a service and a TCP port through which it can be accessed
   /// </summary>
   public class Capability
   {
      public string Type { get; private set; }
      public int Port { get; private set; }

      private Capability() { }

      /// <summary>
      ///    Creates a new <see cref="Capability" /> instance from a string containing a <see cref="System.Type" />'s fully
      ///    qualified name and a port number,
      ///    separated by a colon
      /// </summary>
      /// <param name="input">a string in <c>type:port</c> format</param>
      /// <returns>An immutable <see cref="Capability" /> instance describing the type and port</returns>
      public static Capability Parse( string input )
      {
         string[] parts = input.Split( ':', StringSplitOptions.RemoveEmptyEntries );
         return new Capability { Type = parts[ 0 ], Port = int.Parse( parts[ 1 ] ) };
      }

      public override string ToString()
      {
         return $"{Type}:{Port}";
      }

      // TODO: builder from type and int
   }
}
