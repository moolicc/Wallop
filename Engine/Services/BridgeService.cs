using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallApp.Bridge;
using WallApp.Bridge.Data;

namespace WallApp.Engine.Services
{
    [Service()]
    class BridgeService : InitializableService
    {
        public MessageScheduler Scheduler { get; private set; }
        private Bridge.Slave _engine;

        protected override void Initialize()
        {
            _engine = new Slave();
            Scheduler = new MessageScheduler(new Bridge.InputReader<Bridge.Data.IPayload>(_engine));
            base.Initialize();
        }
    }
}
