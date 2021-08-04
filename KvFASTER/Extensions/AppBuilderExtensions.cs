using KvFASTER.Services;
using Microsoft.AspNetCore.Builder;

namespace KvFASTER.Extensions
{
    public static class AppBuilderExtensions
    {
        public static void UseSharedFASTERKeyValueStorage(this IApplicationBuilder builder)
        {
            var fasterKv = (SharedFASTERKVService)builder.ApplicationServices.GetService(typeof(SharedFASTERKVService));
            if (fasterKv != null)
                fasterKv.InitializeService();
        }
    }
}
