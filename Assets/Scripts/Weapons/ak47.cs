using System;
using System.Collections;
using UnityEngine;

public class ak47 : Gun
{
    public override void Shoot()
    {
        RaycastHit hit;

        if (Physics.Raycast(RayCaster.position, RayCaster.forward, out hit, gunData.shootingRange, gunData.targerLayerMask))
        {
            if (hit.collider.gameObject.CompareTag("Enviroment"))
            {
                hit.rigidbody.AddExplosionForce(350, hit.point, 100, 0);
            }
            else
            {
                HealthEnemy healthEnemy = hit.collider.gameObject.GetComponentInParent<HealthEnemy>();
                if (healthEnemy != null)
                {
                    ApplyDamage(hit, healthEnemy);
                }
            }
        }

        gunRecoil.ApplyRecoil();
    }

    private void ApplyDamage(RaycastHit hit, HealthEnemy healthEnemy)
    {
        float damage = DetermineDamage(hit.collider.gameObject.tag);
        if (healthEnemy.IsDead(damage))
        {
            healthEnemy.TakeDamage(damage, RayCaster.forward, hit);
            StartCoroutine(Knowback(hit));
        }
        healthEnemy.TakeDamage(damage, RayCaster.forward, hit);
    }

    private IEnumerator Knowback(RaycastHit hit)
    {
        yield return null;
        hit.rigidbody.AddForce(RayCaster.forward * 15f, ForceMode.Impulse);
    }

    public void Update()
    {
        base.Update();

        HandleAiming();
        HandleFire();
        HandleIdleToggle();
        HandleReload();
    }
    private void HandleIdleToggle()
    {
        if (Input.GetKeyDown(KeyCode.V))
        { SetIdleAnimation(); }
    }
    private void HandleReload()
    {
        if (Input.GetKeyDown(KeyCode.R))
        { TryReload(); }
    }
    private void HandleFire()
    {
        if (Input.GetKeyDown(KeyCode.B)) {
            FireMode = !FireMode;
            StartCoroutine(FireModeSound(ShootEmptySFX.length)); 
        }
        if (!isIdle)
        { (FireMode ? (Action)AutoMode : SemiMode)(); }
    }

    private void HandleAiming()
    {
        if (Input.GetKey(KeyCode.Mouse1) && !animator.GetBool(RunHashCode))
        {
            animator.SetBool(AimHashCode, true);

            if (!hasPlayedSound)
            {
                StartCoroutine(AimSound(ShootEmptySFX.length));
                hasPlayedSound = true;
            }
        }
        else
        {
            animator.SetBool(AimHashCode, false); 
            hasPlayedSound = false;
        }
    }

    private void SemiMode()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            canShoot = true;
            TryShoot();
        }
        else
        { canShoot = false;
          animator.SetBool(FireHashCode, canShoot);
        }
    }

    private void AutoMode()
    {
        if (Input.GetButton("Fire1"))
        {
            canShoot = true;
            TryShoot();
        }
        else
        { canShoot = false;
          animator.SetBool(FireHashCode, canShoot);
        }
    }

}
