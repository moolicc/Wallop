using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Engine
{
    public interface IFrameInvocable
    {
        void Update();
        void Draw();
    }
}
