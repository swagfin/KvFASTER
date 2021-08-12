using KvFASTER.Services;
using Microsoft.AspNetCore.Builder;

namespace KvFASTER.Extensions
{
    public static class AppBuilderExtensions
    {
        public static void UseRegisteredFASTERKVServices(this IApplicationBuilder builder)
        {
            //We could have used an Enumerable DI but for testing lets just do it the hardway
            //var fasterKv = (SharedFASTERKVService)builder.ApplicationServices.GetService(typeof(SharedFASTERKVService));
            //if (fasterKv != null)
            //    fasterKv.InitializeService();

            var fasterKv2 = (YetAnotherTestingFASTERKVService)builder.ApplicationServices.GetService(typeof(YetAnotherTestingFASTERKVService));
            if (fasterKv2 != null)
                fasterKv2.InitializeService();
        }
    }
}
