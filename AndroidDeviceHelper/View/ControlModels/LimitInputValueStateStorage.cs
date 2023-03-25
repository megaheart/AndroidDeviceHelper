using System;
using System.Collections.Generic;
using System.Text;

namespace AndroidDeviceHelper
{
    public class InputValueState<T>
    {
        public InputValueState(T value, int caretIndex)
        {
            Value = value;
            CaretIndex = caretIndex;
        }
        public T Value { get; private set; }
        public int CaretIndex { get; private set; }
    }
    /// <summary>
    /// Ease undo and redo proccess with memory limition
    /// </summary>
    /// <typeparam name="T">Type of main (value) property</typeparam>
    public class LimitInputValueStateStorage<T>
    {
        private int currentIndex = 0;
        private int lastestIndex = 0;
        private InputValueState<T>[] states;
        /// <summary>
        /// Maximum states' amount storage can save.
        /// </summary>
        public int Limit
        {
            get => states.Length;
        }
        /// <exception cref="Exception">
        /// throw if set limit less than 2
        /// </exception>
        public LimitInputValueStateStorage(int limit)
        {
            if (limit < 2) throw new Exception("Limit parameter must be more than 1");
            states = new InputValueState<T>[limit];
        }
        ~LimitInputValueStateStorage()
        {
            states = null;
        }
        /// <summary>
        /// Save last state before undo
        /// </summary>
        /// <exception cref="Exception">
        /// throw if use this funtion when current state is not lastest state
        /// </exception>
        public bool SaveLatestState(T value, int caretIndex)
        {
            if (currentIndex != lastestIndex) return false;
            states[currentIndex] = new InputValueState<T>(value, caretIndex);
            return true;
        }
        public void SaveLastState(T value, int caretIndex)
        {
            if (currentIndex != lastestIndex) states[currentIndex] = null;
            states[currentIndex] = new InputValueState<T>(value, caretIndex);
            currentIndex++;
            currentIndex %= states.Length;
            lastestIndex = currentIndex;
            states[currentIndex] = null;
        }
        /// <summary>
        /// Check whether previous state exists
        /// </summary>
        /// <returns>
        /// True if previous state exists, otherwise false
        /// </returns>
        public bool HasPreviousState()
        {
            if (states[lastestIndex] == null) throw new Exception("Save Latest State before use this function");
            var _currentIndex = currentIndex + states.Length - 1;
            _currentIndex %= states.Length;
            return _currentIndex != lastestIndex && states[_currentIndex] != null;
        }
        /// <summary>
        /// Please use <seealso cref="HasPreviousState()"/> before to avoid unforeseeable result
        /// </summary>
        public InputValueState<T> GetPreviousState()
        {
            currentIndex += states.Length - 1;
            currentIndex %= states.Length;
            return states[currentIndex];
        }
        /// <summary>
        /// Check whether following state exists
        /// </summary>
        /// <returns>
        /// True if following state exists, otherwise false
        /// </returns>
        public bool HasFollowingState()
        {
            return currentIndex != lastestIndex;
        }
        /// <summary>
        /// Please use <seealso cref="HasFollowingState()"/> before to avoid unforeseeable result
        /// </summary>
        public InputValueState<T> GetFollowingState()
        {
            currentIndex++;
            currentIndex %= states.Length;
            return states[currentIndex];
        }
    }
}
