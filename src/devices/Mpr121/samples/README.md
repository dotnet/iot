# MPR121 Sample

This sample demonstrates how to read channel statuses using auto-refresh configuration.

### Handling the channel statuses changes

```csharp
mpr121.PropertyChanged += (object sender, PropertyChangedEventArgs e) => {
    if (e.PropertyName == nameof(Mpr121.ChannelStatuses))
    {
        // update the channel statuses table.
    }
};
```

### Channel statuses table

The following status means that the **channel 1** and **channel 3** are pressed:

![](./ChannelStatuses.png)