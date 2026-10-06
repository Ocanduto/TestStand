using Data;

using Flurl;
using Flurl.Http;

using KreativStend.Properties;

using System.Reflection;

namespace KreativStend
{
    public class ApiClient
    {
        private static ApiClient _Instance;
        public static ApiClient Instance => _Instance ??= new ApiClient();

        public async Task<int> Create<T>(T model)
        {
            int result = -1;
            try
            {
                var path_attr = typeof(T).GetCustomAttribute<PathModelAttribute>();
                if (path_attr == null) throw new Exception("Не задан аттрибут пути");

                result = await Settings.Default.ServerURL
                    .AppendPathSegment("api")
                    .AppendPathSegment(path_attr.Path)
                    .PostJsonAsync(model)
                    .ReceiveJson<int>();
            }
            catch (Exception ex)
            {
                result = -1;
            }

            return result;
        }
    }
}
