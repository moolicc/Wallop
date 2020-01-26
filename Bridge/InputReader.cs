using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace WallApp.Bridge
{
    public class InputReader<T>
    {
        public IOPipe Pipe { get; private set; }

        public ConcurrentQueue<T> Queue { get; private set; }

        public InputReader(IOPipe pipe)
        {
            Pipe = pipe;
            Queue = new ConcurrentQueue<T>();
            Run();
        }

        private void Run()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    var data = Pipe.Read<T>();
                    Queue.Enqueue(data);
                }
            });
        }
    }
}
