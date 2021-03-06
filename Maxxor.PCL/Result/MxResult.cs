﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Maxxor.PCL.Exceptions;
using Maxxor.PCL.Result.Interfaces;

namespace Maxxor.PCL.Result
{
    /// <summary>
    /// Used to wrap method return values so that calling methods always have a consistent 
    /// way to determine if the called method achieved its intended result and proceed accordingly 
    /// - without having to try and catch exceptions everywhere. 
    /// </summary>
    public  class MxResult
    {

        #region Properties

        /// <summary>
        /// The called method achieved the expected result. No exceptions or undesirable events happened. 
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// The method did not succeed in its goal. Perhaps an exception was thrown, or a known 
        /// and expected deviation occured (eg incorrect password) that needs to be dealt with by the caller
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// If the Result IsFailure, it will have an Error attached to it containing details of the problem
        /// </summary>
        public MxError Error { get; }

        public string SuccessMessage { get; protected set; } = string.Empty;

        #endregion

        #region Ok

        /// <summary>
        /// Creates a new Result with IsSuccess true
        /// </summary>
        /// <returns></returns>
        public static MxResult Ok()
        {
            return new MxResult(true, null);
        }
        /// <summary>
        /// Creates a new Result with IsSuccess true and Value set
        /// </summary>
        /// <returns></returns>
        public static MxResult<T> Ok<T>(T value)
        {
            return new MxResult<T>(value, true, null);
        }

        #endregion

        #region WithSuccessMessage

        public MxResult WithSuccessMessage(string successMessage)
        {
            SuccessMessage = successMessage;
            return this;
        }

        #endregion


        #region Fail

        /// <summary>
        /// Creates a new Result from a received Result so that it can be updated and passed up the chain.
        /// Error stacktrace is updated from the sender
        /// </summary>
        /// <param name="sender">Class that is receiving the Result</param>
        /// <param name="previousResult">Result returned from called method</param>
        /// <param name="methodName">Populated automatically</param>
        /// <returns></returns>
        public static MxResult Fail(object sender, MxResult previousResult, [CallerMemberName] string methodName = "")
        {
            return Fail(MxError.Update(sender, previousResult.Error, methodName));
        }

        /// <summary>
        /// Creates a new Result with IsSuccess false and ErrorCondition set. 
        /// Exception can also be provided if there is one. 
        /// </summary>
        /// <param name="sender">The class in which the Error occured (this)</param>
        /// <param name="errorCondition">Contains error name and user-friendly message</param>
        /// <param name="exception">Optional</param>
        /// <param name="methodName">Automatic</param>
        /// <returns></returns>
        public static MxResult Fail(object sender, IMxErrorCondition errorCondition, Exception exception = null, [CallerMemberName] string methodName = "")
        {
            return Fail(MxError.Create(sender, errorCondition, exception, methodName));
        }

        /// <summary>
        /// Creates a new Result from a received Result so that it can be updated and passed up the chain.
        /// Error stacktrace is updated from the sender
        /// </summary>
        /// <param name="sender">Class that is receiving the Result</param>
        /// <param name="previousResult">Result returned from called method</param>
        /// <param name="methodName">Populated automatically</param>
        /// <returns></returns>
        public static MxResult<T> Fail<T>(object sender, MxResult previousResult, [CallerMemberName] string methodName = "")
        {
            return Fail<T>(MxError.Update(sender, previousResult.Error, methodName));
        }

        /// <summary>
        /// Creates a new Result with IsSuccess false and ErrorCondition set. 
        /// Exception can also be provided if there is one. 
        /// </summary>
        /// <param name="sender">The class in which the Error occured (this)</param>
        /// <param name="errorCondition">Contains error name and user-friendly message</param>
        /// <param name="exception">Optional</param>
        /// <param name="methodName">Automatic</param>
        /// <returns></returns>
        public static MxResult<T> Fail<T>(object sender, IMxErrorCondition errorCondition, Exception exception = null, [CallerMemberName] string methodName = "")
        {
            return Fail<T>(MxError.Create(sender, errorCondition, exception, methodName));
        }



        #endregion

        #region Return

        public static MxResult Return(object sender, MxResult previousResult)
        {
            return previousResult.IsSuccess
                ? Ok()
                : Fail(sender, previousResult);
        }

