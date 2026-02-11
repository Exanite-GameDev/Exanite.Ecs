using System;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Queries;

namespace Exanite.Myriad.Ecs.Worlds;

internal record struct InterfaceResolverRegistration(InterfaceId Id, QueryFilter Filter, Func<ImmutableOrderedListSet<ComponentId>, object> Factory);
