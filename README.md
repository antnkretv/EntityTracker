# EntityTracker
A simple library that can help track entities in distributed systems, aka SOA

### Usage

Define the conditional compilation constants in .csproj file 

```
<DefineConstants>ENABLE_LOG_TRACKER</DefineConstants>
```

Example 1
```cs

// Method at a service 1
void Main()
{
	var entityA = new EntityA { SomePropertyA = "Value of property A" };
	var trackId = new Guid("c2be2311-a0e2-4a20-8510-3207d06894d3"); // Context id for tracking
	Tracker.BeginTrack(ref entityA, trackId); // now entityA has additional property that has track id
	PublishToService2ViaRabbit(entityA); // publishing entity to other service via rabbit for example
}

// Method at a service 2
void RabbitHandler(EntityB entity)
{
	var json = ToJsonString(entity);
	Console.WriteLine(json); // { \"SomePropertyA\": \"Value of property A\", \"TrackId\": \"c2be2311-a0e2-4a20-8510-3207d06894d3\" }
}

public class EntityA
{
	public string SomePropertyA { get; set; }
}

public class EntityB
{
	public string SomePropertyA { get; set; }
#if ENABLE_LOG_TRACKER
	public Guid TrackId { get; set; }
#endif
}

```

Example 2

```cs
void Main()
{
	var entityA = new EntityA();
	Tracker.BeginTrack(ref entityA); // now entityA has additional property that has track id
	
	var entityB = new EntityB { SomePropertyB = "Value of property B" }; // Any other entity
	Tracker.LinkTo(entityA, ref entityB); // Copy track id to entityB
	
	var json = ToJsonString(entityB);
	Console.WriteLine(json); // { \"SomePropertyB\": \"Value of property B\", \"TrackId\": \"c2be2311-a0e2-4a20-8510-3207d06894d3\" }
}

public class EntityA
{
}

public class EntityB
{
	public string SomePropertyB { get; set; }
}
```
