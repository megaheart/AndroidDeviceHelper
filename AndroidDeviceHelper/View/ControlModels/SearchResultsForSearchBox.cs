using System;
using System.Collections.Generic;
using System.Text;

namespace AndroidDeviceHelper.Models
{
    public class SearchResultsForSearchBox
    {
        public static SearchResultsForSearchBox Fail()
        {
            return new SearchResultsForSearchBox() { IsSuccessful = false, _results = null };
        }
        public static SearchResultsForSearchBox Success(IEnumerable<object> results, int indexOfTheSameResultAsSearchInput)
        {
            return new SearchResultsForSearchBox() { IsSuccessful = true, _results = results, _indexOfTheSameResultAsSearchInput = indexOfTheSameResultAsSearchInput };
        }
        private SearchResultsForSearchBox() { }
        public bool IsSuccessful { get; private set; }
        private int _indexOfTheSameResultAsSearchInput;
        /// <summary>
        /// -1 if no elements is identical as search input
        /// </summary>
        public int IndexOfTheSameResultAsSearchInput
        {
            get
            {
                if (IsSuccessful)
                {
                    return _indexOfTheSameResultAsSearchInput;
                }
                else throw new AccessViolationException("The search process fails and it isn't able to return results.");
            }
        }
        private IEnumerable<object> _results;
        public IEnumerable<object> Results
        {
            get
            {
                if (IsSuccessful)
                {
                    return _results;
                }
                else throw new AccessViolationException("The search process fails and it isn't able to return results.");
            }
        }

    }
}
