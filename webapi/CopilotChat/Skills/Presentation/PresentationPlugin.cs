using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.SemanticKernel.Orchestration;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace CopilotChat.Skills.Presentation
{
    public class PresentationPlugin 
    {
        private static ConcurrentBag<string> _feedbackList = new ConcurrentBag<string>();

        [SKFunction("Any questions will be stored and can be answered later. Use this tool to store the questions or subjects the user is interested about. Don't answer anything yourself.")]
        [SKFunctionContextParameter(Name = "userfeedback", Description = "The feedback or expectation the user stated about the upcoming presentation")]
        public string ProvideFeedback(SKContext context)
        {
            string feedback = context["userfeedback"];

            var summary = GetFeedbackKeywords(feedback, context);

            _feedbackList.Add(summary);
            return $"Answer with this text: Thank you for your feedback. It has been stored. These are the keywords: {summary}";
        }

        [SKFunction("Only use this tool when the user specifically asks for the result, e.g. 'what is the result'")]
        public string SummarizeFeedback(SKContext context)
        {
            if (!_feedbackList.Any())
            {
                return "No feedback has been given yet.";
            }

            var feedbacks = $"Feedback summary: {string.Join(", ", _feedbackList)}";
            var summary = GetFeedbackSummary(feedbacks, context);

            return $"That there have been {_feedbackList.Count} pieces of feedback so far. The topics covered in these feedbacks are as follows: {summary}";
        }

        private string GetFeedbackSummary(string feedback, SKContext? context = null)
        {
            if (feedback.Length > 1000) {
                feedback = feedback.Substring(0, 1000);
            }
            ISKFunction summerizer = context.Func("SummerizePlugin", "summerize");
            var result = summerizer.InvokeAsync(feedback).Result;
            return result.Result.ReplaceLineEndings(" ");
        }
        private string GetFeedbackKeywords(string feedback, SKContext? context = null)
        {
            ISKFunction summerizer = context.Func("SummerizePlugin", "createKeywords");
            var result = summerizer.InvokeAsync(feedback).Result;
            return result.Result.ReplaceLineEndings(" ");
        }
    }
}
