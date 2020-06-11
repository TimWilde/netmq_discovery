namespace Alpha
{
   using Microsoft.Extensions.DependencyInjection;
   using Microsoft.Extensions.Hosting;
   using Microsoft.Extensions.Logging;
   using Microsoft.Extensions.Logging.Console;
   using Services;

   public class Program
   {
      public static void Main( string[] args )
      {
         CreateHostBuilder( args ).Build().Run();
      }

      public static IHostBuilder CreateHostBuilder( string[] args ) =>
         Host.CreateDefaultBuilder( args )
             .ConfigureLogging( logging =>
                                {
                                   logging.ClearProviders();
                                   logging.AddConsole( console => console.Format = ConsoleLoggerFormat.Systemd );
                                } )
             .ConfigureServices( ( hostContext, services ) => { services.AddHostedService<AlphaService>(); } );
   }
}
