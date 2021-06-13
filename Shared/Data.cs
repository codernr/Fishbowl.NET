using System.Collections.Generic;
using System.Linq;
using Fishbowl.Net.Shared.Collections;

namespace Fishbowl.Net.Shared.Data
{
    public record RandomEnumeratorData<T>(T? Current, IEnumerable<T> List, IEnumerable<T> Stack);

    public static class DataExtensions
    {
        public static RandomEnumeratorData<T> Map<T>(this RandomEnumerator<T> enumerator) =>
            new(enumerator.Current, enumerator.List, enumerator.Stack);

        public static RandomEnumerator<T> Map<T>(this RandomEnumeratorData<T> data) where T : class
        {
            if (data.Current is null)
            {
                return new(data.List);
            }

            var current = data.List.First(item => item == data.Current);
            var stack = new Stack<T>(data.Stack.Select(item => data.List.First(listItem => listItem == item)));

            return new(current, stack, data.List.ToList());
        }
    }
}