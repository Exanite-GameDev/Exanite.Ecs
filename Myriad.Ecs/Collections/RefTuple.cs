
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedParameter.Global
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable CheckNamespace
// ReSharper disable ArrangeAccessorOwnerBody

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Myriad.Ecs.Collections;

public readonly ref struct RefT<T>
{
	private readonly ref T value;

	public ref T Value
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref value;
	}

	internal RefT(ref T r)
	{
		value = ref r;
	}
}



public readonly ref struct RefTuple<T0>
{
	public readonly Worlds.Entity Entity;

	private readonly RefT<T0> item0;
	public ref T0 Item0
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item0.Value;
	}

	internal RefTuple(Worlds.Entity entity, RefT<T0> item0)
	{
		Entity = entity;
		this.item0 = item0;
	}

	public void Deconstruct(out Worlds.Entity entity, out RefT<T0> item0)
	{
		entity = Entity;
		item0 = this.item0;
	}
}


public readonly ref struct RefTuple<T0, T1>
{
	public readonly Worlds.Entity Entity;

	private readonly RefT<T0> item0;
	public ref T0 Item0
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item0.Value;
	}

	private readonly RefT<T1> item1;
	public ref T1 Item1
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item1.Value;
	}

	internal RefTuple(Worlds.Entity entity, RefT<T0> item0, RefT<T1> item1)
	{
		Entity = entity;
		this.item0 = item0;
		this.item1 = item1;
	}

	public void Deconstruct(out Worlds.Entity entity, out RefT<T0> item0, out RefT<T1> item1)
	{
		entity = Entity;
		item0 = this.item0;
		item1 = this.item1;
	}
}

[ExcludeFromCodeCoverage]
public readonly ref struct RefTuple<T0, T1, T2>
{
	public readonly Worlds.Entity Entity;

	private readonly RefT<T0> item0;
	public ref T0 Item0
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item0.Value;
	}

	private readonly RefT<T1> item1;
	public ref T1 Item1
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item1.Value;
	}

	private readonly RefT<T2> item2;
	public ref T2 Item2
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item2.Value;
	}

	internal RefTuple(Worlds.Entity entity, RefT<T0> item0, RefT<T1> item1, RefT<T2> item2)
	{
		Entity = entity;
		this.item0 = item0;
		this.item1 = item1;
		this.item2 = item2;
	}

	public void Deconstruct(out Worlds.Entity entity, out RefT<T0> item0, out RefT<T1> item1, out RefT<T2> item2)
	{
		entity = Entity;
		item0 = this.item0;
		item1 = this.item1;
		item2 = this.item2;
	}
}

[ExcludeFromCodeCoverage]
public readonly ref struct RefTuple<T0, T1, T2, T3>
{
	public readonly Worlds.Entity Entity;

	private readonly RefT<T0> item0;
	public ref T0 Item0
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item0.Value;
	}

	private readonly RefT<T1> item1;
	public ref T1 Item1
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item1.Value;
	}

	private readonly RefT<T2> item2;
	public ref T2 Item2
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item2.Value;
	}

	private readonly RefT<T3> item3;
	public ref T3 Item3
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item3.Value;
	}

	internal RefTuple(Worlds.Entity entity, RefT<T0> item0, RefT<T1> item1, RefT<T2> item2, RefT<T3> item3)
	{
		Entity = entity;
		this.item0 = item0;
		this.item1 = item1;
		this.item2 = item2;
		this.item3 = item3;
	}

	public void Deconstruct(out Worlds.Entity entity, out RefT<T0> item0, out RefT<T1> item1, out RefT<T2> item2, out RefT<T3> item3)
	{
		entity = Entity;
		item0 = this.item0;
		item1 = this.item1;
		item2 = this.item2;
		item3 = this.item3;
	}
}

