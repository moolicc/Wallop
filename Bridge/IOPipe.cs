using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace WallApp.Bridge
{
    public class IOPipe
    {
        public StreamReader InputStream { get; internal set; }
        public StreamWriter OutputStream { get; internal set; }

        internal IOPipe()
        {
        }

        public void Write<T>(T payloadData)
        {
            var payload = new Payload()
            {
                TypeName = payloadData.GetType().FullName,
                Inner = Newtonsoft.Json.JsonConvert.SerializeObject(payloadData),
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
            OutputStream.WriteLine(json);
        }

        public T Read<T>()
        {
            while (InputStream.EndOfStream) System.Threading.Thread.Sleep(100);
            var json = InputStream.ReadToEnd();
            var payload = Newtonsoft.Json.JsonConvert.DeserializeObject<Payload>(json);

            var targetType = Type.GetType(payload.TypeName);
            return (T)Newtonsoft.Json.JsonConvert.DeserializeObject(payload.Inner, targetType);
        }

        public async Task<T> ReadAsync<T>()
        {
            while (InputStream.EndOfStream) System.Threading.Thread.Sleep(100);
            var json = await InputStream.ReadToEndAsync();
            var payload = Newtonsoft.Json.JsonConvert.DeserializeObject<Payload>(json);

            var targetType = Type.GetType(payload.TypeName);
            return (T)Newtonsoft.Json.JsonConvert.DeserializeObject(payload.Inner, targetType);
        }


        private class Payload
        {
            public string TypeName { get; set; }

            public string Inner { get; set; }

            public Payload()
            {
            }
        }
    }
}
