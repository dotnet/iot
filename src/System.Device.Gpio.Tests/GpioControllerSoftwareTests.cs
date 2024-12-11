// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace System.Device.Gpio.Tests;

public class GpioControllerSoftwareTests : IDisposable
{
    private Mock<MockableGpioDriver> _mockedGpioDriver;

    public GpioControllerSoftwareTests()
    {
        _mockedGpioDriver = new Mock<MockableGpioDriver>(MockBehavior.Default);
        _mockedGpioDriver.CallBase = true;
    }

    public void Dispose()
    {
        _mockedGpioDriver.VerifyAll();
    }

    [Fact]
    public void PinCountReportedCorrectly()
    {
        var ctrl = new GpioController(_mockedGpioDriver.Object);
        Assert.Equal(28, ctrl.PinCount);
    }

    [Fact]
    public void OpenTwiceGpioPinAndClosedTwiceThrows()
    {
        var ctrl = new GpioController(_mockedGpioDriver.Object);
        _mockedGpioDriver.Setup(x => x.OpenPinEx(1));
        _mockedGpioDriver.Setup(x => x.ClosePinEx(1));
        GpioPin gpioPin1 = ctrl.OpenPin(1);
        GpioPin gpiopin2 = ctrl.OpenPin(1);
        Assert.Equal(gpioPin1, gpiopin2);
        ctrl.ClosePin(1);
        Assert.Throws<InvalidOperationException>(() => ctrl.ClosePin(1));
    }

    [Fact]
    public void WriteInputPinDoesNotThrow()
    {
        _mockedGpioDriver.Setup(x => x.OpenPinEx(1));
        _mockedGpioDriver.Setup(x => x.IsPinModeSupportedEx(1, It.IsAny<PinMode>())).Returns(true);
        _mockedGpioDriver.Setup(x => x.SetPinModeEx(1, It.IsAny<PinMode>()));
        _mockedGpioDriver.Setup(x => x.GetPinModeEx(1)).Returns(PinMode.Input);
        var ctrl = new GpioController(_mockedGpioDriver.Object);

        ctrl.OpenPin(1, PinMode.Input);
        ctrl.Write(1, PinValue.High);
    }

    [Fact]
    public void GpioControllerCreateOpenClosePin()
    {
        _mockedGpioDriver.Setup(x => x.OpenPinEx(1));
        _mockedGpioDriver.Setup(x => x.IsPinModeSupportedEx(1, PinMode.Output)).Returns(true);
        _mockedGpioDriver.Setup(x => x.GetPinModeEx(1)).Returns(PinMode.Output);
        _mockedGpioDriver.Setup(x => x.WriteEx(1, PinValue.High));
        _mockedGpioDriver.Setup(x => x.ClosePinEx(1));
        var ctrl = new GpioController(_mockedGpioDriver.Object);
        Assert.NotNull(ctrl);
        ctrl.OpenPin(1, PinMode.Output);
        Assert.True(ctrl.IsPinOpen(1));
        ctrl.Write(1, PinValue.High);
        ctrl.ClosePin(1);
        Assert.False(ctrl.IsPinOpen(1));
    }

    [Fact]
    public void IsPinModeSupported()
    {
        _mockedGpioDriver.Setup(x => x.IsPinModeSupportedEx(1, PinMode.Input)).Returns(true);
        var ctrl = new GpioController(_mockedGpioDriver.Object);
        Assert.NotNull(ctrl);
        Assert.True(ctrl.IsPinModeSupported(1, PinMode.Input));
    }

    [Fact]
    public void GetPinMode()
    {
        _mockedGpioDriver.Setup(x => x.OpenPinEx(1));
        _mockedGpioDriver.Setup(x => x.GetPinModeEx(1)).Returns(PinMode.Output);
        var ctrl = new GpioController(_mockedGpioDriver.Object);
        Assert.NotNull(ctrl);
        // Not open
        Assert.Throws<InvalidOperationException>(() => ctrl.GetPinMode(1));
        ctrl.OpenPin(1);
        Assert.Equal(PinMode.Output, ctrl.GetPinMode(1));
    }