[ExcludeFromCodeCoverage]
public readonly ref struct RefTuple<T0, T1, T2, T3, T4>
{
	public readonly Worlds.Entity Entity;

	private readonly RefT<T0> item0;
	public ref T0 Item0
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item0.Value;
	}

	private readonly RefT<T1> item1;
	public ref T1 Item1
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item1.Value;
	}

	private readonly RefT<T2> item2;
	public ref T2 Item2
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item2.Value;
	}

	private readonly RefT<T3> item3;
	public ref T3 Item3
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item3.Value;
	}

	private readonly RefT<T4> item4;
	public ref T4 Item4
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item4.Value;
	}

	internal RefTuple(Worlds.Entity entity, RefT<T0> item0, RefT<T1> item1, RefT<T2> item2, RefT<T3> item3, RefT<T4> item4)
	{
		Entity = entity;
		this.item0 = item0;
		this.item1 = item1;
		this.item2 = item2;
		this.item3 = item3;
		this.item4 = item4;
	}

	public void Deconstruct(out Worlds.Entity entity, out RefT<T0> item0, out RefT<T1> item1, out RefT<T2> item2, out RefT<T3> item3, out RefT<T4> item4)
	{
		entity = Entity;
		item0 = this.item0;
		item1 = this.item1;
		item2 = this.item2;
		item3 = this.item3;
		item4 = this.item4;
	}
}

[ExcludeFromCodeCoverage]
public readonly ref struct RefTuple<T0, T1, T2, T3, T4, T5>
{
	public readonly Worlds.Entity Entity;

	private readonly RefT<T0> item0;
	public ref T0 Item0
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item0.Value;
	}

	private readonly RefT<T1> item1;
	public ref T1 Item1
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item1.Value;
	}

	private readonly RefT<T2> item2;
	public ref T2 Item2
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item2.Value;
	}

	private readonly RefT<T3> item3;
	public ref T3 Item3
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item3.Value;
	}

	private readonly RefT<T4> item4;
	public ref T4 Item4
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item4.Value;
	}

	private readonly RefT<T5> item5;
	public ref T5 Item5
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item5.Value;
	}

	internal RefTuple(Worlds.Entity entity, RefT<T0> item0, RefT<T1> item1, RefT<T2> item2, RefT<T3> item3, RefT<T4> item4, RefT<T5> item5)
	{
		Entity = entity;
		this.item0 = item0;
		this.item1 = item1;
		this.item2 = item2;
		this.item3 = item3;
		this.item4 = item4;
		this.item5 = item5;
	}

	public void Deconstruct(out Worlds.Entity entity, out RefT<T0> item0, out RefT<T1> item1, out RefT<T2> item2, out RefT<T3> item3, out RefT<T4> item4, out RefT<T5> item5)
	{
		entity = Entity;
		item0 = this.item0;
		item1 = this.item1;
		item2 = this.item2;
		item3 = this.item3;
		item4 = this.item4;
		item5 = this.item5;
	}
}

[ExcludeFromCodeCoverage]
public readonly ref struct RefTuple<T0, T1, T2, T3, T4, T5, T6>
{
	public readonly Worlds.Entity Entity;

	private readonly RefT<T0> item0;
	public ref T0 Item0
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item0.Value;
	}

	private readonly RefT<T1> item1;
	public ref T1 Item1
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item1.Value;
	}

	private readonly RefT<T2> item2;
	public ref T2 Item2
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item2.Value;
	}

	private readonly RefT<T3> item3;
	public ref T3 Item3
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item3.Value;
	}

	private readonly RefT<T4> item4;
	public ref T4 Item4
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item4.Value;
	}

	private readonly RefT<T5> item5;
	public ref T5 Item5
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item5.Value;
	}

	private readonly RefT<T6> item6;
	public ref T6 Item6
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item6.Value;
	}

	internal RefTuple(Worlds.Entity entity, RefT<T0> item0, RefT<T1> item1, RefT<T2> item2, RefT<T3> item3, RefT<T4> item4, RefT<T5> item5, RefT<T6> item6)
	{
		Entity = entity;
		this.item0 = item0;
		this.item1 = item1;
		this.item2 = item2;
		this.item3 = item3;
		this.item4 = item4;
		this.item5 = item5;
		this.item6 = item6;
	}

	public void Deconstruct(out Worlds.Entity entity, out RefT<T0> item0, out RefT<T1> item1, out RefT<T2> item2, out RefT<T3> item3, out RefT<T4> item4, out RefT<T5> item5, out RefT<T6> item6)
	{
		entity = Entity;
		item0 = this.item0;
		item1 = this.item1;
		item2 = this.item2;
		item3 = this.item3;
		item4 = this.item4;
		item5 = this.item5;
		item6 = this.item6;
	}
}

