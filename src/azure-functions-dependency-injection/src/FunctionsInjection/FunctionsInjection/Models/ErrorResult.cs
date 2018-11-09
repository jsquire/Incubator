using System;
using System.Collections.Generic;

namespace FunctionsInjection.Models
{
    public class ErrorResult
    {
        public string MemberName { get; }
        public IEnumerable<string> Messages { get; }

        public ErrorResult(string memberName, IEnumerable<string> messages)
        {
            this.MemberName = memberName ?? throw new ArgumentNullException(nameof(memberName));
            this.Messages   = messages   ?? throw new ArgumentNullException(nameof(messages));
        }
    }
}
