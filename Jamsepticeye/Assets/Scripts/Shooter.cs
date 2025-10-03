using UnityEngine;

public class Shooter : MonoBehaviour
{
    [SerializeField] GameObject _bulletPrefab;
    [SerializeField] float _shootCooldown = 1.0f;
    float _currentTime = 0.0f;

    [SerializeField] Vector2 _shootDirection = Vector2.zero;
    [SerializeField] int _bulletSpeed = 1;

    [SerializeField] bool _canShoot = true;

    void Update()
    {
        if (_canShoot)
        {
            if(_currentTime < _shootCooldown)
            {
                _currentTime += Time.deltaTime;
            }
            else
            {
                shoot();
                _currentTime = 0.0f;
            }
        }
    }

    void shoot()
    {
        GameObject bullet = Instantiate(_bulletPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().linearVelocity = _shootDirection * _bulletSpeed;
    }
}

