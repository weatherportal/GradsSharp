# Command Interface

The command interface is the primary way of interacting with the GradsEngine.
After initializing the engine you can use the methods defined in the [IGradsCommandInterface](/interface_grads_sharp_1_1_i_grads_command_interface.html) to interact with the engine.


Some sample code:

```csharp
engine.GradsCommandInterface.SetPArea(OnOffSetting.On, 0, 11, 0, 8);
engine.GradsCommandInterface.SetMPVals(OnOffSetting.On, -3, 12, 42, 55);
engine.GradsCommandInterface.SetLat(42,55);
engine.GradsCommandInterface.SetLon(-3,12);
engine.GradsCommandInterface.SetGraphicsOut(GxOutSetting.Shaded);
engine.GradsCommandInterface.SetT(1);
engine.GradsCommandInterface.SetMapResolution(MapResolution.HighResolution);
```