    [Fact]
    [Obsolete("Tests an obsolete feature")]
    public void UsingBoardNumberingWorks()
    {
        // Our mock driver maps physical pin 2 to logical pin 1
        _mockedGpioDriver.Setup(x => x.ConvertPinNumberToLogicalNumberingSchemeEx(2)).Returns(1);
        _mockedGpioDriver.Setup(x => x.OpenPinEx(1));
        _mockedGpioDriver.Setup(x => x.SetPinModeEx(1, PinMode.Output));
        _mockedGpioDriver.Setup(x => x.IsPinModeSupportedEx(1, PinMode.Output)).Returns(true);
        _mockedGpioDriver.Setup(x => x.GetPinModeEx(1)).Returns(PinMode.Output);
        _mockedGpioDriver.Setup(x => x.WriteEx(1, PinValue.High));
        _mockedGpioDriver.Setup(x => x.ReadEx(1)).Returns(PinValue.High);
        _mockedGpioDriver.Setup(x => x.ClosePinEx(1));
        var ctrl = new GpioController(PinNumberingScheme.Board, _mockedGpioDriver.Object);
        ctrl.OpenPin(2, PinMode.Output);
        ctrl.Write(2, PinValue.High);
        Assert.Equal(PinValue.High, ctrl.Read(2));
        ctrl.ClosePin(2);
        ctrl.Dispose();
    }

    [Fact]
    public void UsingLogicalNumberingDisposesTheRightPin()
    {
        _mockedGpioDriver.Setup(x => x.OpenPinEx(1));
        _mockedGpioDriver.Setup(x => x.ClosePinEx(1));
        _mockedGpioDriver.Setup(x => x.IsPinModeSupportedEx(1, PinMode.Output)).Returns(true);
        _mockedGpioDriver.Setup(x => x.GetPinModeEx(1)).Returns(PinMode.Output);
        var ctrl = new GpioController(_mockedGpioDriver.Object);
        ctrl.OpenPin(1, PinMode.Output);
        ctrl.Write(1, PinValue.High);
        // No close on the pin here, we want to check that the Controller's Dispose works correctly
        ctrl.Dispose();
    }

    [Fact]
    [Obsolete("Tests obsolete features")]
    public void UsingBoardNumberingDisposesTheRightPin()
    {
        // Our mock driver maps physical pin 2 to logical pin 1
        _mockedGpioDriver.Setup(x => x.ConvertPinNumberToLogicalNumberingSchemeEx(2)).Returns(1);
        _mockedGpioDriver.Setup(x => x.OpenPinEx(1));
        _mockedGpioDriver.Setup(x => x.SetPinModeEx(1, PinMode.Output));
        _mockedGpioDriver.Setup(x => x.ClosePinEx(1));
        _mockedGpioDriver.Setup(x => x.IsPinModeSupportedEx(1, PinMode.Output)).Returns(true);
        var ctrl = new GpioController(PinNumberingScheme.Board, _mockedGpioDriver.Object);
        ctrl.OpenPin(2, PinMode.Output);
        // No close on the pin here, we want to check that the Controller's Dispose works correctly
        ctrl.Dispose();
    }

    [Fact]
    public void CallbackOnEventWorks()
    {
        // Our mock driver maps physical pin 2 to logical pin 1
        _mockedGpioDriver.Setup(x => x.OpenPinEx(1));
        _mockedGpioDriver.Setup(x => x.AddCallbackForPinValueChangedEventEx(1,
            PinEventTypes.Rising, It.IsAny<PinChangeEventHandler>()));
        var ctrl = new GpioController(_mockedGpioDriver.Object);
        ctrl.OpenPin(1); // logical pin 1 on our test board
        bool callbackSeen = false;
        PinChangeEventHandler eventHandler = (sender, args) =>
        {
            callbackSeen = true;
            Assert.Equal(1, args.PinNumber);
            Assert.Equal(PinEventTypes.Falling, args.ChangeType);
        };

        ctrl.RegisterCallbackForPinValueChangedEvent(1, PinEventTypes.Rising, eventHandler);

        _mockedGpioDriver.Object.FireEventHandler(1, PinEventTypes.Falling);

        Assert.True(callbackSeen);

        ctrl.UnregisterCallbackForPinValueChangedEvent(1, eventHandler);
    }

