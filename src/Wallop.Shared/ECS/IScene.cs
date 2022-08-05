using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallop.Shared.ECS
{
    public interface IScene
    {
        string Name { get; }
        List<ILayout> Layouts { get; }
        List<IDirector> Directors { get; }

        IEnumerable<ILayout> GetActiveLayouts();

        void Shutdown();
        void Update();
        void Draw();
    }
}