[ExcludeFromCodeCoverage]
public readonly ref struct RefTuple<T0, T1, T2, T3, T4, T5, T6, T7>
{
	public readonly Worlds.Entity Entity;

	private readonly RefT<T0> item0;
	public ref T0 Item0
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item0.Value;
	}

	private readonly RefT<T1> item1;
	public ref T1 Item1
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item1.Value;
	}

	private readonly RefT<T2> item2;
	public ref T2 Item2
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item2.Value;
	}

	private readonly RefT<T3> item3;
	public ref T3 Item3
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item3.Value;
	}

	private readonly RefT<T4> item4;
	public ref T4 Item4
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item4.Value;
	}

	private readonly RefT<T5> item5;
	public ref T5 Item5
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item5.Value;
	}

	private readonly RefT<T6> item6;
	public ref T6 Item6
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item6.Value;
	}

	private readonly RefT<T7> item7;
	public ref T7 Item7
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item7.Value;
	}

	internal RefTuple(Worlds.Entity entity, RefT<T0> item0, RefT<T1> item1, RefT<T2> item2, RefT<T3> item3, RefT<T4> item4, RefT<T5> item5, RefT<T6> item6, RefT<T7> item7)
	{
		Entity = entity;
		this.item0 = item0;
		this.item1 = item1;
		this.item2 = item2;
		this.item3 = item3;
		this.item4 = item4;
		this.item5 = item5;
		this.item6 = item6;
		this.item7 = item7;
	}

	public void Deconstruct(out Worlds.Entity entity, out RefT<T0> item0, out RefT<T1> item1, out RefT<T2> item2, out RefT<T3> item3, out RefT<T4> item4, out RefT<T5> item5, out RefT<T6> item6, out RefT<T7> item7)
	{
		entity = Entity;
		item0 = this.item0;
		item1 = this.item1;
		item2 = this.item2;
		item3 = this.item3;
		item4 = this.item4;
		item5 = this.item5;
		item6 = this.item6;
		item7 = this.item7;
	}
}

[ExcludeFromCodeCoverage]
public readonly ref struct RefTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8>
{
	public readonly Worlds.Entity Entity;

	private readonly RefT<T0> item0;
	public ref T0 Item0
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item0.Value;
	}

	private readonly RefT<T1> item1;
	public ref T1 Item1
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item1.Value;
	}

	private readonly RefT<T2> item2;
	public ref T2 Item2
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item2.Value;
	}

	private readonly RefT<T3> item3;
	public ref T3 Item3
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item3.Value;
	}

	private readonly RefT<T4> item4;
	public ref T4 Item4
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item4.Value;
	}

	private readonly RefT<T5> item5;
	public ref T5 Item5
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item5.Value;
	}

	private readonly RefT<T6> item6;
	public ref T6 Item6
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item6.Value;
	}

	private readonly RefT<T7> item7;
	public ref T7 Item7
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item7.Value;
	}

	private readonly RefT<T8> item8;
	public ref T8 Item8
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item8.Value;
	}

	internal RefTuple(Worlds.Entity entity, RefT<T0> item0, RefT<T1> item1, RefT<T2> item2, RefT<T3> item3, RefT<T4> item4, RefT<T5> item5, RefT<T6> item6, RefT<T7> item7, RefT<T8> item8)
	{
		Entity = entity;
		this.item0 = item0;
		this.item1 = item1;
		this.item2 = item2;
		this.item3 = item3;
		this.item4 = item4;
		this.item5 = item5;
		this.item6 = item6;
		this.item7 = item7;
		this.item8 = item8;
	}

	public void Deconstruct(out Worlds.Entity entity, out RefT<T0> item0, out RefT<T1> item1, out RefT<T2> item2, out RefT<T3> item3, out RefT<T4> item4, out RefT<T5> item5, out RefT<T6> item6, out RefT<T7> item7, out RefT<T8> item8)
	{
		entity = Entity;
		item0 = this.item0;
		item1 = this.item1;
		item2 = this.item2;
		item3 = this.item3;
		item4 = this.item4;
		item5 = this.item5;
		item6 = this.item6;
		item7 = this.item7;
		item8 = this.item8;
	}
}

