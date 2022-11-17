using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MHotkeyCommands
{
    // thanks to ShimmyMySherbet#5694 in the imperial discord for this
    public delegate void PlayerKeyInputArgs(Player player, EPlayerKey key, bool down);
    public class PlayerInputListener : MonoBehaviour
    {
        public static event PlayerKeyInputArgs PlayerKeyInput;
        public PlayerInput Input { get; private set; }
        private bool[] m_KeyStates = new bool[0];
        public bool awake = false;
        private void Awake()
        {
            Input = GetComponentInParent<PlayerInput>();
            if (Input == null)
            {
                throw new InvalidOperationException("Must be attached to a Player");
            }
            m_KeyStates = new bool[Input.keys.Length];
            awake = true;
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < m_KeyStates.Length; i++)
            {
                if (m_KeyStates[i] != Input.keys[i])
                {
                    m_KeyStates[i] = Input.keys[i];
                    RaiseFor(i, m_KeyStates[i]);
                }
            }
        }

        private void RaiseFor(int key, bool state)
        {
            PlayerKeyInput?.Invoke(Input.player, (EPlayerKey)key, state);
        }
    }

    public enum EPlayerKey : int
    {
        Jump = 0,
        Primary = 1,
        Secondary = 2,
        Crouch = 3,
        Prone = 4,
        Sprint = 5,
        LeanLeft = 6,
        LeanRight = 7,
        Nill = 8,
        SteadyAim = 9,
        HotKey1 = 10,
        HotKey2 = 11,
        HotKey3 = 12,
        HotKey4 = 13,
        HotKey5 = 14
    }

}