    [Fact]
    public void WriteSpan()
    {
        _mockedGpioDriver.Setup(x => x.OpenPinEx(1));
        _mockedGpioDriver.Setup(x => x.OpenPinEx(2));
        _mockedGpioDriver.Setup(x => x.IsPinModeSupportedEx(1, PinMode.Output)).Returns(true);
        _mockedGpioDriver.Setup(x => x.IsPinModeSupportedEx(2, PinMode.Output)).Returns(true);
        _mockedGpioDriver.Setup(x => x.GetPinModeEx(1)).Returns(PinMode.Output);
        _mockedGpioDriver.Setup(x => x.GetPinModeEx(2)).Returns(PinMode.Output);
        _mockedGpioDriver.Setup(x => x.WriteEx(1, PinValue.High));
        _mockedGpioDriver.Setup(x => x.WriteEx(2, PinValue.Low));
        _mockedGpioDriver.Setup(x => x.ClosePinEx(1));
        _mockedGpioDriver.Setup(x => x.ClosePinEx(2));
        var ctrl = new GpioController(_mockedGpioDriver.Object);
        Assert.NotNull(ctrl);
        ctrl.OpenPin(1, PinMode.Output);
        ctrl.OpenPin(2, PinMode.Output);
        Assert.True(ctrl.IsPinOpen(1));
        Span<PinValuePair> towrite = stackalloc PinValuePair[2];
        towrite[0] = new PinValuePair(1, PinValue.High);
        towrite[1] = new PinValuePair(2, PinValue.Low);
        ctrl.Write(towrite);
        ctrl.ClosePin(1);
        ctrl.ClosePin(2);
        Assert.False(ctrl.IsPinOpen(1));
    }

    [Fact]
    public void ReadSpan()
    {
        _mockedGpioDriver.Setup(x => x.OpenPinEx(1));
        _mockedGpioDriver.Setup(x => x.OpenPinEx(2));
        _mockedGpioDriver.Setup(x => x.IsPinModeSupportedEx(1, PinMode.Input)).Returns(true);
        _mockedGpioDriver.Setup(x => x.IsPinModeSupportedEx(2, PinMode.Input)).Returns(true);
        _mockedGpioDriver.Setup(x => x.ReadEx(1)).Returns(PinValue.Low);
        _mockedGpioDriver.Setup(x => x.ReadEx(2)).Returns(PinValue.High);
        _mockedGpioDriver.Setup(x => x.ClosePinEx(1));
        _mockedGpioDriver.Setup(x => x.ClosePinEx(2));
        var ctrl = new GpioController(_mockedGpioDriver.Object);
        Assert.NotNull(ctrl);
        ctrl.OpenPin(1, PinMode.Input);
        ctrl.OpenPin(2, PinMode.Input);
        Assert.True(ctrl.IsPinOpen(1));

        // Invalid usage (we need to prefill the array)
        // Was this the intended use case?
        Assert.Throws<InvalidOperationException>(() =>
        {
            Span<PinValuePair> wrongArg = stackalloc PinValuePair[2];
            ctrl.Read(wrongArg);
        });

        Span<PinValuePair> toread = stackalloc PinValuePair[2];
        toread[0] = new PinValuePair(1, PinValue.Low);
        toread[1] = new PinValuePair(2, PinValue.Low);
        ctrl.Read(toread);
        Assert.Equal(1, toread[0].PinNumber);
        Assert.Equal(2, toread[1].PinNumber);
        Assert.Equal(PinValue.Low, toread[0].PinValue);
        Assert.Equal(PinValue.High, toread[1].PinValue);
        ctrl.ClosePin(1);
        ctrl.ClosePin(2);
        Assert.False(ctrl.IsPinOpen(1));
    }