[ExcludeFromCodeCoverage]
public readonly ref struct RefTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>
{
	public readonly Worlds.Entity Entity;

	private readonly RefT<T0> item0;
	public ref T0 Item0
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item0.Value;
	}

	private readonly RefT<T1> item1;
	public ref T1 Item1
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item1.Value;
	}

	private readonly RefT<T2> item2;
	public ref T2 Item2
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item2.Value;
	}

	private readonly RefT<T3> item3;
	public ref T3 Item3
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item3.Value;
	}

	private readonly RefT<T4> item4;
	public ref T4 Item4
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item4.Value;
	}

	private readonly RefT<T5> item5;
	public ref T5 Item5
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item5.Value;
	}

	private readonly RefT<T6> item6;
	public ref T6 Item6
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item6.Value;
	}

	private readonly RefT<T7> item7;
	public ref T7 Item7
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item7.Value;
	}

	private readonly RefT<T8> item8;
	public ref T8 Item8
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item8.Value;
	}

	private readonly RefT<T9> item9;
	public ref T9 Item9
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item9.Value;
	}

	internal RefTuple(Worlds.Entity entity, RefT<T0> item0, RefT<T1> item1, RefT<T2> item2, RefT<T3> item3, RefT<T4> item4, RefT<T5> item5, RefT<T6> item6, RefT<T7> item7, RefT<T8> item8, RefT<T9> item9)
	{
		Entity = entity;
		this.item0 = item0;
		this.item1 = item1;
		this.item2 = item2;
		this.item3 = item3;
		this.item4 = item4;
		this.item5 = item5;
		this.item6 = item6;
		this.item7 = item7;
		this.item8 = item8;
		this.item9 = item9;
	}

	public void Deconstruct(out Worlds.Entity entity, out RefT<T0> item0, out RefT<T1> item1, out RefT<T2> item2, out RefT<T3> item3, out RefT<T4> item4, out RefT<T5> item5, out RefT<T6> item6, out RefT<T7> item7, out RefT<T8> item8, out RefT<T9> item9)
	{
		entity = Entity;
		item0 = this.item0;
		item1 = this.item1;
		item2 = this.item2;
		item3 = this.item3;
		item4 = this.item4;
		item5 = this.item5;
		item6 = this.item6;
		item7 = this.item7;
		item8 = this.item8;
		item9 = this.item9;
	}
}

[ExcludeFromCodeCoverage]
public readonly ref struct RefTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
{
	public readonly Worlds.Entity Entity;

	private readonly RefT<T0> item0;
	public ref T0 Item0
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item0.Value;
	}

	private readonly RefT<T1> item1;
	public ref T1 Item1
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item1.Value;
	}

	private readonly RefT<T2> item2;
	public ref T2 Item2
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item2.Value;
	}

	private readonly RefT<T3> item3;
	public ref T3 Item3
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item3.Value;
	}

	private readonly RefT<T4> item4;
	public ref T4 Item4
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item4.Value;
	}

	private readonly RefT<T5> item5;
	public ref T5 Item5
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item5.Value;
	}

	private readonly RefT<T6> item6;
	public ref T6 Item6
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item6.Value;
	}

	private readonly RefT<T7> item7;
	public ref T7 Item7
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item7.Value;
	}

	private readonly RefT<T8> item8;
	public ref T8 Item8
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item8.Value;
	}

	private readonly RefT<T9> item9;
	public ref T9 Item9
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item9.Value;
	}

	private readonly RefT<T10> item10;
	public ref T10 Item10
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item10.Value;
	}

	internal RefTuple(Worlds.Entity entity, RefT<T0> item0, RefT<T1> item1, RefT<T2> item2, RefT<T3> item3, RefT<T4> item4, RefT<T5> item5, RefT<T6> item6, RefT<T7> item7, RefT<T8> item8, RefT<T9> item9, RefT<T10> item10)
	{
		Entity = entity;
		this.item0 = item0;
		this.item1 = item1;
		this.item2 = item2;
		this.item3 = item3;
		this.item4 = item4;
		this.item5 = item5;
		this.item6 = item6;
		this.item7 = item7;
		this.item8 = item8;
		this.item9 = item9;
		this.item10 = item10;
	}

	public void Deconstruct(out Worlds.Entity entity, out RefT<T0> item0, out RefT<T1> item1, out RefT<T2> item2, out RefT<T3> item3, out RefT<T4> item4, out RefT<T5> item5, out RefT<T6> item6, out RefT<T7> item7, out RefT<T8> item8, out RefT<T9> item9, out RefT<T10> item10)
	{
		entity = Entity;
		item0 = this.item0;
		item1 = this.item1;
		item2 = this.item2;
		item3 = this.item3;
		item4 = this.item4;
		item5 = this.item5;
		item6 = this.item6;
		item7 = this.item7;
		item8 = this.item8;
		item9 = this.item9;
		item10 = this.item10;
	}
}

