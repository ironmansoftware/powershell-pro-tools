using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    public sealed class VSCodeProxyTarget
    {
        [JsonProperty("path")]
        public string[] Path { get; set; }

        [JsonProperty("handle")]
        public int? Handle { get; set; }

        [JsonProperty("typeName")]
        public string TypeName { get; set; }
    }

    public sealed class VSCodeProxyArgument
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("value")]
        public object Value { get; set; }

        [JsonProperty("handle")]
        public int? Handle { get; set; }

        [JsonProperty("path")]
        public string[] Path { get; set; }
    }

    public sealed class VSCodeProxyRequest
    {
        [JsonProperty("protocol")]
        public string Protocol { get; set; } = "vscode-api";

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("operation")]
        public string Operation { get; set; }

        [JsonProperty("target")]
        public VSCodeProxyTarget Target { get; set; }

        [JsonProperty("member")]
        public string Member { get; set; }

        [JsonProperty("arguments")]
        public VSCodeProxyArgument[] Arguments { get; set; }
    }

    public sealed class VSCodeProxyResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("result")]
        public VSCodeProxyResult Result { get; set; }
    }

    public sealed class VSCodeProxyResult
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("value")]
        public JToken Value { get; set; }

        [JsonProperty("handle")]
        public int? Handle { get; set; }

        [JsonProperty("typeName")]
        public string TypeName { get; set; }

        [JsonProperty("items")]
        public VSCodeProxyResult[] Items { get; set; }
    }

    public class VSCodeObject
    {
        internal VSCodeObject(VSCodeProxyClient client, VSCodeProxyTarget target)
        {
            Client = client;
            Target = target;
        }

        internal VSCodeProxyClient Client { get; }
        internal VSCodeProxyTarget Target { get; }

        public string ApiPath
        {
            get
            {
                if (Target?.Path == null)
                {
                    return null;
                }

                return "vscode." + string.Join(".", Target.Path);
            }
        }

        public string ApiTypeName => Target?.TypeName ?? GetType().Name;

        public int? ApiHandle => Target?.Handle;

        public object GetApiProperty(string member)
        {
            return Client.Get<object>(Target, member);
        }

        public string GetApiPropertyJson(string member)
        {
            return Client.GetRaw(Target, member);
        }

        public string TestApiProperty(string member)
        {
            try
            {
                var value = Client.Get<object>(Target, member);
                return value == null ? "<null>" : $"{value} ({value.GetType().FullName})";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public override string ToString()
        {
            if (ApiPath != null)
            {
                return ApiPath;
            }

            return ApiTypeName ?? base.ToString();
        }

        protected T Get<T>(string member)
        {
            return Client.Get<T>(Target, member);
        }

        protected T Invoke<T>(string member, params object[] arguments)
        {
            return Client.Invoke<T>(Target, member, arguments);
        }

        protected void Invoke(string member, params object[] arguments)
        {
            Client.Invoke<object>(Target, member, arguments);
        }

        protected T Construct<T>(params object[] arguments)
        {
            return Client.Construct<T>(Target, arguments);
        }
    }

    public sealed class VSCodeUnknownObject : VSCodeObject
    {
        internal VSCodeUnknownObject(VSCodeProxyClient client, VSCodeProxyTarget target)
            : base(client, target)
        {
        }

        public object Get(string member)
        {
            return Client.Get<object>(Target, member);
        }

        public new object Invoke(string member, params object[] arguments)
        {
            return Client.Invoke<object>(Target, member, arguments);
        }
    }

    public sealed class VSCodeProxyClient
    {
        private static readonly Lazy<VSCodeProxyClient> DefaultClient = new Lazy<VSCodeProxyClient>(() => new VSCodeProxyClient());
        private static readonly int ProcessId = Process.GetCurrentProcess().Id;

        public static VSCodeProxyClient Default => DefaultClient.Value;

        public T Get<T>(VSCodeProxyTarget target, string member)
        {
            return Send<T>("get", target, member, Array.Empty<object>());
        }

        public string GetRaw(VSCodeProxyTarget target, string member)
        {
            var request = new VSCodeProxyRequest
            {
                Id = Guid.NewGuid().ToString("N"),
                Operation = "get",
                Target = target,
                Member = member,
                Arguments = Array.Empty<VSCodeProxyArgument>()
            };

            return SendRequest(request);
        }

        public T Invoke<T>(VSCodeProxyTarget target, string member, params object[] arguments)
        {
            return Send<T>("invoke", target, member, arguments);
        }

        public T Construct<T>(VSCodeProxyTarget target, params object[] arguments)
        {
            return Send<T>("construct", target, null, arguments);
        }

        internal static VSCodeProxyTarget Path(params string[] path)
        {
            return new VSCodeProxyTarget { Path = path };
        }

        private T Send<T>(string operation, VSCodeProxyTarget target, string member, object[] arguments)
        {
            var request = new VSCodeProxyRequest
            {
                Id = Guid.NewGuid().ToString("N"),
                Operation = operation,
                Target = target,
                Member = member,
                Arguments = (arguments ?? Array.Empty<object>()).Select(ConvertArgument).ToArray()
            };

            var responseJson = SendRequest(request);
            var response = JsonConvert.DeserializeObject<VSCodeProxyResponse>(responseJson);
            if (response == null)
            {
                throw new InvalidOperationException("VS Code API returned an empty response.");
            }

            if (!response.Success)
            {
                throw new InvalidOperationException(response.Error ?? "VS Code API request failed.");
            }

            return ConvertResult<T>(response.Result);
        }

        private static string SendRequest(VSCodeProxyRequest request)
        {
            using (var namedPipeClient = new NamedPipeClientStream(".", $"PPTPipeCode{ProcessId}", PipeDirection.InOut))
            {
                namedPipeClient.Connect(5000);

                var requestBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request) + "!PS");
                namedPipeClient.Write(requestBytes, 0, requestBytes.Length);
                namedPipeClient.Flush();

                try
                {
                    namedPipeClient.WaitForPipeDrain();
                }
                catch (PlatformNotSupportedException)
                {
                }
                catch (IOException)
                {
                    // The VS Code side may close immediately after reading the request.
                }

                return ReadResponse(namedPipeClient);
            }
        }

        private static string ReadResponse(NamedPipeClientStream namedPipeClient)
        {
            var buffer = new byte[4096];
            using (var memoryStream = new MemoryStream())
            {
                while (true)
                {
                    int read;
                    try
                    {
                        var readTask = namedPipeClient.ReadAsync(buffer, 0, buffer.Length);
                        if (!readTask.Wait(10000))
                        {
                            throw new TimeoutException("Timed out waiting for a response from the VS Code API bridge.");
                        }

                        read = readTask.Result;
                    }
                    catch (IOException)
                    {
                        if (memoryStream.Length > 0)
                        {
                            break;
                        }

                        throw;
                    }
                    catch (ObjectDisposedException)
                    {
                        if (memoryStream.Length > 0)
                        {
                            break;
                        }

                        throw;
                    }

                    if (read == 0)
                    {
                        break;
                    }

                    memoryStream.Write(buffer, 0, read);
                }

                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }

        private static VSCodeProxyArgument ConvertArgument(object value)
        {
            if (value == null)
            {
                return new VSCodeProxyArgument { Kind = "null" };
            }

            if (value is VSCodeObject vscodeObject)
            {
                return new VSCodeProxyArgument
                {
                    Kind = "target",
                    Handle = vscodeObject.Target.Handle,
                    Path = vscodeObject.Target.Path,
                    Value = vscodeObject.Target.TypeName
                };
            }

            if (value is Enum)
            {
                return new VSCodeProxyArgument { Kind = "primitive", Value = Convert.ToInt32(value) };
            }

            return new VSCodeProxyArgument { Kind = "value", Value = value };
        }

        private T ConvertResult<T>(VSCodeProxyResult result)
        {
            if (result == null || result.Kind == "null" || result.Kind == "undefined")
            {
                return default(T);
            }

            var targetType = typeof(T);
            if (targetType == typeof(void))
            {
                return default(T);
            }

            if (result.Kind == "array")
            {
                return ConvertArray<T>(result);
            }

            if (result.Kind == "handle")
            {
                return ConvertHandle<T>(result);
            }

            if (targetType == typeof(object))
            {
                return (T)ConvertToObject(result);
            }

            if (result.Value == null)
            {
                return default(T);
            }

            return result.Value.ToObject<T>();
        }

        private T ConvertArray<T>(VSCodeProxyResult result)
        {
            var targetType = typeof(T);
            if (!targetType.IsArray)
            {
                if (targetType == typeof(object))
                {
                    return (T)(object)result.Items.Select(ConvertToObject).ToArray();
                }

                return default(T);
            }

            var elementType = targetType.GetElementType();
            var items = result.Items ?? Array.Empty<VSCodeProxyResult>();
            var array = Array.CreateInstance(elementType, items.Length);
            var method = typeof(VSCodeProxyClient).GetMethod(nameof(ConvertResult), BindingFlags.Instance | BindingFlags.NonPublic);
            var generic = method.MakeGenericMethod(elementType);

            for (var index = 0; index < items.Length; index++)
            {
                array.SetValue(generic.Invoke(this, new object[] { items[index] }), index);
            }

            return (T)(object)array;
        }

        private T ConvertHandle<T>(VSCodeProxyResult result)
        {
            var target = new VSCodeProxyTarget
            {
                Handle = result.Handle,
                TypeName = result.TypeName
            };

            var targetType = typeof(T);
            if (targetType == typeof(object) || targetType == typeof(VSCodeUnknownObject))
            {
                return (T)(object)new VSCodeUnknownObject(this, target);
            }

            if (!typeof(VSCodeObject).IsAssignableFrom(targetType))
            {
                return default(T);
            }

            return (T)Activator.CreateInstance(targetType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object[] { this, target }, null);
        }

        private object ConvertToObject(VSCodeProxyResult result)
        {
            if (result == null || result.Kind == "null" || result.Kind == "undefined")
            {
                return null;
            }

            if (result.Kind == "array")
            {
                return (result.Items ?? Array.Empty<VSCodeProxyResult>()).Select(ConvertToObject).ToArray();
            }

            if (result.Kind == "handle")
            {
                return ConvertHandle<VSCodeUnknownObject>(result);
            }

            return result.Value?.ToObject<object>();
        }
    }

    public static class VSCodeApiBootstrap
    {
        public static object Create()
        {
            var apiType = typeof(VSCodeProxyClient).Assembly.GetType("PowerShellToolsPro.Cmdlets.VSCode.VSCodeApi");
            if (apiType == null)
            {
                throw new InvalidOperationException("The generated VS Code API surface was not found.");
            }

            return Activator.CreateInstance(apiType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object[] { VSCodeProxyClient.Default }, null);
        }
    }
}
