namespace RealmCore.Server.Modules.Players;

public class ToggleControlsScope : IDisposable
{
    protected readonly RealmPlayer _player;
    private readonly bool _fireEnabled;
    private readonly bool _aimWeaponEnabled;
    private readonly bool _nextWeaponEnabled;
    private readonly bool _previousWeaponEnabled;
    private readonly bool _forwardsEnabled;
    private readonly bool _backwardsEnabled;
    private readonly bool _leftEnabled;
    private readonly bool _rightEnabled;
    private readonly bool _zoomInEnabled;
    private readonly bool _zoomOutEnabled;
    private readonly bool _changeCameraEnabled;
    private readonly bool _jumpEnabled;
    private readonly bool _sprintEnabled;
    private readonly bool _lookBehindEnabled;
    private readonly bool _crouchEnabled;
    private readonly bool _actionEnabled;
    private readonly bool _walkEnabled;
    private readonly bool _enterExitEnabled;
    private readonly bool _vehicleFireEnabled;
    private readonly bool _vehicleSecondaryFireEnabled;
    private readonly bool _vehicleLeftEnabled;
    private readonly bool _vehicleRightEnabled;
    private readonly bool _steerForwardEnabled;
    private readonly bool _steerBackEnabled;
    private readonly bool _brakeReverseEnabled;
    private readonly bool _hornEnabled;
    private readonly bool _enterPassengerEnabled;

    public ToggleControlsScope(RealmPlayer player)
    {
        _player = player;

        if (_player.IsDestroyed)
            return;

        if (!player.TryEnterToggleControlScope())
            throw new InvalidOperationException("Only one toggleControlScope is allowed per player");

        var controls = _player.Controls;
        _fireEnabled = controls.FireEnabled;
        _aimWeaponEnabled = controls.AimWeaponEnabled;
        _nextWeaponEnabled = controls.NextWeaponEnabled;
        _previousWeaponEnabled = controls.PreviousWeaponEnabled;
        _forwardsEnabled = controls.ForwardsEnabled;
        _backwardsEnabled = controls.BackwardsEnabled;
        _leftEnabled = controls.LeftEnabled;
        _rightEnabled = controls.RightEnabled;
        _zoomInEnabled = controls.ZoomInEnabled;
        _zoomOutEnabled = controls.ZoomOutEnabled;
        _changeCameraEnabled = controls.ChangeCameraEnabled;
        _jumpEnabled = controls.JumpEnabled;
        _sprintEnabled = controls.SprintEnabled;
        _lookBehindEnabled = controls.LookBehindEnabled;
        _crouchEnabled = controls.CrouchEnabled;
        _actionEnabled = controls.ActionEnabled;
        _walkEnabled = controls.WalkEnabled;
        _enterExitEnabled = controls.EnterExitEnabled;
        _vehicleFireEnabled = controls.VehicleFireEnabled;
        _vehicleSecondaryFireEnabled = controls.VehicleSecondaryFireEnabled;
        _vehicleLeftEnabled = controls.VehicleLeftEnabled;
        _vehicleRightEnabled = controls.VehicleRightEnabled;
        _steerForwardEnabled = controls.SteerForwardEnabled;
        _steerBackEnabled = controls.SteerBackEnabled;
        _brakeReverseEnabled = controls.BrakeReverseEnabled;
        _hornEnabled = controls.HornEnabled;
        _enterPassengerEnabled = controls.EnterPassengerEnabled;
        player.ToggleAllControls(false, true, false);
    }

    public virtual void Dispose()
    {
        if (_player.IsDestroyed)
            return;

        if (!_player.ExitToggleControlScope())
            throw new InvalidOperationException("Scope already exited");

        _player.ToggleAllControls(true, true, false);
        var controls = _player.Controls;
        controls.FireEnabled = _fireEnabled;
        controls.AimWeaponEnabled = _aimWeaponEnabled;
        controls.NextWeaponEnabled = _nextWeaponEnabled;
        controls.PreviousWeaponEnabled = _previousWeaponEnabled;
        controls.ForwardsEnabled = _forwardsEnabled;
        controls.BackwardsEnabled = _backwardsEnabled;
        controls.LeftEnabled = _leftEnabled;
        controls.RightEnabled = _rightEnabled;
        controls.ZoomInEnabled = _zoomInEnabled;
        controls.ZoomOutEnabled = _zoomOutEnabled;
        controls.ChangeCameraEnabled = _changeCameraEnabled;
        controls.JumpEnabled = _jumpEnabled;
        controls.SprintEnabled = _sprintEnabled;
        controls.LookBehindEnabled = _lookBehindEnabled;
        controls.CrouchEnabled = _crouchEnabled;
        controls.ActionEnabled = _actionEnabled;
        controls.WalkEnabled = _walkEnabled;
        controls.EnterExitEnabled = _enterExitEnabled;
        controls.VehicleFireEnabled = _vehicleFireEnabled;
        controls.VehicleSecondaryFireEnabled = _vehicleSecondaryFireEnabled;
        controls.VehicleLeftEnabled = _vehicleLeftEnabled;
        controls.VehicleRightEnabled = _vehicleRightEnabled;
        controls.SteerForwardEnabled = _steerForwardEnabled;
        controls.SteerBackEnabled = _steerBackEnabled;
        controls.BrakeReverseEnabled = _brakeReverseEnabled;
        controls.HornEnabled = _hornEnabled;
        controls.EnterPassengerEnabled = _enterPassengerEnabled;
    }
}