[ExcludeFromCodeCoverage]
public readonly ref struct RefTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
{
	public readonly Worlds.Entity Entity;

	private readonly RefT<T0> item0;
	public ref T0 Item0
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item0.Value;
	}

	private readonly RefT<T1> item1;
	public ref T1 Item1
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item1.Value;
	}

	private readonly RefT<T2> item2;
	public ref T2 Item2
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item2.Value;
	}

	private readonly RefT<T3> item3;
	public ref T3 Item3
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item3.Value;
	}

	private readonly RefT<T4> item4;
	public ref T4 Item4
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item4.Value;
	}

	private readonly RefT<T5> item5;
	public ref T5 Item5
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item5.Value;
	}

	private readonly RefT<T6> item6;
	public ref T6 Item6
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item6.Value;
	}

	private readonly RefT<T7> item7;
	public ref T7 Item7
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item7.Value;
	}

	private readonly RefT<T8> item8;
	public ref T8 Item8
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item8.Value;
	}

	private readonly RefT<T9> item9;
	public ref T9 Item9
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item9.Value;
	}

	private readonly RefT<T10> item10;
	public ref T10 Item10
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item10.Value;
	}

	private readonly RefT<T11> item11;
	public ref T11 Item11
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item11.Value;
	}

	internal RefTuple(Worlds.Entity entity, RefT<T0> item0, RefT<T1> item1, RefT<T2> item2, RefT<T3> item3, RefT<T4> item4, RefT<T5> item5, RefT<T6> item6, RefT<T7> item7, RefT<T8> item8, RefT<T9> item9, RefT<T10> item10, RefT<T11> item11)
	{
		Entity = entity;
		this.item0 = item0;
		this.item1 = item1;
		this.item2 = item2;
		this.item3 = item3;
		this.item4 = item4;
		this.item5 = item5;
		this.item6 = item6;
		this.item7 = item7;
		this.item8 = item8;
		this.item9 = item9;
		this.item10 = item10;
		this.item11 = item11;
	}

	public void Deconstruct(out Worlds.Entity entity, out RefT<T0> item0, out RefT<T1> item1, out RefT<T2> item2, out RefT<T3> item3, out RefT<T4> item4, out RefT<T5> item5, out RefT<T6> item6, out RefT<T7> item7, out RefT<T8> item8, out RefT<T9> item9, out RefT<T10> item10, out RefT<T11> item11)
	{
		entity = Entity;
		item0 = this.item0;
		item1 = this.item1;
		item2 = this.item2;
		item3 = this.item3;
		item4 = this.item4;
		item5 = this.item5;
		item6 = this.item6;
		item7 = this.item7;
		item8 = this.item8;
		item9 = this.item9;
		item10 = this.item10;
		item11 = this.item11;
	}
}

