using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;

namespace DCCLocomotiveFactory
{
    [ServiceContract]
    public interface IDCCLocomotiveContract
    {
        /// <summary>
        /// Stops the locomotive
        /// </summary>
        [OperationContract]
        void Stop();

        /// <summary>
        /// Stop the locomotive for an emergency
        /// </summary>
        [OperationContract]
        void EmergencyStop();

        /// <summary>
        /// Gets the running direction of that locomotive
        /// </summary>
        /// <returns></returns>
        [OperationContract(IsOneWay=false)]
        Direction GetDirection();

        /// <summary>
        /// Changes the direction of that locomotive
        /// </summary>
        /// <param name="direction">New direction</param>
        [OperationContract]
        void ChangeDirection(Direction direction);

        /// <summary>
        /// Sets the new speed value
        /// </summary>
        /// <param name="speed">Speed value (0 - 28)</param>
        [OperationContract]
        void SetSpeed(byte speed);

        /// <summary>
        /// Gets the current locomotive speed
        /// </summary>
        /// <returns>Speed value</returns>
        [OperationContract(IsOneWay=false)]
        byte GetSpeed();

        /// <summary>
        /// Switch the main light
        /// </summary>
        /// <param name="state">ON if true, OFF if false</param>
        [OperationContract]
        void SwitchLight(bool state);

        /// <summary>
        /// Gets the main light status
        /// </summary>
        /// <returns>true if ON, false otherwise</returns>
        [OperationContract(IsOneWay=false)]
        bool GetLightState();

        /// <summary>
        /// Sets the state of a function
        /// </summary>
        /// <param name="idx">Function number (1 - 8)</param>
        /// <param name="state">ON if true, OFF if false</param>
        [OperationContract]
        void SetFunction(uint idx, bool state);

        /// <summary>
        /// Gets the state of a function
        /// </summary>
        /// <param name="idx">Function number (1 - 8)</param>
        /// <returns>true if ON, false otherwise</returns>
        [OperationContract(IsOneWay=false)]
        bool GetFunction(uint idx);
    }


    /// <summary>
    /// Implements the IDCCLocomotiveContract
    /// </summary>
    [ServiceBehavior(InstanceContextMode=InstanceContextMode.Single)]
    class DCCLocomotiveService : IDCCLocomotiveContract, IDisposable
    {
        const int
            MinFunction = 1,
            FunctionNumber = 8;
        const byte
            StopSpeed = 0,
            EmergencyStopSpeed = 1,
            MaxSpeed = 28,
            MinSpeed = 0;

        protected byte m_speed = 0;
        protected bool m_light = false;
        protected Direction m_direction = Direction.Forward;
        protected bool[] m_functions = new bool[FunctionNumber] { false, false, false, false, false, false, false, false };
 
        #region IDCCLocomotiveContract Members

        public void Stop()
        {
            m_speed = StopSpeed;
        }

        public void EmergencyStop()
        {
            m_speed = EmergencyStopSpeed;
        }

        public Direction GetDirection()
        {
            return m_direction;
        }

        public void ChangeDirection(Direction direction)
        {
            m_direction = direction;
        }

        public void SetSpeed(byte speed)
        {
            if (speed >= MinSpeed && speed <= MaxSpeed)
                m_speed = speed;
        }

        public byte GetSpeed()
        {
            return m_speed;
        }

        public void SwitchLight(bool state)
        {
            m_light = state;
        }

        public bool GetLightState()
        {
            return m_light;
        }

        public void SetFunction(uint idx, bool state)
        {
            if (idx >= MinFunction && idx <= FunctionNumber)
                m_functions[idx - 1] = state;
        }

        public bool GetFunction(uint idx)
        {
            if (!(idx >= MinFunction && idx <= FunctionNumber))
                throw new IndexOutOfRangeException();

            return m_functions[idx - 1];
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            m_speed = 0;
            m_light = false;
        }

        #endregion
    }

    /// <summary>
    /// Direction enumeration
    /// </summary>
    public enum Direction : byte
    {
        Reverse = 0,
        Forward
    }


    class Constants
    {
        public const string
            LocoServerBaseAddress = "net.tcp://localhost:80{0}/LocServer/";

        public class ColumnName
        {
            public const string
                Name = "Name",
                Address = "Address";
        }
    }
}
