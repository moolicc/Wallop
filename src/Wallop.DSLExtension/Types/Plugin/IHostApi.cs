using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.DSLExtension.Scripting;

namespace Wallop.DSLExtension.Types.Plugin
{
    public interface IHostApi
    {
        string Name { get; }

        void Use(IScriptContext scriptContext);
        void BeforeUpdate(IScriptContext scriptContext, double delta);
        void AfterUpdate(IScriptContext scriptContext);
        void BeforeDraw(IScriptContext scriptContext, double delta);
        void AfterDraw(IScriptContext scriptContext);
    }
}