[ExcludeFromCodeCoverage]
public readonly ref struct RefTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
{
	public readonly Worlds.Entity Entity;

	private readonly RefT<T0> item0;
	public ref T0 Item0
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item0.Value;
	}

	private readonly RefT<T1> item1;
	public ref T1 Item1
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item1.Value;
	}

	private readonly RefT<T2> item2;
	public ref T2 Item2
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item2.Value;
	}

	private readonly RefT<T3> item3;
	public ref T3 Item3
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item3.Value;
	}

	private readonly RefT<T4> item4;
	public ref T4 Item4
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item4.Value;
	}

	private readonly RefT<T5> item5;
	public ref T5 Item5
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item5.Value;
	}

	private readonly RefT<T6> item6;
	public ref T6 Item6
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item6.Value;
	}

	private readonly RefT<T7> item7;
	public ref T7 Item7
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item7.Value;
	}

	private readonly RefT<T8> item8;
	public ref T8 Item8
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item8.Value;
	}

	private readonly RefT<T9> item9;
	public ref T9 Item9
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item9.Value;
	}

	private readonly RefT<T10> item10;
	public ref T10 Item10
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item10.Value;
	}

	private readonly RefT<T11> item11;
	public ref T11 Item11
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item11.Value;
	}

	private readonly RefT<T12> item12;
	public ref T12 Item12
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item12.Value;
	}

	internal RefTuple(Worlds.Entity entity, RefT<T0> item0, RefT<T1> item1, RefT<T2> item2, RefT<T3> item3, RefT<T4> item4, RefT<T5> item5, RefT<T6> item6, RefT<T7> item7, RefT<T8> item8, RefT<T9> item9, RefT<T10> item10, RefT<T11> item11, RefT<T12> item12)
	{
		Entity = entity;
		this.item0 = item0;
		this.item1 = item1;
		this.item2 = item2;
		this.item3 = item3;
		this.item4 = item4;
		this.item5 = item5;
		this.item6 = item6;
		this.item7 = item7;
		this.item8 = item8;
		this.item9 = item9;
		this.item10 = item10;
		this.item11 = item11;
		this.item12 = item12;
	}

	public void Deconstruct(out Worlds.Entity entity, out RefT<T0> item0, out RefT<T1> item1, out RefT<T2> item2, out RefT<T3> item3, out RefT<T4> item4, out RefT<T5> item5, out RefT<T6> item6, out RefT<T7> item7, out RefT<T8> item8, out RefT<T9> item9, out RefT<T10> item10, out RefT<T11> item11, out RefT<T12> item12)
	{
		entity = Entity;
		item0 = this.item0;
		item1 = this.item1;
		item2 = this.item2;
		item3 = this.item3;
		item4 = this.item4;
		item5 = this.item5;
		item6 = this.item6;
		item7 = this.item7;
		item8 = this.item8;
		item9 = this.item9;
		item10 = this.item10;
		item11 = this.item11;
		item12 = this.item12;
	}
}

[ExcludeFromCodeCoverage]
public readonly ref struct RefTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
{
	public readonly Worlds.Entity Entity;

	private readonly RefT<T0> item0;
	public ref T0 Item0
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item0.Value;
	}

	private readonly RefT<T1> item1;
	public ref T1 Item1
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item1.Value;
	}

	private readonly RefT<T2> item2;
	public ref T2 Item2
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item2.Value;
	}

	private readonly RefT<T3> item3;
	public ref T3 Item3
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item3.Value;
	}

	private readonly RefT<T4> item4;
	public ref T4 Item4
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item4.Value;
	}

	private readonly RefT<T5> item5;
	public ref T5 Item5
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item5.Value;
	}

	private readonly RefT<T6> item6;
	public ref T6 Item6
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item6.Value;
	}

	private readonly RefT<T7> item7;
	public ref T7 Item7
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item7.Value;
	}

	private readonly RefT<T8> item8;
	public ref T8 Item8
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item8.Value;
	}

	private readonly RefT<T9> item9;
	public ref T9 Item9
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item9.Value;
	}

	private readonly RefT<T10> item10;
	public ref T10 Item10
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item10.Value;
	}

	private readonly RefT<T11> item11;
	public ref T11 Item11
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item11.Value;
	}

	private readonly RefT<T12> item12;
	public ref T12 Item12
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item12.Value;
	}

	private readonly RefT<T13> item13;
	public ref T13 Item13
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item13.Value;
	}

	internal RefTuple(Worlds.Entity entity, RefT<T0> item0, RefT<T1> item1, RefT<T2> item2, RefT<T3> item3, RefT<T4> item4, RefT<T5> item5, RefT<T6> item6, RefT<T7> item7, RefT<T8> item8, RefT<T9> item9, RefT<T10> item10, RefT<T11> item11, RefT<T12> item12, RefT<T13> item13)
	{
		Entity = entity;
		this.item0 = item0;
		this.item1 = item1;
		this.item2 = item2;
		this.item3 = item3;
		this.item4 = item4;
		this.item5 = item5;
		this.item6 = item6;
		this.item7 = item7;
		this.item8 = item8;
		this.item9 = item9;
		this.item10 = item10;
		this.item11 = item11;
		this.item12 = item12;
		this.item13 = item13;
	}

	public void Deconstruct(out Worlds.Entity entity, out RefT<T0> item0, out RefT<T1> item1, out RefT<T2> item2, out RefT<T3> item3, out RefT<T4> item4, out RefT<T5> item5, out RefT<T6> item6, out RefT<T7> item7, out RefT<T8> item8, out RefT<T9> item9, out RefT<T10> item10, out RefT<T11> item11, out RefT<T12> item12, out RefT<T13> item13)
	{
		entity = Entity;
		item0 = this.item0;
		item1 = this.item1;
		item2 = this.item2;
		item3 = this.item3;
		item4 = this.item4;
		item5 = this.item5;
		item6 = this.item6;
		item7 = this.item7;
		item8 = this.item8;
		item9 = this.item9;
		item10 = this.item10;
		item11 = this.item11;
		item12 = this.item12;
		item13 = this.item13;
	}
}

