using System;
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
            int peeked = -1;
            while (peeked == -1)
            {
                System.Threading.Thread.Sleep(100);
                try
                {
                    peeked = InputStream.Peek();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            int next = InputStream.Read();
            string json = "";
            while (next != -1 && ((char)next) != '\0')
            {
                json += (char)next;
                peeked = InputStream.Peek();
                if (peeked != -1)
                {
                    next = InputStream.Read();
                }
                else
                {
                    break;
                }
            }

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
