using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Snes;

namespace SnesBox.Components
{
    class Input : GameComponent
    {
        private static Dictionary<Joypad, Buttons> _snesToXnaButtons = new Dictionary<Joypad, Buttons>();
        private static Dictionary<Joypad, Keys> _snesToXnaKeys = new Dictionary<Joypad, Keys>();
        private const Joypad LeftRight = Joypad.Left | Joypad.Right;
        private const Joypad UpDown = Joypad.Up | Joypad.Down;

        private ushort[] _inputButtons = new ushort[8];
        private int[] _inputCoords = new int[8];

        public Input(SnesBoxGame game)
            : base(game)
        {
            LibSnes.InputState += new EventHandler<InputStateEventArgs>(OnInputState);
        }

        static Input()
        {
            _snesToXnaButtons.Add(Joypad.A, Buttons.B);
            _snesToXnaButtons.Add(Joypad.B, Buttons.A);
            _snesToXnaButtons.Add(Joypad.X, Buttons.Y);
            _snesToXnaButtons.Add(Joypad.Y, Buttons.X);
            _snesToXnaButtons.Add(Joypad.L, Buttons.LeftShoulder);
            _snesToXnaButtons.Add(Joypad.R, Buttons.RightShoulder);
            _snesToXnaButtons.Add(Joypad.Start, Buttons.Start);
            _snesToXnaButtons.Add(Joypad.Select, Buttons.Back);
            _snesToXnaButtons.Add(Joypad.Down, Buttons.DPadDown);
            _snesToXnaButtons.Add(Joypad.Up, Buttons.DPadUp);
            _snesToXnaButtons.Add(Joypad.Left, Buttons.DPadLeft);
            _snesToXnaButtons.Add(Joypad.Right, Buttons.DPadRight);

            _snesToXnaKeys.Add(Joypad.A, Keys.X);
            _snesToXnaKeys.Add(Joypad.B, Keys.Z);
            _snesToXnaKeys.Add(Joypad.X, Keys.Z);
            _snesToXnaKeys.Add(Joypad.Y, Keys.A);
            _snesToXnaKeys.Add(Joypad.L, Keys.D);
            _snesToXnaKeys.Add(Joypad.R, Keys.C);
            _snesToXnaKeys.Add(Joypad.Start, Keys.Enter);
            _snesToXnaKeys.Add(Joypad.Select, Keys.OemQuotes);
            _snesToXnaKeys.Add(Joypad.Down, Keys.Up);
            _snesToXnaKeys.Add(Joypad.Up, Keys.Down);
            _snesToXnaKeys.Add(Joypad.Left, Keys.Left);
            _snesToXnaKeys.Add(Joypad.Right, Keys.Right);
        }

        public override void Initialize()
        {
            LibSnes.SetControllerPortDevice(Port.One, Device.Joypad);
            LibSnes.SetControllerPortDevice(Port.Two, Device.Joypad);
        }

        public override void Update(GameTime gameTime)
        {
            SetInputState(Port.One, 0, (int)ParseInput(PlayerIndex.One), 0, 0);
            SetInputState(Port.Two, 0, (int)ParseInput(PlayerIndex.Two), 0, 0);
        }

        public void SetInputState(Port port, uint index, int buttonStates, int x, int y)
        {
            if ((buttonStates & (int)LeftRight) == (int)(LeftRight))
            {
                buttonStates &= (int)~LeftRight;
            }

            if ((buttonStates & (int)UpDown) == (int)UpDown)
            {
                buttonStates &= (int)~UpDown;
            }

            var i = (uint)port * 4 + index;
            _inputButtons[i] = (ushort)buttonStates;
            _inputCoords[i] = (int)(((ushort)y << 16) | (ushort)x);
        }

        private static Joypad ParseInput(PlayerIndex playerIndex)
        {
            var gamePadState = GamePad.GetState(playerIndex);
            var keyboardState = Keyboard.GetState();
            var snesButtonStates = default(Joypad);

            foreach (Joypad button in EnumExtensions.GetEnumValues<Joypad>())
            {
                if (gamePadState.IsButtonDown(_snesToXnaButtons[button]) || keyboardState.IsKeyDown(_snesToXnaKeys[button]))
                {
                    snesButtonStates |= button;
                }
            }

            return snesButtonStates;
        }

        private void OnInputState(object sender, InputStateEventArgs e)
        {
            var i = ((uint)e.Port * 4) + e.Index;

            if ((uint)e.Device >= (uint)Device.Mouse && e.Id <= 1)
            {
                e.State = (short)(_inputCoords[i] >> (int)(e.Id * 16));
            }
            else
            {
                e.State = Convert.ToInt16((_inputButtons[i] & (1 << (int)e.Id)) != 0);
            }
        }
    }
}