[ExcludeFromCodeCoverage]
public readonly ref struct RefTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
{
	public readonly Worlds.Entity Entity;

	private readonly RefT<T0> item0;
	public ref T0 Item0
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item0.Value;
	}

	private readonly RefT<T1> item1;
	public ref T1 Item1
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item1.Value;
	}

	private readonly RefT<T2> item2;
	public ref T2 Item2
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item2.Value;
	}

	private readonly RefT<T3> item3;
	public ref T3 Item3
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item3.Value;
	}

	private readonly RefT<T4> item4;
	public ref T4 Item4
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item4.Value;
	}

	private readonly RefT<T5> item5;
	public ref T5 Item5
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item5.Value;
	}

	private readonly RefT<T6> item6;
	public ref T6 Item6
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item6.Value;
	}

	private readonly RefT<T7> item7;
	public ref T7 Item7
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item7.Value;
	}

	private readonly RefT<T8> item8;
	public ref T8 Item8
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item8.Value;
	}

	private readonly RefT<T9> item9;
	public ref T9 Item9
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item9.Value;
	}

	private readonly RefT<T10> item10;
	public ref T10 Item10
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item10.Value;
	}

	private readonly RefT<T11> item11;
	public ref T11 Item11
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item11.Value;
	}

	private readonly RefT<T12> item12;
	public ref T12 Item12
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item12.Value;
	}

	private readonly RefT<T13> item13;
	public ref T13 Item13
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item13.Value;
	}

	private readonly RefT<T14> item14;
	public ref T14 Item14
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item14.Value;
	}

	internal RefTuple(Worlds.Entity entity, RefT<T0> item0, RefT<T1> item1, RefT<T2> item2, RefT<T3> item3, RefT<T4> item4, RefT<T5> item5, RefT<T6> item6, RefT<T7> item7, RefT<T8> item8, RefT<T9> item9, RefT<T10> item10, RefT<T11> item11, RefT<T12> item12, RefT<T13> item13, RefT<T14> item14)
	{
		Entity = entity;
		this.item0 = item0;
		this.item1 = item1;
		this.item2 = item2;
		this.item3 = item3;
		this.item4 = item4;
		this.item5 = item5;
		this.item6 = item6;
		this.item7 = item7;
		this.item8 = item8;
		this.item9 = item9;
		this.item10 = item10;
		this.item11 = item11;
		this.item12 = item12;
		this.item13 = item13;
		this.item14 = item14;
	}

	public void Deconstruct(out Worlds.Entity entity, out RefT<T0> item0, out RefT<T1> item1, out RefT<T2> item2, out RefT<T3> item3, out RefT<T4> item4, out RefT<T5> item5, out RefT<T6> item6, out RefT<T7> item7, out RefT<T8> item8, out RefT<T9> item9, out RefT<T10> item10, out RefT<T11> item11, out RefT<T12> item12, out RefT<T13> item13, out RefT<T14> item14)
	{
		entity = Entity;
		item0 = this.item0;
		item1 = this.item1;
		item2 = this.item2;
		item3 = this.item3;
		item4 = this.item4;
		item5 = this.item5;
		item6 = this.item6;
		item7 = this.item7;
		item8 = this.item8;
		item9 = this.item9;
		item10 = this.item10;
		item11 = this.item11;
		item12 = this.item12;
		item13 = this.item13;
		item14 = this.item14;
	}
}

