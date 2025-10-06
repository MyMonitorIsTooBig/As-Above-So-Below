using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerAnimManager : MonoBehaviour
{

    [SerializeField] Animator _playerAnim;
    [SerializeField] PlayerMovement _player;
    InputAction _walkInput;
    InputAction _jumpInput;

    [SerializeField] List<AudioClip> _stepSounds;
    [SerializeField] AudioSource _stepSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _player = GetComponent<PlayerMovement>();

        _jumpInput = InputSystem.actions.FindAction("Jump");
        _jumpInput.started += animChange;
        _jumpInput.performed += animChange;
        _jumpInput.canceled += animChange;
    }

    private void OnDisable()
    {
        _jumpInput.started -= animChange;
        _jumpInput.performed -= animChange;
        _jumpInput.canceled -= animChange;
    }

    void animChange(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _playerAnim.SetTrigger("IsJumping");
        }
    }

    private void Update()
    {
        if (_player.CanMove)
        {


            if (_player.Grounded)
            {
                _playerAnim.SetBool("IsGrounded", true);
            }
            else
            {
                _playerAnim.SetBool("IsGrounded", false);
            }

            if (_player.Moving)
            {
                _playerAnim.SetBool("IsMoving", true);
            }
            else
            {
                _playerAnim.SetBool("IsMoving", false);
            }

            
        }
    }

    public void playRandomStepSound()
    {
        _stepSource.clip = _stepSounds[Random.Range(0, _stepSounds.Count - 1)];
        _stepSource.pitch = Random.Range(0.85f, 1.15f);
        _stepSource.Play();
    }
}