    [Fact]
    public async Task WaitForEventAsyncFail()
    {
        var ctrl = new GpioController(_mockedGpioDriver.Object);
        _mockedGpioDriver.Setup(x => x.OpenPinEx(1));
        _mockedGpioDriver.Setup(x => x.IsPinModeSupportedEx(1, PinMode.Input)).Returns(true);
        _mockedGpioDriver.Setup(x => x.WaitForEventEx(1, PinEventTypes.Rising | PinEventTypes.Falling, It.IsAny<CancellationToken>()))
            .Returns(new WaitForEventResult()
            {
                EventTypes = PinEventTypes.None, TimedOut = true
            });
        Assert.NotNull(ctrl);
        ctrl.OpenPin(1, PinMode.Input);

        var task = ctrl.WaitForEventAsync(1, PinEventTypes.Falling | PinEventTypes.Rising, TimeSpan.FromSeconds(0.01)).AsTask();
        var result = await task.WaitAsync(CancellationToken.None);
        Assert.True(task.IsCompleted);
        Assert.Null(task.Exception);
        Assert.True(result.TimedOut);
        Assert.Equal(PinEventTypes.None, result.EventTypes);
    }

    [Fact]
    public void WaitForEventSuccess()
    {
        var ctrl = new GpioController(_mockedGpioDriver.Object);
        _mockedGpioDriver.Setup(x => x.OpenPinEx(1));
        _mockedGpioDriver.Setup(x => x.IsPinModeSupportedEx(1, PinMode.Input)).Returns(true);
        _mockedGpioDriver.Setup(x => x.WaitForEventEx(1, PinEventTypes.Rising | PinEventTypes.Falling, It.IsAny<CancellationToken>()))
            .Returns(new WaitForEventResult()
            {
                EventTypes = PinEventTypes.Falling,
                TimedOut = false
            });
        Assert.NotNull(ctrl);
        ctrl.OpenPin(1, PinMode.Input);

        var result = ctrl.WaitForEvent(1, PinEventTypes.Falling | PinEventTypes.Rising, TimeSpan.FromSeconds(0.01));
        Assert.False(result.TimedOut);
        Assert.Equal(PinEventTypes.Falling, result.EventTypes);
    }

    // TODO: This is still broken. See #974
    ////[Fact]
    ////public void UsingBoardNumberingForCallbackWorks()
    ////{
    ////    // Our mock driver maps physical pin 2 to logical pin 1
    ////    _mockedGpioDriver.Setup(x => x.ConvertPinNumberToLogicalNumberingSchemeEx(2)).Returns(1);
    ////    _mockedGpioDriver.Setup(x => x.OpenPinEx(1));
    ////    _mockedGpioDriver.Setup(x => x.AddCallbackForPinValueChangedEventEx(1,
    ////        PinEventTypes.Rising, It.IsAny<PinChangeEventHandler>()));
    ////    var ctrl = new GpioController(PinNumberingScheme.Board, _mockedGpioDriver.Object);
    ////    ctrl.OpenPin(2); // logical pin 1 on our test board
    ////    bool callbackSeen = false;
    ////    ctrl.RegisterCallbackForPinValueChangedEvent(2, PinEventTypes.Rising, (sender, args) =>
    ////    {
    ////        callbackSeen = true;
    ////        Assert.Equal(2, args.PinNumber);
    ////        Assert.Equal(PinEventTypes.Falling, args.ChangeType);
    ////    });

    ////    _mockedGpioDriver.Object.FireEventHandler(1, PinEventTypes.Falling);

    ////    Assert.True(callbackSeen);
    ////}

}