[ExcludeFromCodeCoverage]
public readonly ref struct RefTuple<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
{
	public readonly Worlds.Entity Entity;

	private readonly RefT<T0> item0;
	public ref T0 Item0
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item0.Value;
	}

	private readonly RefT<T1> item1;
	public ref T1 Item1
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item1.Value;
	}

	private readonly RefT<T2> item2;
	public ref T2 Item2
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item2.Value;
	}

	private readonly RefT<T3> item3;
	public ref T3 Item3
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item3.Value;
	}

	private readonly RefT<T4> item4;
	public ref T4 Item4
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item4.Value;
	}

	private readonly RefT<T5> item5;
	public ref T5 Item5
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item5.Value;
	}

	private readonly RefT<T6> item6;
	public ref T6 Item6
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item6.Value;
	}

	private readonly RefT<T7> item7;
	public ref T7 Item7
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item7.Value;
	}

	private readonly RefT<T8> item8;
	public ref T8 Item8
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item8.Value;
	}

	private readonly RefT<T9> item9;
	public ref T9 Item9
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item9.Value;
	}

	private readonly RefT<T10> item10;
	public ref T10 Item10
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item10.Value;
	}

	private readonly RefT<T11> item11;
	public ref T11 Item11
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item11.Value;
	}

	private readonly RefT<T12> item12;
	public ref T12 Item12
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item12.Value;
	}

	private readonly RefT<T13> item13;
	public ref T13 Item13
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item13.Value;
	}

	private readonly RefT<T14> item14;
	public ref T14 Item14
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item14.Value;
	}

	private readonly RefT<T15> item15;
	public ref T15 Item15
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref item15.Value;
	}

	internal RefTuple(Worlds.Entity entity, RefT<T0> item0, RefT<T1> item1, RefT<T2> item2, RefT<T3> item3, RefT<T4> item4, RefT<T5> item5, RefT<T6> item6, RefT<T7> item7, RefT<T8> item8, RefT<T9> item9, RefT<T10> item10, RefT<T11> item11, RefT<T12> item12, RefT<T13> item13, RefT<T14> item14, RefT<T15> item15)
	{
		Entity = entity;
		this.item0 = item0;
		this.item1 = item1;
		this.item2 = item2;
		this.item3 = item3;
		this.item4 = item4;
		this.item5 = item5;
		this.item6 = item6;
		this.item7 = item7;
		this.item8 = item8;
		this.item9 = item9;
		this.item10 = item10;
		this.item11 = item11;
		this.item12 = item12;
		this.item13 = item13;
		this.item14 = item14;
		this.item15 = item15;
	}

	public void Deconstruct(out Worlds.Entity entity, out RefT<T0> item0, out RefT<T1> item1, out RefT<T2> item2, out RefT<T3> item3, out RefT<T4> item4, out RefT<T5> item5, out RefT<T6> item6, out RefT<T7> item7, out RefT<T8> item8, out RefT<T9> item9, out RefT<T10> item10, out RefT<T11> item11, out RefT<T12> item12, out RefT<T13> item13, out RefT<T14> item14, out RefT<T15> item15)
	{
		entity = Entity;
		item0 = this.item0;
		item1 = this.item1;
		item2 = this.item2;
		item3 = this.item3;
		item4 = this.item4;
		item5 = this.item5;
		item6 = this.item6;
		item7 = this.item7;
		item8 = this.item8;
		item9 = this.item9;
		item10 = this.item10;
		item11 = this.item11;
		item12 = this.item12;
		item13 = this.item13;
		item14 = this.item14;
		item15 = this.item15;
	}
}

