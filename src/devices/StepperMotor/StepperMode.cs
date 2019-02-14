namespace Iot.Device.StepperMotor
{
    /// <summary>
    /// The 28BYJ-48 motor has 512 full engine rotations to rotate the drive shaft once.
    /// In half-step mode these are 8 x 512 = 4096 steps for a full rotation.
    /// In full-step mode these are 4 x 512 = 2048 steps for a full rotation.
    /// </summary>
    public enum StepperMode
    {
        HalfStep,
        FullStepSinglePhase,
        FullStepDualPhase
    }
}
