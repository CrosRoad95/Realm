namespace RealmCore.Server.Components.Elements;

public class PedElementComponent : ElementComponent
{
    protected readonly Ped _ped;

    internal Ped Ped => _ped;

    internal override Element Element => _ped;

    public ushort Model
    {
        get
        {
            ThrowIfDisposed();
            return _ped.Model;
        }
        set
        {
            ThrowIfDisposed();
            _ped.Model = value;
        }
    }

    public float Health
    {
        get
        {
            ThrowIfDisposed();
            return _ped.Health;
        }
        set
        {
            ThrowIfDisposed();
            _ped.Health = value;
        }
    }

    public float Armor
    {
        get
        {
            ThrowIfDisposed();
            return _ped.Armor;
        }
        set
        {
            ThrowIfDisposed();
            _ped.Armor = value;
        }
    }

    public bool HasJetpack
    {
        get
        {
            ThrowIfDisposed();
            return _ped.HasJetpack;
        }
        set
        {
            ThrowIfDisposed();
            _ped.HasJetpack = value;
        }
    }

    public Clothing Clothing
    {
        get
        {
            ThrowIfDisposed();
            return _ped.Clothing;
        }
    }

    internal PedElementComponent(Ped ped)
    {
        _ped = ped;
    }

    public bool HasWeapon(WeaponId weaponId)
    {
        ThrowIfDisposed();
        return _ped.Weapons.Any(x => x.Type == weaponId && x.Ammo > 0);
    }

    public void GiveWeapon(WeaponId weaponId, ushort ammo = 1, ushort? ammoInClip = null)
    {
        ThrowIfDisposed();

        var weapon = _ped.Weapons.FirstOrDefault(x => x.Type == weaponId);
        if (weapon == null)
        {
            _ped.Weapons.Add(new Weapon(WeaponId.Bat, 1));
        }
        else
        {
            weapon.Ammo += ammo;
            if (ammoInClip != null)
                weapon.AmmoInClip += ammoInClip.Value;
        }
    }

    public void TakeWeapon(WeaponId weaponId, ushort ammo = 9999, ushort? ammoInClip = null)
    {
        ThrowIfDisposed();

        var weapon = _ped.Weapons.FirstOrDefault(x => x.Type == weaponId);
        if (weapon == null)
            return;

        weapon.Ammo -= Math.Min(weapon.Ammo, ammo);
        if (ammoInClip != null)
            weapon.AmmoInClip += Math.Min(ammoInClip.Value, ammo);

        if (weapon.Ammo == 0)
            _ped.Weapons.Remove(weapon);
    }

    public void TakeAllWeapons()
    {
        ThrowIfDisposed();

        _ped.Weapons.Clear();
    }
}
