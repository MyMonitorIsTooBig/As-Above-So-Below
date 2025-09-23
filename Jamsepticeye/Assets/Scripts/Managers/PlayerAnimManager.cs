using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerAnimManager : MonoBehaviour
{

    [SerializeField] Animator _playerAnim;
    [SerializeField] PlayerMovement _player;
    InputAction _walkInput;

    [SerializeField] List<AudioClip> _stepSounds;
    [SerializeField] AudioSource _stepSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _walkInput = InputSystem.actions.FindAction("Move");
        _walkInput.started += animChange;
        _walkInput.performed += animChange;
        _walkInput.canceled += animChange;
        _player = GetComponent<PlayerMovement>();
    }

    private void OnDisable()
    {
        _walkInput.started -= animChange;
        _walkInput.performed -= animChange;
        _walkInput.canceled -= animChange;
    }

    void animChange(InputAction.CallbackContext context)
    {
        
    }

    private void Update()
    {
        if (_player.CanMove)
        {
            Vector2 _move = _walkInput.ReadValue<Vector2>();

            switch (_move)
            {
                case Vector2 m when m.Equals(Vector2.down):
                    animBool(false, false, false, false, true);
                    break;

                case Vector2 m when m.Equals(Vector2.up):
                    animBool(true, false, false, false, false);
                    break;

                case Vector2 m when m.Equals(Vector2.left):
                    animBool(false, false, true, false, false);
                    break;

                case Vector2 m when m.Equals(Vector2.right):
                    animBool(false, true, false, false, false);
                    break;

                case Vector2 m when m.Equals(Vector2.zero):
                    animBool(false, false, false, true, false);
                    break;
            }
        }

        else
        {
            animBool(false, false, false, false, false);
        }
    }

    void animBool(bool backwards, bool right, bool left, bool idle, bool forwards)
    {
        _playerAnim.SetBool("backwards", backwards);
        _playerAnim.SetBool("right", right);
        _playerAnim.SetBool("left", left);
        _playerAnim.SetBool("idle", idle);
        _playerAnim.SetBool("forward", forwards);
    }

    public void playRandomStepSound()
    {
        _stepSource.clip = _stepSounds[Random.Range(0, _stepSounds.Count - 1)];
        _stepSource.pitch = Random.Range(0.85f, 1.15f);
        _stepSource.Play();
    }
}