        public static MxResult<T> Return<T>(object sender, MxResult<T> previousResult)
        {
            return previousResult.IsSuccess
                ? Ok(previousResult.Value)
                : Fail<T>(sender, previousResult);
        }

        #endregion

        #region Comine

        /// <summary>
        /// Checks all values for failures
        /// </summary>
        /// <param name="sender">this</param>
        /// <param name="results"></param>
        /// <returns>Success if all are success else fail with error updated with all failures in order received</returns>
        public static MxResult Combine(object sender, IEnumerable<MxResult> results, [CallerMemberName] string methodName = "")
        {
            if(sender.GetType() == typeof(MxResult))
            {
                throw new MxInvalidSenderException();    
            }

            var successMessage = string.Empty;
            foreach (var result in results)
            {
                if (result.IsFailure)
                {
                    return Fail(sender, result);
                }
                if (result.IsSuccess && !string.IsNullOrEmpty(result.SuccessMessage))
                {
                    successMessage = result.SuccessMessage;
                }
            }

            return Ok()
                .WithSuccessMessage(successMessage);
        }

        /// <summary>
        /// Checks all Results in the list and returns Fail the first time one returns Fail
        /// and return a list of values if all Results succeed
        /// </summary>
        /// <typeparam name="T">Type of object returned by the Results in the list</typeparam>
        /// <param name="sender">this</param>
        /// <param name="results">List of Results of Type T</param>
        /// <returns>List of Result values if all Succeed else Fail</returns>
        public static MxResult<List<T>> Combine<T>(object sender, IEnumerable<MxResult<T>> results, [CallerMemberName] string methodName = "")
        {
            if (sender.GetType() == typeof(MxResult) || sender.GetType() == typeof(MxResult<T>))
            {
                throw new MxInvalidSenderException();
            }

            var successMessage = string.Empty;
            var values = new List<T>();
            foreach (var result in results)
            {
                if (result.IsFailure)
                {
                    return Fail<List<T>>(sender, result);
                }
                if (!string.IsNullOrEmpty(result.SuccessMessage))
                {
                    successMessage = result.SuccessMessage;
                }
                values.Add(result.Value);
            }
            return Ok(values).WithSuccessMessage(successMessage);
        }

        /// <summary>
        /// Checks all values for failures
        /// </summary>
        /// <param name="sender">this</param>
        /// <param name="results"></param>
        /// <returns>Success if all are success else fail with error updated with all failures in order received</returns>
        public static MxResult Combine(object sender, params MxResult[] results)
        {
            if (sender.GetType() == typeof(MxResult) || sender.GetType() == typeof(MxResult<>) || sender is ICollection)
            {
                throw new MxInvalidSenderException();
            }
            var successMessage = string.Empty;
            foreach (var result in results)
            {
                if (result.IsFailure)
                {
                    return Fail(sender, result);
                }
                if (!string.IsNullOrEmpty(result.SuccessMessage))
                {
                    successMessage = result.SuccessMessage;
                }
            }
            return Ok()
                .WithSuccessMessage(successMessage);
        }


        #endregion

        #region Privates

        protected MxResult(bool isSuccess, MxError error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        protected static MxResult Fail(MxError error)
        {
            return new MxResult(false, error);
        }

        protected static MxResult<T> Fail<T>(MxError error)
        {
            return new MxResult<T>(default(T), false, error);
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            if (IsFailure)
            {
                return "FAIL: " + Error;
            }
            else
            {
                return "OK";
            }
        }

        #endregion

    }

    #region ResultOfType

    /// <summary>
    /// Used to wrap method return values so that calling methods always have a consistent 
    /// way to determine if the called method achieved its intended result and proceed accordingly 
    /// - without having to try and catch exceptions everywhere. 
    /// </summary>
    public class MxResult<T> : MxResult
    {
        public T Value { get; }
        public MxResult(T value, bool isSuccess, MxError error) : base(isSuccess, error)
        {
            Value = value;
        }

        #region WithSuccessMessage

        public new MxResult<T> WithSuccessMessage(string successMessage)
        {
            SuccessMessage = successMessage;
            return this;
        }

        #endregion
    }

    #endregion

}