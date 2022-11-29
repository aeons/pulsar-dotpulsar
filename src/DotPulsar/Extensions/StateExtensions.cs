/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace DotPulsar.Extensions;

using DotPulsar.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Extensions for IState.
/// </summary>
public static class StateExtensions
{
    /// <summary>
    /// Wait for the state to change to a specific state with a delay.
    /// </summary>
    /// <returns>
    /// The current state.
    /// </returns>
    /// <remarks>
    /// If the state change to a final state, then all awaiting tasks will complete.
    /// </remarks>
    public static async ValueTask<TState> OnStateChangeTo<TState>(
        this IState<TState> stateChanged,
        TState state,
        TimeSpan delay,
        CancellationToken cancellationToken = default) where TState : notnull
    {
        while (true)
        {
            var currentState = await stateChanged.OnStateChangeTo(state, cancellationToken).ConfigureAwait(false);
            if (stateChanged.IsFinalState(currentState))
                return currentState;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(delay);

            try
            {
                currentState = await stateChanged.OnStateChangeFrom(state, cts.Token).ConfigureAwait(false);
                if (stateChanged.IsFinalState(currentState))
                    return currentState;
            }
            catch (OperationCanceledException)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw;

                return state;
            }
        }
    }

    /// <summary>
    /// Wait for the state to change from a specific state with a delay.
    /// </summary>
    /// <returns>
    /// The current state.
    /// </returns>
    /// <remarks>
    /// If the state change to a final state, then all awaiting tasks will complete.
    /// </remarks>
    public static async ValueTask<TState> OnStateChangeFrom<TState>(
        this IState<TState> stateChanged,
        TState state,
        TimeSpan delay,
        CancellationToken cancellationToken = default) where TState : notnull
    {
        while (true)
        {
            var currentState = await stateChanged.OnStateChangeFrom(state, cancellationToken).ConfigureAwait(false);
            if (stateChanged.IsFinalState(currentState))
                return currentState;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(delay);

            try
            {
                currentState = await stateChanged.OnStateChangeTo(state, cts.Token).ConfigureAwait(false);
                if (stateChanged.IsFinalState(currentState))
                    return currentState;
            }
            catch (OperationCanceledException)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw;

                return state;
            }
        }
    }
}
