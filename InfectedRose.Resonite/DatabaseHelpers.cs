using System.Linq;
using InfectedRose.Database;
using InfectedRose.Database.Concepts.Tables;

namespace InfectedRose.Resonite;

public static class DatabaseHelpers
{
    public static RenderComponentTable GetRenderComponent(this AccessDatabase database, int lot)
    {
        //todo: performance
        var renderers = database["RenderComponent"].Select(i => new RenderComponentTable(i));
        var registry = database["ComponentsRegistry"].Select(i => new ComponentsRegistryTable(i));

        var yippee = registry.First(i => i.id == lot && i.component_type == 2);
        var yippee2 = renderers.First(i => i.id == yippee.component_id);

        return yippee2;
    }
    
    public static PhysicsComponentTable GetPhysicsComponent(this AccessDatabase database, int lot)
    {
        //todo: performance
        var renderers = database["PhysicsComponent"].Select(i => new PhysicsComponentTable(i));
        var registry = database["ComponentsRegistry"].Select(i => new ComponentsRegistryTable(i));

        var yippee = registry.First(i => i.id == lot && i.component_type == 3);
        var yippee2 = renderers.First(i => i.id == yippee.component_id);

        return yippee2;
    }
}